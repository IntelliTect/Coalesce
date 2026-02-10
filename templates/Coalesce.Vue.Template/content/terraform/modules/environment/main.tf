locals {
  tags = merge(var.tags, {
    environment = var.environment_name
  })
}

module "resource_group" {
  source = "../resource_group"

  name     = "${var.project_name}-${var.environment_name}-rg"
  location = var.location
  tags     = local.tags
}

locals {
  context = {
    project_name        = var.project_name
    environment_name    = var.environment_name
    location            = module.resource_group.location
    resource_group_name = module.resource_group.name
    tags                = local.tags
  }
}

module "vnet" {
  source = "../vnet"

  context                      = local.context
  address_space                = var.vnet_address_space
  container_apps_subnet_prefix = var.container_apps_subnet_prefix
}

module "identity" {
  source = "../identity"

  context = local.context
}

module "app_insights" {
  source = "../app_insights"

  context           = local.context
  retention_in_days = var.log_retention_in_days
}

module "storage" {
  source = "../storage"

  context               = local.context
  container_name        = "data"
  replication_type      = var.storage_replication_type
  identity_principal_id = module.identity.principal_id
  allowed_subnet_ids    = [module.vnet.container_apps_subnet_id]
}

module "sql" {
  source = "../sql"

  context             = local.context
  sku_name            = var.sql_sku_name
  aad_admin_login     = module.identity.client_id
  aad_admin_object_id = module.identity.principal_id
  subnet_id           = module.vnet.container_apps_subnet_id
}

module "key_vault" {
  source = "../key_vault"

  context               = local.context
  identity_principal_id = module.identity.principal_id
  allowed_subnet_ids    = [module.vnet.container_apps_subnet_id]
  secrets               = var.additional_secrets
}

module "container_app" {
  source = "../container_app"

  context                         = local.context
  log_analytics_workspace_id      = module.app_insights.log_analytics_workspace_id
  subnet_id                       = module.vnet.container_apps_subnet_id
  identity_id                     = module.identity.id
  container_registry_login_server = var.container_registry_login_server
  container_image_name            = var.container_image_name
  container_image_tag             = var.container_image_tag
  cpu                             = var.container_app_cpu
  memory                          = var.container_app_memory
  min_replicas                    = var.container_app_min_replicas
  max_replicas                    = var.container_app_max_replicas

  secrets = []

  env_vars = concat(
    [
      {
        name  = "ConnectionStrings__DefaultConnection"
        value = module.sql.connection_string
      },
      {
        name  = "AZURE_CLIENT_ID"
        value = module.identity.client_id
      },
      {
        name  = "ASPNETCORE_FORWARDEDHEADERS_ENABLED"
        value = "true"
      }
    ],
    var.app_insights_connection_string != null ? [
      {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.app_insights_connection_string
      }
    ] : [],
    var.additional_env_vars
  )
}
