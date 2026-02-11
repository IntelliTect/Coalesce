variable "context" {
  description = "The shared context object containing project_name, environment_name, location, resource_group_name, and tags."
  type = object({
    project_name        = string
    environment_name    = string
    location            = string
    resource_group_name = string
    tags                = map(string)
  })
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace for the Container App Environment."
  type        = string
  default     = null
}

variable "subnet_id" {
  description = "The subnet ID for the Container App Environment."
  type        = string
}

variable "identity_id" {
  description = "The resource ID of the user-assigned managed identity."
  type        = string
}

variable "container_registry" {
  description = "The container registry module outputs."
  type = object({
    login_server  = string
    initial_image = string
  })
}

variable "cpu" {
  description = "CPU cores allocated to the container (e.g. 0.5, 1.0)."
  type        = number
  default     = 0.5
}

variable "memory" {
  description = "Memory allocated to the container (e.g. 1Gi)."
  type        = string
  default     = "1Gi"
}

variable "min_replicas" {
  description = "Minimum number of replicas."
  type        = number
  default     = 0
}

variable "max_replicas" {
  description = "Maximum number of replicas."
  type        = number
  default     = 3
}

variable "env_vars" {
  description = "List of environment variables for the container."
  type        = map(string)
  default     = {}
}
