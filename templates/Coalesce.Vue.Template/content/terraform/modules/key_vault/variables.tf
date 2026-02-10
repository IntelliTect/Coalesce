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

variable "identity_principal_id" {
  description = "The principal ID of the managed identity to grant Key Vault Secrets User."
  type        = string
}

variable "secrets" {
  description = "Map of secret names to values to store in the Key Vault."
  type        = map(string)
  default     = {}
}

variable "allowed_subnet_ids" {
  description = "List of subnet IDs allowed through network rules."
  type        = list(string)
  default     = []
}
