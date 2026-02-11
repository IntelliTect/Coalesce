# Uncomment and configure after running terraform/bootstrap to create the storage account.
# terraform {
#   backend "azurerm" {
#     resource_group_name  = "<project_name>-shared-rg"
#     storage_account_name = "<storage_account_name>"
#     container_name       = "tfstate"
#     key                  = "terraform.tfstate"
#     use_azuread_auth     = true
#   }
# }
