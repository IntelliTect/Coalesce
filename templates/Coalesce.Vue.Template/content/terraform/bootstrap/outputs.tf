output "resource_group_name" {
  value = azurerm_resource_group.shared.name
}

output "storage_account_name" {
  value = azurerm_storage_account.tfstate.name
}

output "container_name" {
  value = azurerm_storage_container.tfstate.name
}

output "backend_config" {
  description = "Copy this into terraform/backend.tf"
  value       = <<-EOT
    terraform {
      backend "azurerm" {
        resource_group_name  = "${azurerm_resource_group.shared.name}"
        storage_account_name = "${azurerm_storage_account.tfstate.name}"
        container_name       = "${azurerm_storage_container.tfstate.name}"
        key                  = "terraform.tfstate"
      }
    }
  EOT
}

output "github_vars_summary" {
  description = "GitHub repository variables to configure after bootstrap."
  value       = <<-EOT
    Configure these in your GitHub repository settings (Variables):

      AZURE_TENANT_ID:       ${azurerm_user_assigned_identity.ci.tenant_id}
      AZURE_SUBSCRIPTION_ID: ${var.subscription_id}
      AZURE_CLIENT_ID:       ${azurerm_user_assigned_identity.ci.client_id}
      PROJECT_NAME:          ${var.project_name}
      ACR_NAME:              ${replace("${var.project_name}acr", "-", "")}
  EOT
}
