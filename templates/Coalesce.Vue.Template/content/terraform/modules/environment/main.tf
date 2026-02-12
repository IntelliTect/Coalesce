locals {
  tags = merge(var.tags, {
    environment = var.environment_name
  })

  context = {
    project_name        = var.project_name
    environment_name    = var.environment_name
    location            = azurerm_resource_group.this.location
    resource_group_name = azurerm_resource_group.this.name
    tags                = local.tags
  }
}

resource "azurerm_resource_group" "this" {
  name     = "${var.project_name}-${var.environment_name}-rg"
  location = var.location
  tags     = local.tags
}

# The identity that the container app runs as
resource "azurerm_user_assigned_identity" "app" {
  name                = "${local.context.project_name}-${local.context.environment_name}-app-identity"
  location            = local.context.location
  resource_group_name = local.context.resource_group_name
  tags                = local.context.tags
}

# Give the container app permission to pull from the container registry.
resource "azurerm_role_assignment" "acr_pull" {
  scope                = var.container_registry.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.app.principal_id
}

# Federated credential for CI identity to deploy via this GitHub Environment
resource "azurerm_federated_identity_credential" "ci_deploy" {
  name                = "github-environment-${var.environment_name}"
  resource_group_name = regex("/resourceGroups/([^/]+)/", var.ci_identity_id)[0]
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

#if (MicrosoftAuth)
module "app_registration" {
  source = "../app_registration"

  display_name = var.display_name
  redirect_origins = concat(var.allowed_origins, [
    module.container_app.fqdn
  ])
}
#endif

#if (AppInsights)
module "app_insights" {
  source = "../app_insights"

  context           = local.context
  retention_in_days = 30
}
#endif

#if (BlobStorage)
module "storage" {
  source = "../storage"

  context          = local.context
  container_name   = "data"
  replication_type = var.storage_replication_type
  blob_contributors = {
    app = azurerm_user_assigned_identity.app.principal_id
  }
}
#endif

module "sql" {
  source = "../sql"

  context             = local.context
  sku_name            = var.sql_sku_name
  aad_admin_login     = azurerm_user_assigned_identity.app.client_id
  aad_admin_object_id = azurerm_user_assigned_identity.app.principal_id
  subnet_id           = module.vnet.container_apps_subnet_id
}

#if (AIChat)
module "ai_services" {
  source = "../ai_services"

  context = local.context
  allowed_users = {
    app = azurerm_user_assigned_identity.app.principal_id
  }
}
#endif

#if (KeyVault)
module "key_vault" {
  source = "../key_vault"

  context = local.context
  secrets_users = {
    app = azurerm_user_assigned_identity.app.principal_id
  }
  secrets = merge(
    var.additional_secrets,
    {
      #if (AIChat)
      "ConnectionStrings--OpenAI" = module.ai_services.connection_string
      #endif
      #if (MicrosoftAuth)
      "Authentication--Microsoft--ClientId"     = module.app_registration.client_id
      "Authentication--Microsoft--ClientSecret" = module.app_registration.client_secret
      #endif
    },
  )
}
#endif

#if (EmailAzure)
module "acs" {
  source = "../acs"

  context               = local.context
  identity_principal_id = azurerm_user_assigned_identity.app.principal_id
}
#endif

module "container_app" {
  source = "../container_app"

  context = local.context
  #if (AppInsights)
  log_analytics_workspace_id = module.app_insights.log_analytics_workspace_id
  #endif
  subnet_id          = module.vnet.container_apps_subnet_id
  identity_id        = azurerm_user_assigned_identity.app.id
  container_registry = var.container_registry
  cpu                = var.container_app_cpu
  memory             = var.container_app_memory
  min_replicas       = var.container_app_min_replicas
  max_replicas       = var.container_app_max_replicas

  env_vars = merge(
    {
      "ASPNETCORE_FORWARDEDHEADERS_ENABLED" = "true"
      "AZURE_CLIENT_ID"                     = azurerm_user_assigned_identity.app.client_id
      #if (AppInsights)
      "ConnectionStrings__AppInsights" = module.app_insights.connection_string
      #endif
      #if (BlobStorage)
      "ConnectionStrings__Blobs" = module.storage.blobs_connection_string
      #endif
      "ConnectionStrings__DefaultConnection" = module.sql.connection_string
      #if (KeyVault)
      "ConnectionStrings__KeyVault" = module.key_vault.vault_uri
      #endif
      #if (EmailAzure)
      "Communication__Azure__Endpoint"    = module.acs.endpoint
      "Communication__Azure__SenderEmail" = module.acs.sender_email
      #endif
    },
    var.env_vars
  )
}
