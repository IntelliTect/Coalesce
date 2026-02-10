data "azurerm_client_config" "current" {}

# App registration for GitHub Actions CI/CD
resource "azuread_application" "ci" {
  display_name = "${var.project_name}-ci"
}

resource "azuread_service_principal" "ci" {
  client_id = azuread_application.ci.client_id
}

# Federated credentials for GitHub OIDC - one per environment
resource "azuread_application_federated_identity_credential" "github_env" {
  for_each = var.github_environments

  application_id = azuread_application.ci.id
  display_name   = "github-${each.key}"
  description    = "GitHub Actions OIDC for ${each.key} environment"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.github_repository}:environment:${each.value}"
}

# Also allow pushes from the main branch (build job runs outside an environment)
resource "azuread_application_federated_identity_credential" "github_branch" {
  application_id = azuread_application.ci.id
  display_name   = "github-branch-main"
  description    = "GitHub Actions OIDC for main branch"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.github_repository}:ref:refs/heads/main"
}

# AcrPush so CI can push container images
resource "azurerm_role_assignment" "acr_push" {
  scope                = var.acr_id
  role_definition_name = "AcrPush"
  principal_id         = azuread_service_principal.ci.object_id
}

# Contributor on each Container App resource group so CI can deploy
resource "azurerm_role_assignment" "container_app_contributor" {
  for_each = var.environment_resource_group_ids

  scope                = each.value
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.ci.object_id
}
