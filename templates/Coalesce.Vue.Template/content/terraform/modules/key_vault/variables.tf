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

variable "secrets_users" {
  description = "Map of descriptive keys to principal IDs to grant Key Vault Secrets User role."
  type        = map(string)
  default     = {}
}

variable "secrets" {
  description = "Map of secret names to values to store in the Key Vault."
  type        = map(string)
  default     = {}
}
