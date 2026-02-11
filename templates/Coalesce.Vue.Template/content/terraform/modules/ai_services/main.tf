
resource "azurerm_ai_services" "this" {
  name                         = "${var.context.project_name}-${var.context.environment_name}-ai"
  location                     = var.context.location
  resource_group_name          = var.context.resource_group_name
  sku_name                     = "S0"
  custom_subdomain_name        = "${var.context.project_name}-${var.context.environment_name}-ai"
  local_authentication_enabled = false
  tags                         = var.context.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.ai_services.id]
  }

  network_acls {
    default_action = "Allow"
    ip_rules       = []
  }
}

resource "azurerm_user_assigned_identity" "ai_services" {
  name                = "${var.context.project_name}-${var.context.environment_name}-ai-identity"
  location            = var.context.location
  resource_group_name = var.context.resource_group_name
  tags                = var.context.tags
}

resource "azurerm_cognitive_deployment" "chat" {
  name                 = "chat"
  cognitive_account_id = azurerm_ai_services.this.id

  model {
    format  = "OpenAI"
    name    = var.chat_model_name
    version = var.chat_model_version
  }

  sku {
    name     = "GlobalStandard"
    capacity = 1000
  }
}

resource "azurerm_role_assignment" "openai_user" {
  for_each = var.allowed_users

  scope                = azurerm_ai_services.this.id
  role_definition_name = "Azure AI User"
  principal_id         = each.value
}
