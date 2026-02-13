locals {
  tags = merge({
    project   = var.project_name
    managedBy = "terraform"
  }, var.tags)

  # TODO: Update with user-friendly name of application
  # display_name = "My Awesome App"
  display_name = var.project_name

  context = {
    project_name      = var.project_name
    location          = var.location
    tags              = local.tags
    github_repository = var.github_repository
    ci_identity_id    = data.azurerm_user_assigned_identity.ci.id
  }
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
  name                = "${var.project_name}-ci-identity"
  resource_group_name = data.azurerm_resource_group.shared.name
}

resource "azurerm_role_assignment" "ci_acr_push" {
  scope                = module.acr.id
  role_definition_name = "AcrPush"
  principal_id         = data.azurerm_user_assigned_identity.ci.principal_id
}

# ============================================================
# Developers AAD Group - created by bootstrap
# ============================================================
data "azuread_group" "developers" {
  display_name = "${var.project_name}-developers"
}

# ============================================================
# Dev Environment
# ============================================================
module "dev" {
  source = "./modules/environment"

  context          = local.context
  display_name     = "${local.display_name} DEV"
  environment_name = "dev"
  allowed_origins  = ["localhost"]

  # Admin principals for dev resources
  admin_principals = {
    developers = data.azuread_group.developers.object_id
  }

  # Networking
  vnet_address_space           = "10.0.0.0/16"
  container_apps_subnet_prefix = "10.0.8.0/21"

  # Container App
  container_registry         = module.acr
  container_app_cpu          = 0.5
  container_app_memory       = "1Gi"
  container_app_min_replicas = 0
  container_app_max_replicas = 1

  # SQL - Basic tier for dev
  sql_sku_name = "Basic"

  # Key Vault
  purge_protection_enabled = false

  # Storage
  storage_replication_type = "LRS"
}

# ============================================================
# Prod Environment
# ============================================================
module "prod" {
  source = "./modules/environment"

  context          = local.context
  display_name     = local.display_name
  environment_name = "prod"
  allowed_origins  = [] # E.g. ["myapp.com"]

  # Networking
  vnet_address_space           = "10.1.0.0/16"
  container_apps_subnet_prefix = "10.1.8.0/21"

  # Container App
  container_registry         = module.acr
  container_app_cpu          = 1.0
  container_app_memory       = "2Gi"
  container_app_min_replicas = 1
  container_app_max_replicas = 1

  # SQL - S1 tier for prod
  sql_sku_name = "S1"

  # Key Vault
  purge_protection_enabled = true

  # Storage
  storage_replication_type = "GRS"
}
