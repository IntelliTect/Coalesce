output "environment_id" {
  description = "The ID of the Container App Environment."
  value       = azurerm_container_app_environment.this.id
}

output "app_id" {
  description = "The ID of the Container App."
  value       = azurerm_container_app.this.id
}

output "fqdn" {
  description = "The FQDN of the Container App."
  value       = azurerm_container_app.this.ingress[0].fqdn
}

output "app_name" {
  description = "The name of the Container App."
  value       = azurerm_container_app.this.name
}

output "environment_name" {
  description = "The name of the Container App Environment."
  value       = azurerm_container_app_environment.this.name
}
