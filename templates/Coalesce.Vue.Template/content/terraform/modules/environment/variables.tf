variable "context" {
  description = "The shared context object containing project_name, location, tags, and CI/CD configuration."
  type = object({
    project_name      = string
    location          = string
    tags              = map(string)
    github_repository = string
    ci_identity_id    = string
  })
}

variable "display_name" {
  description = "An end-user-friendly display name of the application/environment."
  type        = string
}

variable "environment_name" {
  description = "The environment name (e.g. dev, prod)."
  type        = string
}

# Networking

variable "vnet_address_space" {
  description = "The VNet address space (e.g. 10.0.0.0/16)."
  type        = string
}

variable "container_apps_subnet_prefix" {
  description = "The Container Apps subnet prefix (e.g. 10.0.0.0/23). Must be at least /23."
  type        = string
}

variable "allowed_origins" {
  description = "A list of origins (without scheme or path) where users may use the application."
  type        = list(string)
  default     = []
}

# Container App

variable "container_registry" {
  description = "The shared container registry module outputs."
  type = object({
    id            = string
    login_server  = string
    initial_image = string
  })
}

variable "container_app_cpu" {
  description = "CPU cores for the container app."
  type        = number
  default     = 0.5
}

variable "container_app_memory" {
  description = "Memory for the container app."
  type        = string
  default     = "1Gi"
}

variable "container_app_min_replicas" {
  description = "Minimum replicas for the container app."
  type        = number
  default     = 0
}

variable "container_app_max_replicas" {
  description = "Maximum replicas for the container app."
  type        = number
  default     = 3
}

# SQL

variable "sql_sku_name" {
  description = "The DTU-based SKU for Azure SQL (e.g. Basic, S0, S1, S2)."
  type        = string
  default     = "Basic"
}

# Storage

variable "storage_replication_type" {
  description = "Storage replication type (LRS, GRS, etc.)."
  type        = string
  default     = "LRS"
}

variable "admin_principals" {
  description = "Map of descriptive keys to principal IDs to grant access to environment resources."
  type        = map(string)
  default     = {}
}

# Extensibility

variable "additional_secrets" {
  description = "Additional secrets to store in Key Vault."
  type        = map(string)
  default     = {}
}

variable "env_vars" {
  description = "Additional environment variables for the container app."
  type        = map(string)
  default     = {}
}
