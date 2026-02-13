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

variable "admin_principals" {
  description = "Map of descriptive keys to principal IDs to grant Azure AI User role."
  type        = map(string)
  default     = {}
}

variable "chat_model_name" {
  description = "The model name for the chat deployment (e.g. gpt-4.1)."
  type        = string
  default     = "gpt-4.1"
}

variable "chat_model_version" {
  description = "The model version for the chat deployment."
  type        = string
  default     = "2025-04-14"
}
