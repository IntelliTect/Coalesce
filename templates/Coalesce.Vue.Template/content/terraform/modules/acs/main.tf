resource "azurerm_communication_service" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-acs"
  resource_group_name = var.context.resource_group_name
  data_location       = var.data_location
  tags                = var.context.tags
}

resource "azurerm_email_communication_service" "this" {
  name                = "${var.context.project_name}-${var.context.environment_name}-ecs"
  resource_group_name = var.context.resource_group_name
  data_location       = var.data_location
  tags                = var.context.tags
}

resource "azurerm_email_communication_service_domain" "azure_managed" {
  name              = "AzureManagedDomain"
  email_service_id  = azurerm_email_communication_service.this.id
  domain_management = "AzureManaged"
}

resource "azurerm_communication_service_email_domain_association" "this" {
  communication_service_id = azurerm_communication_service.this.id
  email_service_domain_id  = azurerm_email_communication_service_domain.azure_managed.id
}

resource "azurerm_role_assignment" "contributor" {
  for_each = var.admin_principals

  scope                = azurerm_communication_service.this.id
  role_definition_name = "Contributor"
  principal_id         = each.value
}
