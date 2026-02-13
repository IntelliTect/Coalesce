output "id" {
  description = "The ID of the storage account."
  value       = azurerm_storage_account.this.id
}

output "name" {
  description = "The name of the storage account."
  value       = azurerm_storage_account.this.name
}

output "blobs_connection_string" {
  description = "Connection string for blob storage"
  value       = "Endpoint=${azurerm_storage_account.this.primary_blob_endpoint};ContainerName=${azurerm_storage_container.this.name}"
}
