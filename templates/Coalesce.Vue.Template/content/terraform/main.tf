locals {
  tags = merge({
    project   = var.project_name
    managedBy = "terraform"
  }, var.tags)
}

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
  tags                = local.tags
}

# ============================================================
# CI identity - created by bootstrap, referenced here for ACR access
# ============================================================
data "azurerm_user_assigned_identity" "ci" {
  name                = "${var.project_name}-ci"
  resource_group_name = data.azurerm_resource_group.shared.name
}

resource "azurerm_role_assignment" "ci_acr_push" {
  scope                = module.acr.id
  role_definition_name = "AcrPush"
  principal_id         = data.azurerm_user_assigned_identity.ci.principal_id
}

# ============================================================
# Dev Environment
# ============================================================
module "dev" {
  source = "./modules/environment"

  project_name     = var.project_name
  environment_name = "dev"
  location         = var.location

  # CI/CD
  github_repository = var.github_repository
  ci_identity_id    = data.azurerm_user_assigned_identity.ci.id

  # Networking
  vnet_address_space           = "10.0.0.0/16"
  container_apps_subnet_prefix = "10.0.8.0/21"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_registry_id           = module.acr.id
  container_app_cpu               = 0.5
  container_app_memory            = "1Gi"
  container_app_min_replicas      = 0
  container_app_max_replicas      = 1

  # SQL - Basic tier for dev
  sql_sku_name = "Basic"

  # Storage
  storage_replication_type = "LRS"

  tags = local.tags
}

# ============================================================
# Prod Environment
# ============================================================
module "prod" {
  source = "./modules/environment"

  project_name     = var.project_name
  environment_name = "prod"
  location         = var.location

  # CI/CD
  github_repository = var.github_repository
  ci_identity_id    = data.azurerm_user_assigned_identity.ci.id

  # Networking
  vnet_address_space           = "10.1.0.0/16"
  container_apps_subnet_prefix = "10.1.8.0/21"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_registry_id           = module.acr.id
  container_app_cpu               = 1.0
  container_app_memory            = "2Gi"
  container_app_min_replicas      = 1
  container_app_max_replicas      = 1

  # SQL - S1 tier for prod
  sql_sku_name = "S1"

  # Storage
  storage_replication_type = "GRS"

  tags = local.tags
}
