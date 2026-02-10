# Plan: Add Terraform Azure Infrastructure to Template

Add a `Terraform` feature flag to the Coalesce template that provisions a complete Azure infrastructure using modules, with two environments (dev/prod), managed identity throughout, and no key-based access. This also requires an updated CI/CD workflow for Container Apps deployment.

## Steps

### 1. Add `Terraform` feature flag

In `template.json` — new boolean symbol, file exclusions for `terraform/` directory when disabled. No dependencies on other flags (Terraform is independently useful).

### 2. Enable SDK Container Support

Add `<EnableSdkContainerSupport>true</EnableSdkContainerSupport>` to the Web project `.csproj` file. This uses .NET's built-in container publishing without needing a Dockerfile.

### 3. Create Terraform modules

Under `terraform/modules/`, one per concern:

- **`resource_group`** — `azurerm_resource_group`
- **`vnet`** — VNet + subnets with `Microsoft.App/environments` delegation (Container Apps) and service endpoints for `Microsoft.Sql`, `Microsoft.Storage`, `Microsoft.KeyVault`, `Microsoft.CommunicationServices`
- **`identity`** — `azurerm_user_assigned_managed_identity` (one per environment)
- **`container_registry`** — `azurerm_container_registry`, role assignment `AcrPull` to managed identity
- **`container_app`** — `azurerm_container_app_environment` + `azurerm_container_app`, health probes pointing at `/health` and `/alive`, identity attached, env vars/secrets from Key Vault references
- **`app_insights`** — `azurerm_application_insights` + `azurerm_log_analytics_workspace`, connection string output
- **`storage`** — `azurerm_storage_account` + `azurerm_storage_container`, `Storage Blob Data Contributor` role to managed identity, `allow_blob_public_access = false`, `shared_access_key_enabled = false`, VNet service endpoint rules
- **`sql`** — `azurerm_mssql_server` (AAD-only admin, no SQL auth) + `azurerm_mssql_database` (DTU model, Basic/S0 for dev, S1+ for prod), VNet service endpoint rules
- **`key-vault`** — `azurerm_key_vault` (RBAC authorization, no access policies), `Key Vault Secrets User` role to managed identity, secrets for connection string and any OAuth credentials, VNet service endpoint rules
- **`acs`** — `azurerm_communication_service`, `Contributor` role to managed identity, outputs endpoint and domain for email sender configuration

### 4. Create `environment` module

At `terraform/modules/environment/` — composes all per-environment modules (vnet, identity, container-app, app-insights, storage, sql, key-vault, acs), accepts `environment_name`, skus/tiers for various resources, tags.

### 5. Create root configuration

At `terraform/` — `main.tf` creates a shared resource group for common infrastructure (ACR), provisions ACR using the `container-registry` module, then calls the `environment` module twice (dev + prod), `variables.tf` for project name/region/subscription, `backend.tf` for Azure Storage remote state, `providers.tf` for `azurerm` provider with required features. Add a `terraform.tfvars.example` with placeholder values.

### 6. Update GitHub Actions workflow

In `build-test-and-deploy.yml` — conditionally (`#if Terraform`): replace `dotnet publish` + App Service deploy with:

- `az acr login --name <acr_name>`
- `dotnet publish -r linux-x64 --self-contained -t:PublishContainer -p:ContainerRegistry=<acr_name>.azurecr.io -p:ContainerImageTags=<version>`
- Deploy to Container App using `azure/container-apps-deploy-action`
- Use `azure/login` with OIDC federated credential instead of publish profile

### 7. Expose health checks unconditionally

In `ProgramServiceDefaults.cs`, map `/health` and `/alive` endpoints outside the `IsDevelopment` guard so Container App probes work in production.

## Further Considerations

### Container Registry and Shared Resource Group

ACR is shared across environments and lives in a dedicated shared resource group at the root level, separate from per-environment resource groups. This allows dev and prod to pull from the same registry while maintaining environment isolation for other resources.

### VNet Service Endpoints

Using VNet service endpoints for SQL, Key Vault, Storage, and ACS. This is simpler than private endpoints (no private DNS zones) while still securing traffic to Azure backbone and restricting access to specific subnets. All resources configured with VNet rules to allow only traffic from Container App subnet.

### GitHub OIDC Federated Credential

For `azure/login` in CI/CD, Terraform should provision an `azuread_application` + `azuread_federated_identity_credential` for the GitHub Actions workflow, or at minimum document the manual setup. Recommend: include a `ci-identity` module that creates the service principal + federated credential, since you're already all-in on IaC.

### Remote state bootstrap

The Terraform backend (Azure Storage for state file) is a chicken-and-egg problem. Recommend: a small `terraform/bootstrap/` config that creates just the storage account for remote state, with instructions to run it first.

### Conditional App Insights + ACS in Terraform

These modules should be conditionally included based on the existing `AppInsights` and `EmailAzure` template flags. Use `#if AppInsights` / `#if EmailAzure` around the relevant module calls and variable references in the environment module, so the Terraform matches the app's configuration.
