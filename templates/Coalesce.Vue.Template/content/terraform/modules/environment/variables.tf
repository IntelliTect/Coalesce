variable "project_name" {
  description = "The project name, used as a prefix for all resources."
  type        = string
}

variable "environment_name" {
  description = "The environment name (e.g. dev, prod)."
  type        = string
}

variable "location" {
  description = "The Azure region."
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

# Container App

variable "container_registry_login_server" {
  description = "The login server of the shared container registry."
  type        = string
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

# App Insights

variable "app_insights_connection_string" {
  description = "The App Insights connection string to inject into the container. Set to null to disable."
  type        = string
  default     = null
  sensitive   = true
}

variable "log_retention_in_days" {
  description = "Log retention period in days."
  type        = number
  default     = 30
}

# CI/CD

variable "github_repository" {
  description = "The GitHub repository in 'owner/repo' format, for OIDC federation."
  type        = string
}

variable "ci_identity_id" {
  description = "The resource ID of the CI managed identity, for federated credentials."
  type        = string
}

# Extensibility

variable "additional_secrets" {
  description = "Additional secrets to store in Key Vault."
  type        = map(string)
  default     = {}
}

variable "additional_env_vars" {
  description = "Additional environment variables for the container app."
  type = list(object({
    name        = string
    value       = optional(string)
    secret_name = optional(string)
  }))
  default = []
}

variable "tags" {
  description = "Tags to apply to all resources."
  type        = map(string)
  default     = {}
}
