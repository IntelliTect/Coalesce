output "vnet_id" {
  description = "The ID of the virtual network."
  value       = azurerm_virtual_network.this.id
}

output "vnet_name" {
  description = "The name of the virtual network."
  value       = azurerm_virtual_network.this.name
}

output "container_apps_subnet_id" {
  description = "The ID of the Container Apps subnet."
  value       = azurerm_subnet.container_apps.id
}
