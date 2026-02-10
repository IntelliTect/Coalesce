resource "azurerm_user_assigned_identity" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-id"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  tags                = var.context.tags
}
