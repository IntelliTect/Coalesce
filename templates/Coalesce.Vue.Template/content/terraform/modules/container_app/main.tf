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

  infrastructure_resource_group_name = "ME_${var.context.resource_group_name}-aca-managed"

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
    server   = var.container_registry.login_server
    identity = var.identity_id
  }

  template {
    min_replicas               = var.min_replicas
    max_replicas               = var.max_replicas
    cooldown_period_in_seconds = 3600

    init_container {
      name = "migrations"
      // NOTE: Initial container image will be replaced by CI/CD deploy
      image  = var.container_registry.initial_init_image
      cpu    = var.cpu
      memory = var.memory

      dynamic "env" {
        for_each = var.env_vars
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    container {
      name = "app"
      // NOTE: Initial container image will be replaced by CI/CD deploy
      image  = var.container_registry.initial_image
      cpu    = var.cpu
      memory = var.memory

      env {
        # Port for crccheck/hello-world to listen on
        name  = "PORT"
        value = "8080"
      }

      env {
        name  = "ASPNETCORE_HTTP_PORTS"
        value = "8080;8081"
      }

      env {
        # Health check endpoints are restricted to this port so they
        # are not publicly accessible through the ingress target port.
        name  = "HEALTH_PORT"
        value = "8081"
      }

      dynamic "env" {
        for_each = var.env_vars
        content {
          name  = env.key
          value = env.value
        }
      }

      liveness_probe {
        path             = "/alive"
        port             = 8081
        transport        = "HTTP"
        initial_delay    = 10
        interval_seconds = 30
      }

      readiness_probe {
        path             = "/alive"
        port             = 8081
        transport        = "HTTP"
        interval_seconds = 10
      }

      startup_probe {
        path                    = "/health"
        port                    = 8081
        transport               = "HTTP"
        interval_seconds        = 5
        failure_count_threshold = 30
      }
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
      template[0].container[0].image,
      template[0].init_container[0].image,
    ]
  }
}
