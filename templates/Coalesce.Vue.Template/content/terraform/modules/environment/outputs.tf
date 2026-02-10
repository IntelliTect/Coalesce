output "identity_principal_id" {
  description = "The principal ID of the environment's managed identity."
  value       = azurerm_user_assigned_identity.app.principal_id
}
