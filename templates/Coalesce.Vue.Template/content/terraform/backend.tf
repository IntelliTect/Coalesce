# Uncomment and configure after applying terraform/bootstrap.
# terraform {
#   backend "azurerm" {
#     resource_group_name  = "<project_name>-shared-rg"
#     storage_account_name = "<storage_account_name>"
#     container_name       = "tfstate"
#     key                  = "terraform.tfstate"
#     use_azuread_auth     = true
#   }
# }
