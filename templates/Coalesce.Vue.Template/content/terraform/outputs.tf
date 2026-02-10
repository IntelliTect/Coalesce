# Dev outputs
output "dev_container_app_fqdn" {
  description = "The FQDN of the dev Container App."
  value       = module.dev.container_app_fqdn
}

output "dev_key_vault_uri" {
  description = "The URI of the dev Key Vault."
  value       = module.dev.key_vault_uri
}

output "dev_sql_server_fqdn" {
  description = "The FQDN of the dev SQL server."
  value       = module.dev.sql_server_fqdn
}

# Prod outputs
output "prod_container_app_fqdn" {
  description = "The FQDN of the prod Container App."
  value       = module.prod.container_app_fqdn
}

output "prod_key_vault_uri" {
  description = "The URI of the prod Key Vault."
  value       = module.prod.key_vault_uri
}

output "prod_sql_server_fqdn" {
  description = "The FQDN of the prod SQL server."
  value       = module.prod.sql_server_fqdn
}

# Shared outputs
output "acr_login_server" {
  description = "The login server of the shared container registry."
  value       = module.acr.login_server
}

output "acr_name" {
  description = "The name of the shared container registry."
  value       = module.acr.name
}

# CI/CD outputs - configure these as GitHub repository secrets
output "ci_client_id" {
  description = "The Application (client) ID for GitHub Actions AZURE_CLIENT_ID secret."
  value       = module.ci_identity.client_id
}

output "ci_tenant_id" {
  description = "The tenant ID for GitHub Actions AZURE_TENANT_ID secret."
  value       = module.ci_identity.tenant_id
}

output "ci_github_secrets_summary" {
  description = "Summary of values to configure in GitHub."
  value       = module.ci_identity.github_secrets_summary
}
