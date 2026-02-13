data "azurerm_client_config" "current" {}

resource "azurerm_container_registry" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  admin_enabled       = false
  tags                = var.tags
}

// Import initial images - this is done because container apps cannot be created without a container image,
// and specifying an external container repository directly in the container app will delete
// the app's managed identity assignment.

// Import hello world image for initial container deployments.
resource "terraform_data" "import_initial_http" {
  provisioner "local-exec" {
    command     = <<-EOT
    az acr import `
      --name ${azurerm_container_registry.this.name} `
      --source docker.io/${var.initial_image} `
      --image hello-world-http `
      --subscription ${data.azurerm_client_config.current.subscription_id}
    EOT
    interpreter = ["pwsh", "-Command"]
  }

  triggers_replace = {
    acr_id  = azurerm_container_registry.this.id
    version = 1
  }

  depends_on = [azurerm_container_registry.this]
}

// Import a short-lived image for init containers that need to exit immediately.
resource "terraform_data" "import_initial_init" {
  provisioner "local-exec" {
    command     = <<-EOT
    az acr import `
      --name ${azurerm_container_registry.this.name} `
      --source mcr.microsoft.com/${var.initial_init_image} `
      --image hello-world-init `
      --subscription ${data.azurerm_client_config.current.subscription_id}
    EOT
    interpreter = ["pwsh", "-Command"]
  }

  triggers_replace = {
    acr_id  = azurerm_container_registry.this.id
    version = 1
  }

  depends_on = [azurerm_container_registry.this]
}
