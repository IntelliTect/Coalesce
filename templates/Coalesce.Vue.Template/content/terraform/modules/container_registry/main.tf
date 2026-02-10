resource "azurerm_container_registry" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  admin_enabled       = false
  tags                = var.tags
}

resource "azurerm_role_assignment" "acr_pull" {
  for_each = var.pull_identity_principal_ids

  scope                = azurerm_container_registry.this.id
  role_definition_name = "AcrPull"
  principal_id         = each.value
}

// Import hello world image for initial container deployments.
resource "terraform_data" "import_helloworld_image" {
  provisioner "local-exec" {
    command     = <<-EOT
    az acr import `
      --name ${azurerm_container_registry.this.name} `
      --source docker.io/crccheck/hello-world:latest `
      --image crccheck/hello-world:latest `
      --subscription ${data.azurerm_client_config.current.subscription_id}
    EOT
    interpreter = ["pwsh", "-Command"]
  }

  triggers_replace = {
    acr_id  = azurerm_container_registry.this.id
    version = 2
  }

  depends_on = [azurerm_container_registry.this]
}
