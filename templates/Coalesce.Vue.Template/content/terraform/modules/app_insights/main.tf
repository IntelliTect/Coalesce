resource "azurerm_log_analytics_workspace" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-law"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.retention_in_days
  tags                = var.context.tags
}

resource "azurerm_application_insights" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-insights"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"
  tags                = var.context.tags
}
