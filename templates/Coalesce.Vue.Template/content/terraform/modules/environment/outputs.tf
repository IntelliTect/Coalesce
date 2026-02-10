output "resource_group_name" {
  description = "The name of the environment's resource group."
  value       = module.resource_group.name
}

output "resource_group_id" {
  description = "The ID of the environment's resource group."
  value       = module.resource_group.id
}

output "container_app_fqdn" {
  description = "The FQDN of the Container App."
  value       = module.container_app.fqdn
}

output "container_app_name" {
  description = "The name of the Container App."
  value       = module.container_app.app_name
}

output "container_app_environment_name" {
  description = "The name of the Container App Environment."
  value       = module.container_app.environment_name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault."
  value       = module.key_vault.vault_uri
}

output "sql_server_fqdn" {
  description = "The FQDN of the SQL server."
  value       = module.sql.server_fqdn
}

output "sql_connection_string" {
  description = "The SQL connection string using managed identity."
  value       = module.sql.connection_string
}

output "identity_principal_id" {
  description = "The principal ID of the environment's managed identity."
  value       = module.identity.principal_id
}

output "identity_client_id" {
  description = "The client ID of the environment's managed identity."
  value       = module.identity.client_id
}

output "storage_blob_endpoint" {
  description = "The primary blob endpoint of the storage account."
  value       = module.storage.primary_blob_endpoint
}

output "app_insights_connection_string" {
  description = "The App Insights connection string."
  value       = module.app_insights.connection_string
  sensitive   = true
}
