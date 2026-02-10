locals {
  tags = merge(var.tags, {
    environment = var.environment_name
  })
}

resource "azurerm_resource_group" "this" {
  name     = "${var.project_name}-${var.environment_name}-rg"
  location = var.location
  tags     = local.tags
}

locals {
  context = {
    project_name        = var.project_name
    environment_name    = var.environment_name
    location            = azurerm_resource_group.this.location
    resource_group_name = azurerm_resource_group.this.name
    tags                = local.tags
  }
}

resource "azurerm_user_assigned_identity" "app" {
  name                = "${local.context.project_name}-${local.context.environment_name}-app-id"
  location            = local.context.location
  resource_group_name = local.context.resource_group_name
  tags                = local.context.tags
}

# Federated credential for CI identity to deploy via this GitHub Environment
resource "azurerm_federated_identity_credential" "ci_deploy" {
  name                = "github-environment-${var.environment_name}"
  resource_group_name = local.context.resource_group_name
  parent_id           = var.ci_identity_id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:environment:${var.environment_name}"
}

module "vnet" {
  source = "../vnet"

  context                      = local.context
  address_space                = var.vnet_address_space
  container_apps_subnet_prefix = var.container_apps_subnet_prefix
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
  identity_principal_id = azurerm_user_assigned_identity.app.principal_id
  allowed_subnet_ids    = [module.vnet.container_apps_subnet_id]
}

module "sql" {
  source = "../sql"

  context             = local.context
  sku_name            = var.sql_sku_name
  aad_admin_login     = azurerm_user_assigned_identity.app.client_id
  aad_admin_object_id = azurerm_user_assigned_identity.app.principal_id
  subnet_id           = module.vnet.container_apps_subnet_id
}

module "key_vault" {
  source = "../key_vault"

  context               = local.context
  identity_principal_id = azurerm_user_assigned_identity.app.principal_id
  allowed_subnet_ids    = [module.vnet.container_apps_subnet_id]
  secrets               = var.additional_secrets
}

module "container_app" {
  source = "../container_app"

  context                         = local.context
  log_analytics_workspace_id      = module.app_insights.log_analytics_workspace_id
  subnet_id                       = module.vnet.container_apps_subnet_id
  identity_id                     = azurerm_user_assigned_identity.app.id
  container_registry_login_server = var.container_registry_login_server
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
        value = azurerm_user_assigned_identity.app.client_id
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
