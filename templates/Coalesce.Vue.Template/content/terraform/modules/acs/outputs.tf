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

output "sender_email" {
  description = "The default DoNotReply sender email address from the Azure-managed domain."
  value       = "DoNotReply@${azurerm_email_communication_service_domain.azure_managed.from_sender_domain}"
}
