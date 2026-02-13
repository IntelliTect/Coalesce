terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

resource "azurerm_resource_group" "shared" {
  name     = "${var.project_name}-shared-rg"
  location = var.location
}

locals {
  storage_account_name = coalesce(var.storage_account_name, "${var.project_name}tfstate")
}

resource "azurerm_storage_account" "tfstate" {
  name                            = local.storage_account_name
  resource_group_name             = azurerm_resource_group.shared.name
  location                        = azurerm_resource_group.shared.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  shared_access_key_enabled       = true # Required for Terraform state backend
  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_container" "tfstate" {
  name                  = "tfstate"
  storage_account_id    = azurerm_storage_account.tfstate.id
  container_access_type = "private"
}

# ============================================================
# CI identity (build, deploy, and Terraform)
# ============================================================
resource "azurerm_user_assigned_identity" "ci" {
  name                = "${var.project_name}-ci-identity"
  resource_group_name = azurerm_resource_group.shared.name
  location            = azurerm_resource_group.shared.location
}

# Federated credential for pushes to main (build)
resource "azurerm_federated_identity_credential" "ci_branch_main" {
  name                = "github-branch-main"
  resource_group_name = azurerm_resource_group.shared.name
  parent_id           = azurerm_user_assigned_identity.ci.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:ref:refs/heads/main"
}

# Federated credential for pull requests (terraform plan)
resource "azurerm_federated_identity_credential" "ci_pull_request" {
  name                = "github-pull-request"
  resource_group_name = azurerm_resource_group.shared.name
  parent_id           = azurerm_user_assigned_identity.ci.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:pull_request"
}

# Federated credential for terraform apply (via 'terraform' GitHub Environment)
resource "azurerm_federated_identity_credential" "ci_terraform_env" {
  name                = "github-environment-terraform"
  resource_group_name = azurerm_resource_group.shared.name
  parent_id           = azurerm_user_assigned_identity.ci.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_repository}:environment:terraform"
}

# Contributor on the subscription so Terraform can manage all resources
resource "azurerm_role_assignment" "ci_contributor" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "Contributor"
  principal_id         = azurerm_user_assigned_identity.ci.principal_id
}

# Terraform needs to create role assignments on resources like ACR, Key Vault, and Storage
resource "azurerm_role_assignment" "ci_uaa" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "User Access Administrator"
  principal_id         = azurerm_user_assigned_identity.ci.principal_id
}

# Terraform state backend uses Azure AD auth, which requires data plane access
resource "azurerm_role_assignment" "ci_tfstate_blob_owner" {
  scope                = azurerm_storage_account.tfstate.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azurerm_user_assigned_identity.ci.principal_id
}

# ============================================================
# Developers AAD Group
# ============================================================
resource "azuread_group" "developers" {
  display_name     = "${var.project_name}-developers"
  security_enabled = true
}
