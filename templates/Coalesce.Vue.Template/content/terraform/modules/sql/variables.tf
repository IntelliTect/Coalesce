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

variable "sku_name" {
  description = "The DTU-based SKU name (e.g. Basic, S0, S1, S2)."
  type        = string
  default     = "Basic"
}

variable "admin_principals" {
  description = "Map of descriptive keys to principal IDs to add as SQL admins. Must include 'app' key."
  type        = map(string)
}

variable "subnet_id" {
  description = "The subnet ID for the VNet service endpoint rule."
  type        = string
}
