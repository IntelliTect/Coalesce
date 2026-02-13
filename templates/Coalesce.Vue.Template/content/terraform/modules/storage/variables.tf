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

variable "admin_principals" {
  description = "Map of descriptive keys to principal IDs to grant Storage Blob Data Contributor role."
  type        = map(string)
  default     = {}
}
