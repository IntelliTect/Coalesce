output "client_id" {
  description = "The client ID for configuring GitHub secrets."
  value       = azurerm_user_assigned_identity.ci.client_id
}

output "tenant_id" {
  description = "The Azure AD tenant ID for configuring GitHub secrets."
  value       = data.azurerm_client_config.current.tenant_id
}

output "subscription_id" {
  description = "The Azure subscription ID for configuring GitHub secrets."
  value       = data.azurerm_client_config.current.subscription_id
}
