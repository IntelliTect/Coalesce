output "server_id" {
  description = "The ID of the SQL server."
  value       = azurerm_mssql_server.this.id
}

output "server_fqdn" {
  description = "The fully qualified domain name of the SQL server."
  value       = azurerm_mssql_server.this.fully_qualified_domain_name
}

output "database_id" {
  description = "The ID of the database."
  value       = azurerm_mssql_database.this.id
}

output "database_name" {
  description = "The name of the database."
  value       = azurerm_mssql_database.this.name
}

output "connection_string" {
  description = "The ADO.NET connection string using managed identity (Azure AD) authentication."
  value       = "Server=tcp:${azurerm_mssql_server.this.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.this.name};Authentication=Active Directory Default;"
}
