output "id" {
  description = "The ID of the container registry."
  value       = azurerm_container_registry.this.id
}

output "login_server" {
  description = "The login server URL of the container registry."
  value       = azurerm_container_registry.this.login_server
}

output "name" {
  description = "The name of the container registry."
  value       = azurerm_container_registry.this.name
}

output "initial_image" {
  description = "The initial hello-world image to use for container apps before the first deployment. Depends on the image import."
  value       = "${azurerm_container_registry.this.login_server}/hello-world-http"
  depends_on  = [terraform_data.import_initial_http]
}

output "initial_init_image" {
  description = "The initial image for init containers that exits immediately. Depends on the image import."
  value       = "${azurerm_container_registry.this.login_server}/hello-world-init"
  depends_on  = [terraform_data.import_initial_init]
}


