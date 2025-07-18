# Template Features

This page provides in-depth details about some of the more advanced template options available on the [Getting Started](../stacks/vue/getting-started.md) page.

## Identity

Enabling the Identity template parameter integrates the fundamentals of [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity). This includes storing users in your application database and facilitating sign-in, authentication, and authorization. Most Identity configuration is located in `ProgramAuthConfiguration.cs`, with the implementation spread across classes in the `Auth` folder of your data project and the Razor Pages in the `Pages` directory of your web project.

A custom `ClaimsPrincipalFactory` is provided to construct the claims available on each user's `ClaimsPrincipal` (typically accessed using extension methods you can add to `ClaimsPrincipalExtensions`). Additional details concerning the Tenancy template option are detailed in the Tenancy section.

Roles are designed to be created and modified by application administrators as needed, rather than being hardcoded. The values you'll use in your [security attributes](../modeling/model-components/attributes/security-attribute.md) and other [security enforcement](./security.md) mechanisms are **Permissions**, defined by the `Permission` enum in your application. A role can grant multiple permissions, and a user can belong to multiple roles. Permissions usually represent specific actions a user can perform, while roles typically map to job functions or titles.

External/third-party authentication options tied to Identity follow Microsoft's standard patterns for external providers. Note that tokens from external providers are only used during sign-in and are discarded afterward in favor of your application's first-party authentication cookies. However, you can enable token storage by uncommenting the `UpdateExternalAuthenticationTokensAsync` call in the `SignInExternalUser` method in `ExternalLogin.cshtml.cs`. It is up to you to decide when and why to retrieve, refresh, or use these tokens.

## Tenancy

The Tenancy feature of the Coalesce project template introduces several areas of code that result in a multi-tenant application that is easy to develop without having to constantly worry about the specific mechanisms of multi-tenancy. 

The short version is that a `TenantId` column is added to all tenanted data in your database, with hooks in your Entity Framework DbContext that automatically handle this column.

### Database

All entity classes that belong to a tenant must implement the provided `ITenanted` interface. In most cases, this is achieved by inheriting from `TenantedBase`, which includes recommended configuration attributes for the `Tenant` and `TenantId` properties.

The `TenantId` property on `AppDbContext` serves as the single source of truth for determining which tenant your application code operates against. When handling authenticated HTTP requests, this property is set in the `OnValidatePrincipal` hook that fires after the incoming authentication cookie is read. For other scenarios, you'll need to assign this some other way while keeping security in mind.

`ClaimsPrincipalFactory` determines which tenant a user is signed into via the `TenantId` claim. By default, this will be the existing authenticated tenant when refreshing claims, or the user's oldest tenant membership when handling a new sign-in. It can also reject a user from accessing any real tenant by signing them into the `NullTenantId`.

