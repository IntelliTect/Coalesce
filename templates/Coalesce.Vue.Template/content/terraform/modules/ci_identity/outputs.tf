output "client_id" {
  description = "The Application (client) ID for configuring GitHub secrets."
  value       = azuread_application.ci.client_id
}

output "tenant_id" {
  description = "The Azure AD tenant ID for configuring GitHub secrets."
  value       = data.azurerm_client_config.current.tenant_id
}

output "subscription_id" {
  description = "The Azure subscription ID for configuring GitHub secrets."
  value       = data.azurerm_client_config.current.subscription_id
}

output "github_secrets_summary" {
  description = "The values to configure as GitHub repository secrets/variables."
  value       = <<-EOT
    Configure these in your GitHub repository settings:

    Secrets:
      AZURE_CLIENT_ID:       ${azuread_application.ci.client_id}
      AZURE_TENANT_ID:       ${data.azurerm_client_config.current.tenant_id}
      AZURE_SUBSCRIPTION_ID: ${data.azurerm_client_config.current.subscription_id}
  EOT
}
