output "id" {
  description = "The ID of the Application Insights resource."
  value       = azurerm_application_insights.this.id
}

output "connection_string" {
  description = "The connection string of the Application Insights resource."
  value       = azurerm_application_insights.this.connection_string
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace."
  value       = azurerm_log_analytics_workspace.this.id
}
