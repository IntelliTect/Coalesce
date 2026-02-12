output "client_id" {
  description = "The application (client) ID"
  value       = azuread_application.this.client_id
}

output "client_secret" {
  description = "The application client secret"
  value       = azuread_application_password.secret1.value
  sensitive   = true
}
