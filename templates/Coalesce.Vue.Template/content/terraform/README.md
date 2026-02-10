# Terraform Infrastructure

This directory contains Terraform configurations for provisioning Azure infrastructure.

## Prerequisites

- [Terraform](https://developer.hashicorp.com/terraform/install)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) - logged in with `az login`
- An Azure subscription

## Getting Started

### 1. Bootstrap Remote State

Before using the main Terraform config, you need a storage account for remote state:

```bash
cd bootstrap
cp ../terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values, and add storage_account_name
terraform init
terraform apply
```

Copy the `backend_config` output into `../backend.tf` (uncomment and fill in the values).

### 2. Deploy Infrastructure

```bash
cd ..  # back to terraform/
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform apply
```

### 3. Deploy Application

After infrastructure is provisioned, push your container image:

```bash
az acr login --name <acr_name>
dotnet publish \
  -r linux-x64 \
  --self-contained \
  -t:PublishContainer \
  -p:ContainerRegistry=<acr_name>.azurecr.io \
  -p:ContainerImageTags=1.0.0
```

Then update the `container_image_tag` variable and run `terraform apply` again, or use the GitHub Actions workflow for CI/CD.

## Structure

```
terraform/
├── bootstrap/          # One-time setup for remote state storage
├── modules/
│   ├── acs/            # Azure Communication Services
│   ├── app_insights/   # Application Insights + Log Analytics
│   ├── container_app/  # Container App Environment + App
│   ├── container_registry/ # Azure Container Registry
│   ├── environment/    # Composition module (one per environment)
│   ├── identity/       # User-assigned managed identity
│   ├── key_vault/      # Key Vault with RBAC
│   ├── resource_group/ # Resource group
│   ├── sql/            # Azure SQL (AAD-only, DTU)
│   ├── storage/        # Storage account + blob container
│   └── vnet/           # Virtual network + subnets
├── main.tf             # Root config: shared resources + dev/prod environments
├── variables.tf        # Input variables
├── outputs.tf          # Output values
├── providers.tf        # Provider configuration
├── backend.tf          # Remote state backend (uncomment after bootstrap)
└── terraform.tfvars.example
```

## Environments

Two environments are defined:

| | Dev | Prod |
|---|---|---|
| SQL SKU | Basic | S1 |
| Container CPU | 0.25 | 1.0 |
| Container Memory | 0.5Gi | 2Gi |
| Min Replicas | 0 | 1 |
| Max Replicas | 1 | 5 |
| Storage Replication | LRS | GRS |
| VNet Range | 10.0.0.0/16 | 10.1.0.0/16 |

## Security

- **No key-based access** — all services use Azure AD / managed identity authentication
- **VNet service endpoints** restrict SQL, Storage, and Key Vault to the Container Apps subnet
- **Key Vault RBAC** authorization (no access policies)
- **SQL AAD-only** admin (no SQL authentication)
- **ACR admin disabled** — uses managed identity AcrPull role
- **Storage shared access keys disabled**
