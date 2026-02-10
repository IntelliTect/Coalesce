locals {
  cae_name = "${var.context.project_name}-${var.context.environment_name}-cae"
}

resource "azurerm_container_app_environment" "this" {
  name                       = local.cae_name
  location                   = var.context.location
  resource_group_name        = var.context.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id
  infrastructure_subnet_id   = var.subnet_id
  tags                       = var.context.tags

  infrastructure_resource_group_name = "${var.context.resource_group_name}-aca-managed"

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }
}

resource "azurerm_container_app" "this" {
  name                         = "${var.context.project_name}-${var.context.environment_name}-app"
  container_app_environment_id = azurerm_container_app_environment.this.id
  resource_group_name          = var.context.resource_group_name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"
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
      name = "app"
      // NOTE: Initial container image will be replaced by azure pipelines deploy
      image  = "${var.container_registry_login_server}/crccheck/hello-world:latest"
      cpu    = var.cpu
      memory = var.memory

      env {
        # Port for crccheck/hello-world to listen on
        name  = "PORT"
        value = "8080"
      }

      env {
        name  = "ASPNETCORE_HTTP_PORTS"
        value = "8080"
      }

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

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }
}
