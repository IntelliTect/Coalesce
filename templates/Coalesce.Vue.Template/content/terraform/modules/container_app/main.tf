resource "azurerm_container_app_environment" "this" {
  name                       = "${var.context.project_name}-${var.context.environment_name}-cae"
  location                   = var.context.location
  resource_group_name        = var.context.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id
  infrastructure_subnet_id   = var.subnet_id
  tags                       = var.context.tags
}

resource "azurerm_container_app" "this" {
  name                         = "${var.context.project_name}-${var.context.environment_name}-ca"
  container_app_environment_id = azurerm_container_app_environment.this.id
  resource_group_name          = var.context.resource_group_name
  revision_mode                = "Single"
  tags                         = var.context.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  registry {
    server   = var.container_registry_login_server
    identity = var.identity_id
  }

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "app"
      image  = "${var.container_registry_login_server}/${var.container_image_name}:${var.container_image_tag}"
      cpu    = var.cpu
      memory = var.memory

      dynamic "env" {
        for_each = var.env_vars
        content {
          name        = env.value.name
          value       = try(env.value.value, null)
          secret_name = try(env.value.secret_name, null)
        }
      }

      liveness_probe {
        path             = "/alive"
        port             = 8080
        transport        = "HTTP"
        initial_delay    = 10
        interval_seconds = 30
      }

      readiness_probe {
        path             = "/health"
        port             = 8080
        transport        = "HTTP"
        interval_seconds = 10
      }

      startup_probe {
        path                    = "/health"
        port                    = 8080
        transport               = "HTTP"
        interval_seconds        = 5
        failure_count_threshold = 30
      }
    }
  }

  dynamic "secret" {
    for_each = var.secrets
    content {
      name                = secret.value.name
      key_vault_secret_id = secret.value.key_vault_secret_id
      identity            = var.identity_id
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}
