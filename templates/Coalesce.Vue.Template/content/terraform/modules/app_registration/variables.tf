variable "display_name" {
  description = "The display name for the application registration"
  type        = string
}

variable "redirect_origins" {
  description = "A list of origin hosts (without scheme or path) where user tokens are sent for sign-in. https:// and /signin-microsoft will be added automatically."
  type        = list(string)
  default     = []
}
