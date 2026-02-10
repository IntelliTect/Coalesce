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

variable "container_name" {
  description = "The name of the blob container."
  type        = string
  default     = "data"
}

variable "replication_type" {
  description = "The replication type (LRS, GRS, etc.)."
  type        = string
  default     = "LRS"
}

variable "identity_principal_id" {
  description = "The principal ID of the managed identity to grant Storage Blob Data Contributor."
  type        = string
}

variable "allowed_subnet_ids" {
  description = "List of subnet IDs allowed through network rules."
  type        = list(string)
  default     = []
}
