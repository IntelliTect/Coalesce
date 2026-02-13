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

variable "address_space" {
  description = "The address space for the VNet (e.g. 10.0.0.0/16)."
  type        = string
}

variable "container_apps_subnet_prefix" {
  description = "The address prefix for the Container Apps subnet (e.g. 10.0.0.0/23). Must be at least /23."
  type        = string
}
