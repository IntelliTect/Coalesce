resource "azurerm_communication_service" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-acs"
  resource_group_name = var.context.resource_group_name
  data_location       = var.data_location
  tags                = var.context.tags
}

resource "azurerm_role_assignment" "contributor" {
  scope                = azurerm_communication_service.this.id
  role_definition_name = "Contributor"
  principal_id         = var.identity_principal_id
}
