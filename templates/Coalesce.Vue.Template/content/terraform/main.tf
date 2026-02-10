# Shared resource group - created by bootstrap
data "azurerm_resource_group" "shared" {
  name = "${var.project_name}-shared-rg"
}

# Shared container registry
module "acr" {
  source = "./modules/container_registry"

  name                = replace("${var.project_name}acr", "-", "")
  location            = data.azurerm_resource_group.shared.location
  resource_group_name = data.azurerm_resource_group.shared.name
  sku                 = "Basic"
  tags                = var.tags

  pull_identity_principal_ids = {
    dev  = module.dev.identity_principal_id
    prod = module.prod.identity_principal_id
  }
}

# ============================================================
# Shared identity for CI build (main branch, outside environments)
# ============================================================
resource "azurerm_user_assigned_identity" "ci_build" {
  name                = "${var.project_name}-ci-build"
  resource_group_name = data.azurerm_resource_group.shared.name
  location            = var.location
}

resource "azurerm_federated_identity_credential" "github_branch" {
  name                = "github-branch-main"
  resource_group_name = data.azurerm_resource_group.shared.name
  parent_id           = azurerm_user_assigned_identity.ci_build.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:ref:refs/heads/main"
}

resource "azurerm_role_assignment" "acr_push_build" {
  scope                = module.acr.id
  role_definition_name = "AcrPush"
  principal_id         = azurerm_user_assigned_identity.ci_build.principal_id
}

# ============================================================
# Dev Environment
# ============================================================
module "dev" {
  source = "./modules/environment"

  project_name     = var.project_name
  environment_name = "dev"
  location         = var.location

  # Networking
  vnet_address_space           = "10.0.0.0/16"
  container_apps_subnet_prefix = "10.0.8.0/21"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_app_cpu               = 0.5
  container_app_memory            = "1Gi"
  container_app_min_replicas      = 0
  container_app_max_replicas      = 1

  # SQL - Basic tier for dev
  sql_sku_name = "Basic"

  # Storage
  storage_replication_type = "LRS"

  # CI/CD
  github_repository  = var.github_repository
  github_environment = "dev"

  tags = var.tags
}

# ============================================================
# Prod Environment
# ============================================================
module "prod" {
  source = "./modules/environment"

  project_name     = var.project_name
  environment_name = "prod"
  location         = var.location

  # Networking
  vnet_address_space           = "10.1.0.0/16"
  container_apps_subnet_prefix = "10.1.8.0/21"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_app_cpu               = 1.0
  container_app_memory            = "2Gi"
  container_app_min_replicas      = 1
  container_app_max_replicas      = 1

  # SQL - S1 tier for prod
  sql_sku_name = "S1"

  # Storage
  storage_replication_type = "GRS"

  # CI/CD
  github_repository  = var.github_repository
  github_environment = "prod"

  tags = var.tags
}
