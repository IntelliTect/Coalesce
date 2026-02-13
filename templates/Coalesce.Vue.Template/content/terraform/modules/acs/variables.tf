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

variable "data_location" {
  description = "The data location for the Communication Service (e.g. United States)."
  type        = string
  default     = "United States"
}

variable "admin_principals" {
  description = "Map of descriptive keys to principal IDs to grant Contributor role."
  type        = map(string)
  default     = {}
}
