
# NOTE: If this is failing to provision due to permission errors, see `../../README.md`.
resource "azuread_application" "this" {
  display_name     = var.display_name
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  api {
    requested_access_token_version = 2
  }

  web {
    redirect_uris = [for origin in var.redirect_origins : "https://${origin}/signin-microsoft"]

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }
}

resource "time_rotating" "secret1" {
  rotation_years = 50
}

resource "azuread_application_password" "secret1" {
  application_id = azuread_application.this.id
  display_name   = "Terraform-managed secret"
  end_date       = time_rotating.secret1.rotation_rfc3339
}
