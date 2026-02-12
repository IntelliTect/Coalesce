
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


locals {
  secret_duration_months = 24
  secret_duration_hours  = local.secret_duration_months * 730
}

// https://2mas.github.io/blog/rotating-azure-app-registration-secrets-with-terraform/
resource "time_rotating" "secret_rotation_1" {
  # In between rotations, secret_rotation2 will be following 100% out of phase
  # so we'll always have a password with lots of time left between rotations.
  rotation_months = secret_duration_months / 2
}

resource "time_rotating" "secret_rotation_2" {
  rfc3339         = time_rotating.secret_rotation.rotation_rfc3339
  rotation_months = secret_duration_months / 2 # additive to rfc3339

  lifecycle {
    ignore_changes = [rfc3339]
  }
}

resource "azuread_application_password" "app_secret_1" {
  application_id = azuread_application.app.id
  display_name   = "tf-managed-rotation-1"
  end_date       = timeadd(time_rotating.secret_rotation_1.id, "${secret_duration_hours}h")

  rotate_when_changed = {
    rotation = time_rotating.secret_rotation_1.id
  }
}

resource "azuread_application_password" "app_secret_2" {
  application_id = azuread_application.app.id
  display_name   = "tf-managed-rotation-2"
  end_date       = timeadd(time_rotating.secret_rotation_2.id, "${secret_duration_hours}h")

  rotate_when_changed = {
    rotation = time_rotating.secret_rotation_2.id
  }
}
