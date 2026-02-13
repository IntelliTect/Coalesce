variable "project_name" {
  description = "The project name, used as a prefix for all Azure resources."
  type        = string

  validation {
    condition     = var.project_name == lower(var.project_name)
    error_message = "project_name must be lowercase."
  }

  validation {
    condition     = length(var.project_name) <= 16
    error_message = "project_name must not exceed 16 characters."
  }
}

variable "location" {
  description = "The Azure region."
  type        = string
  default     = "westus2"
}

variable "subscription_id" {
  description = "The Azure subscription ID."
  type        = string
}

variable "storage_account_name" {
  description = "The storage account name for Terraform state. Must be globally unique. Defaults to '{project_name}tfstate'."
  type        = string
  default     = null
}

variable "github_repository" {
  description = "The GitHub repository in 'owner/repo' format, for OIDC federation."
  type        = string
}
