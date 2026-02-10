output "id" {
  description = "The ID of the Communication Service."
  value       = azurerm_communication_service.this.id
}

output "endpoint" {
  description = "The endpoint of the Communication Service."
  value       = "https://${azurerm_communication_service.this.name}.communication.azure.com"
}

output "name" {
  description = "The name of the Communication Service."
  value       = azurerm_communication_service.this.name
}
