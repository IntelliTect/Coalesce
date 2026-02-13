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

variable "retention_in_days" {
  description = "Log retention in days."
  type        = number
  default     = 30
}
