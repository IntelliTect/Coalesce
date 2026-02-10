resource "azurerm_storage_account" "this" {
  name                            = replace("${var.context.project_name}${var.context.environment_name}st", "-", "")
  location                        = var.context.location
  resource_group_name             = var.context.resource_group_name
  account_tier                    = "Standard"
  account_replication_type        = var.replication_type
  shared_access_key_enabled       = false
  allow_nested_items_to_be_public = false
  tags                            = var.context.tags

  network_rules {
    default_action             = "Deny"
    virtual_network_subnet_ids = var.allowed_subnet_ids
  }
}

data "azurerm_client_config" "current" {}

# Grant the Terraform executor access so blob containers can be managed
# when shared_access_key_enabled = false.
resource "azurerm_role_assignment" "deployer_blob_contributor" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}


resource "azurerm_storage_container" "this" {
  name                  = var.container_name
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"

  depends_on = [azurerm_role_assignment.deployer_blob_contributor]
}

resource "azurerm_role_assignment" "blob_contributor" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.identity_principal_id
}
