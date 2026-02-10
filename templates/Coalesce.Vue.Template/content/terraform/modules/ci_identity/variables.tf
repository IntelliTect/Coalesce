variable "project_name" {
  description = "The project name."
  type        = string
}

variable "github_repository" {
  description = "The GitHub repository in 'owner/repo' format."
  type        = string
}

variable "github_environments" {
  description = "Map of environment names to GitHub environment names for OIDC federation."
  type        = map(string)
  default = {
    dev  = "dev"
    prod = "production"
  }
}

variable "acr_id" {
  description = "The resource ID of the container registry to grant AcrPush."
  type        = string
}

variable "environment_resource_group_ids" {
  description = "Map of environment names to resource group IDs to grant Contributor for deployments."
  type        = map(string)
  default     = {}
}
