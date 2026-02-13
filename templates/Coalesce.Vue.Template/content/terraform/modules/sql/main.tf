data "azurerm_client_config" "current" {}

# Uncomment this section to use an AAD group as the SQL administrator, allowing
# multiple principals (e.g. developers) to have admin access. Requires granting
# the CI identity the Group.Create permission - see README for instructions.
# resource "azuread_group" "sql_admins" {
#   display_name     = "${var.context.project_name}-${var.context.environment_name}-sql-admins"
#   security_enabled = true
# }
# resource "azuread_group_member" "sql_admins" {
#   for_each = var.admin_principals
#
#   group_object_id  = azuread_group.sql_admins.object_id
#   member_object_id = each.value
# }

resource "azurerm_mssql_server" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-sql"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  version             = "12.0"
  minimum_tls_version = "1.2"
  tags                = var.context.tags

  azuread_administrator {
    tenant_id                   = data.azurerm_client_config.current.tenant_id
    azuread_authentication_only = true
    login_username              = var.admin_principals["app"]
    object_id                   = var.admin_principals["app"]
    # To use the AAD group instead:
    # login_username              = azuread_group.sql_admins.display_name
    # object_id                   = azuread_group.sql_admins.object_id
  }
}

resource "azurerm_mssql_database" "this" {
  name      = var.context.project_name
  server_id = azurerm_mssql_server.this.id
  sku_name  = var.sku_name
  tags      = var.context.tags
}

resource "azurerm_mssql_virtual_network_rule" "this" {
  name      = "allow-container-apps"
  server_id = azurerm_mssql_server.this.id
  subnet_id = var.subnet_id
}
