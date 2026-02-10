# Shared resource group for cross-environment resources (ACR)
module "shared_rg" {
  source = "./modules/resource_group"

  name     = "${var.project_name}-shared-rg"
  location = var.location
  tags     = var.tags
}

# Shared container registry
module "acr" {
  source = "./modules/container_registry"

  name                = replace("${var.project_name}acr", "-", "")
  location            = module.shared_rg.location
  resource_group_name = module.shared_rg.name
  sku                 = "Basic"
  tags                = var.tags

  pull_identity_principal_ids = {
    dev  = module.dev.identity_principal_id
    prod = module.prod.identity_principal_id
  }
}

# ============================================================
# CI/CD Identity (GitHub Actions OIDC)
# ============================================================
module "ci_identity" {
  source = "./modules/ci_identity"

  project_name      = var.project_name
  github_repository = var.github_repository
  acr_id            = module.acr.id

  environment_resource_group_ids = {
    dev  = module.dev.resource_group_id
    prod = module.prod.resource_group_id
  }
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
  container_apps_subnet_prefix = "10.0.0.0/23"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_image_name            = var.project_name
  container_image_tag             = var.container_image_tag
  container_app_cpu               = 0.5
  container_app_memory            = "1Gi"
  container_app_min_replicas      = 0
  container_app_max_replicas      = 1

  # SQL - Basic tier for dev
  sql_sku_name = "Basic"

  # Storage
  storage_replication_type = "LRS"

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
  container_apps_subnet_prefix = "10.1.0.0/23"

  # Container App
  container_registry_login_server = module.acr.login_server
  container_image_name            = var.project_name
  container_image_tag             = var.container_image_tag
  container_app_cpu               = 1.0
  container_app_memory            = "2Gi"
  container_app_min_replicas      = 1
  container_app_max_replicas      = 1

  # SQL - S1 tier for prod
  sql_sku_name = "S1"

  # Storage
  storage_replication_type = "GRS"

  tags = var.tags
}
