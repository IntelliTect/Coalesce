data "azurerm_client_config" "current" {}

resource "azurerm_mssql_server" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-sql"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  version             = "12.0"
  minimum_tls_version = "1.2"
  tags                = var.context.tags

  azuread_administrator {
    login_username              = var.aad_admin_login
    object_id                   = var.aad_admin_object_id
    tenant_id                   = data.azurerm_client_config.current.tenant_id
    azuread_authentication_only = true
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
