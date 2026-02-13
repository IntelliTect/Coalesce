output "client_id" {
  description = "The application (client) ID"
  value       = azuread_application.this.client_id
}

output "client_secret" {
  description = "The password value (client secret) for the Azure AD application - uses the one with latest expiration"
  value       = time_rotating.secret_rotation_1.unix > time_rotating.secret_rotation_2.unix ? azuread_application_password.app_secret_1.value : azuread_application_password.app_secret_2.value
  sensitive   = true
}
