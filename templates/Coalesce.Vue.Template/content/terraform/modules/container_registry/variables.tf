variable "name" {
  description = "The name of the container registry. Must be globally unique, alphanumeric only."
  type        = string
}

variable "location" {
  description = "The Azure region."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group."
  type        = string
}

variable "sku" {
  description = "The SKU of the container registry (Basic, Standard, Premium)."
  type        = string
  default     = "Basic"
}

variable "pull_identity_principal_ids" {
  description = "Map of identity names to principal IDs to grant AcrPull role."
  type        = map(string)
  default     = {}
}

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
  default     = {}
}
