# CI/CD outputs - configure these as GitHub repository secrets
output "github_secrets_summary" {
  description = "Summary of values to configure in GitHub."
  value       = <<-EOT
    Configure these in your GitHub repository settings:

    Repository Secrets (shared):
      AZURE_TENANT_ID:       ${module.dev.ci_identity_client_id != "" ? data.azurerm_client_config.current.tenant_id : ""}
      AZURE_SUBSCRIPTION_ID: ${module.dev.ci_identity_client_id != "" ? data.azurerm_client_config.current.subscription_id : ""}
      AZURE_CLIENT_ID_BUILD: ${azurerm_user_assigned_identity.ci_build.client_id}

    Environment Secrets (per environment):
      dev environment:
        AZURE_CLIENT_ID: ${module.dev.ci_identity_client_id}
      
      production environment:
        AZURE_CLIENT_ID: ${module.prod.ci_identity_client_id}
  EOT
}

data "azurerm_client_config" "current" {}