Creating new records in your database (via Entity Framework's `SaveChanges`/`SaveChangesAsync`) is handled by the `TenantInterceptor`. This interceptor attaches the current `TenantId` to all tenanted entities being created. Note that this only works through SaveChanges calls and does not intercept data created via raw SQL statements.

Querying tenanted data is managed by a global query filter dynamically applied to all tenanted entities in the `ConfigureTenancy` method of your `AppDbContext`. This filter applies a simple `entity.TenantId == db.TenantIdOrThrow` predicate to all queries, eliminating the need to manually filter data in your application code.

Referential integrity in the database is configured to ensure that data cannot "leak" between tenants. In the `ConfigureTenancy` method, the primary key of all tenanted tables is modified to include the `TenantId` as the first ordinal column of the PK. Foreign key constraints are updated accordingly. This approach clusters tenant data together in the database (improving performance) and prevents [foreign key injection](../topics/security.md#foreign-key-injection-vulnerabilities) from linking data between tenants.

### Identity & Users

The `User` entity (`AspNetUsers` table) is not a tenanted entity. Users exist independently of tenants and can belong to zero or more tenants.

User membership in tenants is managed through the `TenantMembership` table. Other template options for tenant creation and membership interact with this table to assign users to tenants. You can add additional properties/columns to this type to store tenant-specific user information.

The `Role` and `UserRole` entities (`AspNetRoles` and `AspNetUserRoles` tables) become tenanted entities when the Tenancy template parameter is enabled. This allows roles and role memberships to be tenant-specific, which is essential when a single user can belong to multiple tenants.

A special role, `GlobalAdmin`, is used to control actions reserved for system administrators. This role is not stored in the `AspNetRoles` table but as a boolean property on the `User` entity since it does not belong to any specific tenant.

### Background Jobs

The recommended way of handling background jobs in a Coalesce multi-tenant application (e.g. when using Hangfire) is to only handle a single tenant per job invocation. If you have jobs that need to loop over all tenants, it is usually best to have that master job queue up an individual job for each tenant, rather than trying to process data for all tenants in one job. This avoids the complexity and risk of having single instances of your application's service classes handle and process data from more than one tenant in their lifecycle.

Or, if the above is not feasible, at least [create a new service scope](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescopefactory.createscope) per tenant and execute handling of each tenant only using services obtained from that scope.



## AI Chat
<Beta/> 

The AI Chat feature adds Microsoft Semantic Kernel integration with Azure OpenAI to provide an AI-powered chat assistant in your application. This feature includes a pre-configured chat agent service and a Vue component for user interaction.

### Configuration

Azure OpenAI can be configured in two ways: through .NET Aspire (recommended) or by manually configuring connection strings.

#### Using .NET Aspire (Recommended)

When launching with the Aspire AppHost, Azure OpenAI will configure and provision cloud resources automatically with [Aspire's local provisioning](https://learn.microsoft.com/en-us/dotnet/aspire/azure/local-provisioning#configuration). Configure the required Azure settings in your AppHost project's `appsettings.json`, `appsettings.localhost.json`, or `dotnet user-secrets`:

```json
{
  "Azure": {
    "SubscriptionId": "<Your subscription id>",
    "AllowResourceGroupCreation": true,
    "ResourceGroup": "<Valid resource group name>",
    "Location": "<Valid Azure location>" // e.g. westus3
  }
}
```

If there's an existing resource you'd rather use instead of provisioning a new resource, see the documentation for [Connecting to an existing Azure OpenAI service](https://learn.microsoft.com/en-us/dotnet/aspire/azureai/azureai-openai-integration?tabs=package-reference#connect-to-an-existing-azure-openai-service).

For more details on Aspire's Azure OpenAI integration, see the [official documentation](https://learn.microsoft.com/en-us/dotnet/aspire/azureai/azureai-openai-integration).

#### Manual Configuration (Without Aspire)

If you're not using Aspire, you can configure Azure OpenAI connection strings directly in your application settings:

Including a Key is optional; if omitted, [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential) will be used to authenticate.

**appsettings.json:** or **appsettings.localhost.json**
```json
{
  "ConnectionStrings": {
    "OpenAI": "Endpoint=https://your-resource.openai.azure.com/;Key=your-api-key"
  }
}
```

**Using User Secrets (recommended for local development):**
```bash
dotnet user-secrets set "ConnectionStrings:OpenAI" "Endpoint=https://your-resource.openai.azure.com/;Key=your-api-key"
```

### Implementation

The AI Chat template option includes:

- **AIAgentService.cs**: A Coalesce service that handles chat interactions using Semantic Kernel agents
- **AIChat.vue**: A front-end chat interface for interacting with the AI agent.

The chat agent is configured with:
- Automatic function calling capabilities
- Chat history summarization to manage token limits
- Built-in error handling for rate limits and service unavailability
- Tamper-proof chat history for maintaining context in a conversation

### Usage

Once configured, users can access the AI chat through the included Vue component. The chat agent can be extended with additional functionality by applying the [`SemanticKernel`](/modeling/model-components/attributes/semantic-kernel.md) attribute to your entities, methods, and data sources to expose them as AI-callable functions.