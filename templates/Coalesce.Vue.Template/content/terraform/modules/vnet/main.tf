resource "azurerm_virtual_network" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-vnet"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  address_space       = [var.address_space]
  tags                = var.context.tags
}

resource "azurerm_subnet" "container_apps" {
  name                 = "snet-container-apps"
  resource_group_name  = var.context.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = [var.container_apps_subnet_prefix]
  service_endpoints    = ["Microsoft.Sql", "Microsoft.Storage", "Microsoft.KeyVault"]

  delegation {
    name = "Microsoft.App.environments"
    service_delegation {
      name    = "Microsoft.App/environments"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}
