data "azurerm_client_config" "current" {}

# User-assigned managed identity for GitHub Actions CI/CD
resource "azurerm_user_assigned_identity" "ci" {
  name                = "${var.context.project_name}-${var.context.environment_name}-deploy-id"
  resource_group_name = var.context.resource_group_name
  location            = var.context.location
  tags                = var.context.tags
}

# Federated credential for GitHub OIDC - tied to specific environment
resource "azurerm_federated_identity_credential" "github" {
  name                = "github-${var.context.environment_name}"
  resource_group_name = var.context.resource_group_name
  parent_id           = azurerm_user_assigned_identity.ci.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:environment:${var.github_environment}"
}

# Contributor on environment resource group so CI can deploy
resource "azurerm_role_assignment" "container_app_contributor" {
  scope                = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.context.resource_group_name}"
  role_definition_name = "Contributor"
  principal_id         = azurerm_user_assigned_identity.ci.principal_id
}
