# Terraform Infrastructure

This directory contains Terraform configurations for provisioning Azure infrastructure.

## Prerequisites

- [Terraform](https://developer.hashicorp.com/terraform/install)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) - logged in with `az login`
- An Azure subscription

## Getting Started

### 1. Bootstrap

Create the shared resource group, state storage, and CI identity:

```bash
cd bootstrap
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform apply
```

### 2. Configure Variables and GitHub

After apply successfully completes, follow the instructions in your terminal to copy values into `backend.tf` and your GitHub repository's Repository Variables.

Create `dev`, `prod`, and `terraform` GitHub Environments (add **required reviewers** to `terraform` and `prod`).

### 3. Deploy Infrastructure

Run the **Terraform** GitHub Actions workflow with the "Run workflow" button in your repository's Actions tab.

Changes to `terraform/` trigger a plan automatically on PR. Plans are posted as PR comments. To apply, use the **Run workflow** button on the Terraform workflow. A reviewer must approve in the `terraform` environment before apply runs.
