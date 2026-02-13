terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.60"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = ">= 3.7"
    }
    time = {
      source  = "hashicorp/time"
      version = ">= 0.12"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id     = var.subscription_id
  storage_use_azuread = true
}

provider "azuread" {}
