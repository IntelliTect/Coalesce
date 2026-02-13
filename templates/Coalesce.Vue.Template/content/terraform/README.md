# Terraform Infrastructure

This directory contains Terraform configurations for provisioning Azure infrastructure.

## Prerequisites

- [Terraform](https://developer.hashicorp.com/terraform/install) (`winget install -e --id Hashicorp.Terraform`)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (`winget install -e --id Microsoft.AzureCLI`)
- An Azure subscription with Contributor access
- For some features (Azure App Registrations and SQL Admin Groups), access to a colleague who has directory-level admin permissions.

## Getting Started

### 1. Bootstrap

Terraform tracks what infrastructure it has created in a "state file". For team collaboration and CI/CD, this state must be stored remotely (not on your local machine). The bootstrap step creates this remote storage (an Azure Storage Account) along with a service principal that GitHub Actions will use to deploy infrastructure. It also creates a "Developers" AAD group that can be easily used to grant project team members access to lower environment resources.

Perform this one-time setup with the following steps:

1. Navigate to the bootstrap directory and create a variables file from the example:

   ```bash
   cd bootstrap
   cp terraform.tfvars.example terraform.tfvars
   ```

2. Edit `terraform.tfvars`, filling in your own values.
   - `project_name` must be lowercase alphanumeric (a–z, 0–9) and should not exceed 16 characters to allow for environment and resource suffixes, which generally have a total limit of 24-characters.
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

## Azure App Registrations

If you selected to include Sign-in with Microsoft in this Coalesce project, the Terraform will be configured to create an Azure App Registration in each environment.

In order for this to succeed, one of the following must occur:

### Option 1: Grant Unscoped Cloud Application Administrator

This option is the simplest, but also requires wide permissions across your entire Azure tenant.

1. Find the CLIENT_ID of the CI Identity that was created in the bootstrap step. This will be the same value that you put in your GitHub repository variables.
2. Grant that principal the [Cloud Application Administrator](https://portal.azure.com/#view/Microsoft_AAD_IAM/RoleMenuBlade/~/RoleMembers/objectId/158c047a-c907-4556-b7ef-446551a6b5f7/roleName/Cloud%20Application%20Administrator/roleTemplateId/158c047a-c907-4556-b7ef-446551a6b5f7/adminUnitObjectId//customRole~/false/resourceScope/%2F) role within Azure Entra ID.
3. After observing a few minutes of propagation time, run or re-run Terraform apply, which should now be able to create the App Registration.

### Option 2: Grant Scoped Cloud Application Administrator

This option uses strictly scoped role assignments so that your CI identity cannot manage other, unrelated applications within your Entra tenant. This is the most ideal long-term option, but it also requires the most work because the CI identity won't be able to initially create the app registration through Terraform.

1. [Create a new App Registration](https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps), either through the Azure Portal or with the Azure CLI. The details you provide during creation do not matter - not even the name - they'll all become managed by Terraform. After creating the app registration, make a note of its `Object ID` (Azure Portal) or `id` (Azure CLI) property.

2. Find the `Object (principal) ID` of the CI User-Assigned Managed Identity that was created in the bootstrap step. In the Azure Portal, navigate to the shared resource group (named with your project name and `-shared` suffix), click on the Managed Identity resource, and copy the `Object (principal) ID` value from the Overview page.

3. Navigate to the Cloud Application Administrator role from [Privileged Identity Management](https://portal.azure.com/#view/Microsoft_Azure_PIMCommon/ResourceMenuBlade/~/roles/resourceId//resourceType/tenant/provider/aadroles) and grant a permanent assignment to the CI identity (using the Object ID from step 2), scoped to the App Registration from step 1. Alternatively, via the Azure CLI, fill in the two placeholder values and run:

   ```pwsh
   echo '{"roleDefinitionId":"158c047a-c907-4556-b7ef-446551a6b5f7", "principalId":"CI_IDENTITY_OBJECT_ID", "directoryScopeId":"/APP_REG_OBJECT_ID"}' | az rest --method POST --uri "https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments" --headers 'Content-Type=application/json' --body "@-"
   ```

4. In the root `main.tf` file, add import block(s) so that Terraform can begin managing the app registration. For example, for the dev environment, using the Object ID from step 1:

   ```hcl
   import {
      to = module.dev.module.app_registration.azuread_application.this
      id = "/applications/<APP_REG_OBJECT_ID>"
   }
   ```

5. Once the above terraform has been applied by the Terraform GitHub action, the import block can be deleted.

### Option 3: Fully Manual App Registration

The last and least ideal option is to create and manage the App Registration entirely manually.

1. Delete the `app_registration` Terraform module and its usages.
2. Manually create an App Registration, either in the Azure Portal or with the Azure CLI.
3. Configure the App Registration as desired. Add sign-in redirect URLs in the form `https://my-domain/signin-microsoft`. Generate a Client Secret and manually add it to the environment's Key Vault with the identifier `Authentication--Microsoft--ClientSecret`, with the app registration's Client Id stored into `Authentication--Microsoft--ClientId`.

## SQL Admin Groups

By default, the SQL server's AAD administrator is set to the application's managed identity. This allows the application to connect, but doesn't allow developers to connect directly.

To enable developer access, you can configure Terraform to create an AAD group as the SQL administrator, with the app identity and a developers group as members.

### Option 1: Grant Group.Create Permission

This option lets Terraform fully manage the group, but requires granting the CI identity the `Group.Create` Microsoft Graph permission.

1. Find the **Object (principal) ID** of the CI User-Assigned Managed Identity. In the Azure Portal, navigate to the shared resource group and click on the Managed Identity resource.

2. Run the following Powershell, replacing both occurrences of `CI_IDENTITY_OBJECT_ID` with the value from step 1:

   ```pwsh
   echo '{"principalId":"CI_IDENTITY_OBJECT_ID","resourceId":"GRAPH_SP_ID","appRoleId":"bf7b1a76-6e77-406b-b258-bf5c7720e98f"}'.Replace("GRAPH_SP_ID", $(az ad sp list --filter "appId eq '00000003-0000-0000-c000-000000000000'" --query "[0].id" -o tsv)) | az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/CI_IDENTITY_OBJECT_ID/appRoleAssignments" --headers "Content-Type=application/json" --body "@-"
   ```

   Alternatively, use the Azure Portal to grant admin consent to the Graph `Group.Create` permission through to the principal (in the Security > Permissions section when looking at the identity's corresponding 'Enterprise Application').

3. Uncomment the AAD group resources in `modules/sql/main.tf` and the `admin_principals` parameter in `modules/environment/main.tf`.

### Option 2: Manually Create Group and Import

This option avoids granting broad Graph API permissions. Instead, you create the group manually, make the CI identity an owner so it can manage membership, and then import the group into Terraform.

1. Find the **Object (principal) ID** of the CI User-Assigned Managed Identity. In the Azure Portal, navigate to the shared resource group and click on the Managed Identity resource.

2. Create the AAD security group manually, e.g. for the `dev` environment:

   ```bash
   az ad group create --display-name "<project-name>-dev-sql-admins" --mail-nickname "<project-name>-dev-sql-admins" --security-enabled
   ```

   Note its `id` (Object ID) from the output.

3. Add the CI identity as an **owner** of the group so that Terraform can manage its membership:

   ```bash
   az ad group owner add --group "<project-name>-dev-sql-admins" --owner-object-id CI_IDENTITY_OBJECT_ID
   ```

4. Uncomment the AAD group resources in `modules/sql/main.tf` and the `admin_principals` parameter in `modules/environment/main.tf`.

5. Add an import block in the root `main.tf` so Terraform adopts the existing group. Replace `<GROUP_OBJECT_ID>` with the Object ID from step 2:

   ```hcl
   import {
      to = module.dev.module.sql.azuread_group.sql_admins
      id = "/groups/<GROUP_OBJECT_ID>"
   }
   ```

6. Run or re-run the Terraform GitHub Action. Once the import has been applied successfully, you can remove the import block.
