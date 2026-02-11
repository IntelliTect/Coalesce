# Terraform Infrastructure

This directory contains Terraform configurations for provisioning Azure infrastructure.

## Prerequisites

- [Terraform](https://developer.hashicorp.com/terraform/install) (`winget install -e --id Hashicorp.Terraform`)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (`winget install -e --id Microsoft.AzureCLI`)
- An Azure subscription with Contributor access

## Getting Started

### 1. Bootstrap

Terraform tracks what infrastructure it has created in a "state file". For team collaboration and CI/CD, this state must be stored remotely (not on your local machine). The bootstrap step creates this remote storage (an Azure Storage Account) along with a service principal that GitHub Actions will use to deploy infrastructure.

Perform this one-time setup with the following steps:

1. Navigate to the bootstrap directory and create a variables file from the example:
   ```bash
   cd bootstrap
   cp terraform.tfvars.example terraform.tfvars
   ```

2. Edit `terraform.tfvars`, filling in your own values. 
    - `project_name` should not exceed 17 characters due to Azure Storage Account name limitations. It should only contain lowercase letters and hyphens.
    - `subscription_id` should be the Azure subscription where you intend to deploy your Azure resources.
    - `github_repository` is the GitHub repo where your code and actions will live and run, in `org-name/repo-name` format. This is used to setup federated authentication to Azure.

3. Initialize Terraform (downloads required providers):
   ```bash
   terraform init
   ```

4. Apply the configuration to create resources:
   ```bash
   terraform apply
   ```
   Review the plan and type `yes` when prompted. If you encounter errors, resolve them and then apply again until completely successful.

### 2. Configure Variables and GitHub

After apply successfully completes, follow the instructions in your terminal to copy values into `backend.tf` and your GitHub repository's Repository Variables.

Create `dev`, `prod`, and `terraform` GitHub Environments. Add **required reviewers** to all environments - otherwise, the Terraform apply step will run automatically into each environment with no chance for intervention.

### 3. Deploy Infrastructure

Perform a run of the **Terraform** GitHub Actions workflow with the "Run workflow" button in your repository's Actions tab to setup all remaining infrastructure. Continue to do this every time you make additional changes to your Terraform.

Pull Request changes to `terraform/` trigger a plan automatically, and plans are posted as PR comments.