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
cp ../terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values, and add storage_account_name and github_repository
terraform init
terraform apply
```

Copy the `backend_config` output into `../backend.tf` (uncomment and fill in the values).
Copy the `github_vars_summary` output into your GitHub repository settings (Variables).
Create `dev`, `prod`, and `terraform` GitHub Environments (add **required reviewers** to `terraform` and `prod`).

### 2. Deploy Infrastructure

Run the **Terraform** GitHub Actions workflow with the "Run workflow" button on the action within your repository's Actions on GitHub.com.

Changes to `terraform/` trigger a plan automatically on PR. Plans are posted as PR comments. To apply, use the **Run workflow** button on the Terraform workflow. A reviewer must approve in the `terraform` environment before apply runs.
