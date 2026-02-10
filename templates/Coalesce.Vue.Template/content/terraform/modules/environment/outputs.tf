output "identity_principal_id" {
  description = "The principal ID of the environment's managed identity."
  value       = azurerm_user_assigned_identity.app.principal_id
}

output "ci_identity_client_id" {
  description = "The client ID of the CI identity for this environment."
  value       = module.ci_identity.client_id
}
