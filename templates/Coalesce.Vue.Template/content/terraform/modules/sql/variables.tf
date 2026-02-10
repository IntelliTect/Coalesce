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

variable "aad_admin_login" {
  description = "The login name of the Azure AD administrator."
  type        = string
}

variable "aad_admin_object_id" {
  description = "The object ID of the Azure AD administrator (typically the managed identity)."
  type        = string
}

variable "subnet_id" {
  description = "The subnet ID for the VNet service endpoint rule."
  type        = string
}
