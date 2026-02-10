# Terraform Template Development

This README is for developers working on the Coalesce project template itself.
It is excluded from template instantiation and will not appear in generated projects.

For end-user instructions, see [README.md](README.md).

## Developer Setup

To test changes to the Terraform configuration, you can deploy it to your own
Azure subscription. Since `backend.tf` ships as a commented-out placeholder,
developers use a `backend_override.tf` file (gitignored) to configure a real
backend locally without modifying committed files.

1. Create `terraform.tfvars` (gitignored) from `terraform.tfvars.example` and fill in real values.

2. Run bootstrap to create the shared resource group and state storage:

   ```bash
   cd bootstrap
   # Create terraform.tfvars with project_name, location, subscription_id, storage_account_name
   terraform init
   terraform apply
   ```

3. Create `backend_override.tf` (gitignored) using the values from the bootstrap output:

   ```hcl
   terraform {
     backend "azurerm" {
       resource_group_name  = "<from bootstrap output>"
       storage_account_name = "<from bootstrap output>"
       container_name       = "tfstate"
       key                  = "terraform.tfstate"
     }
   }
   ```

4. Initialize and apply:

   ```bash
   cd ..  # back to terraform/
   terraform init
   terraform apply
   ```

## How It Works

- **`backend.tf`** ships with a commented-out backend block with placeholder values.
  End users uncomment it and fill in their values after running bootstrap.

- **`backend_override.tf`** is gitignored (via the override pattern in `.gitignore`).
  Template developers use this to point at the real test environment backend
  without modifying committed files.

- **`terraform.tfvars`** is also gitignored. Both template developers and end users
  may create this locally from `terraform.tfvars.example`.

## Architecture

```
bootstrap/          # One-time setup: creates shared RG + state storage account
modules/
  environment/      # Per-environment resources (dev, prod)
  container_app/    # Azure Container Apps + environment
  container_registry/ # Shared ACR
  vnet/             # Virtual network + subnets
  sql/              # Azure SQL Server + database
  key_vault/        # Azure Key Vault
  storage/          # Azure Blob Storage
  app_insights/     # Application Insights + Log Analytics
  ci_identity/      # Per-environment CI/CD managed identity
```

The shared resource group (`<project>-shared-rg`) is created by bootstrap and
referenced as a data source in the main config. Each environment (dev, prod)
gets its own resource group.
