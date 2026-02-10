variable "project_name" {
  description = "The project name, used as a prefix for all Azure resources."
  type        = string
}

variable "location" {
  description = "The Azure region for all resources."
  type        = string
  default     = "westus2"
}

variable "subscription_id" {
  description = "The Azure subscription ID."
  type        = string
}

variable "github_repository" {
  description = "The GitHub repository in 'owner/repo' format, for OIDC federation."
  type        = string
}

variable "tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
