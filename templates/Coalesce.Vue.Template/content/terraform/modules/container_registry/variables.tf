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

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
  default     = {}
}

variable "initial_image" {
  description = "The initial image to import for container app deployments before CI/CD pushes the real application."
  type        = string
  default     = "crccheck/hello-world:latest"
}

variable "initial_init_image" {
  description = "The initial image to import for init containers. Must exit successfully."
  type        = string
  default     = "busybox:latest"
}
