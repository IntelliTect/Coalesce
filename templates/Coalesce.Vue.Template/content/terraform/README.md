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

After infrastructure is provisioned, use the GitHub Actions workflow to build images and deploy them to environments for CI/CD.
