using Projects;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.RuntimeInformation;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlDb = OperatingSystem.IsWindows() && OSArchitecture != Architecture.Arm64
    // Assume that localdb is available on Windows (non-ARM)
    ? builder.AddConnectionString("DefaultConnection")
    // Fall back to a container
    : builder.AddSqlServer("sql").WithLifetime(ContainerLifetime.Persistent).AddDatabase(nameof(Coalesce_Starter_Vue_Web));

#if BlobStorage
var blobs = builder.AddAzureStorage("storage").RunAsEmulator().AddBlobContainer("blobs");

#endif
#if KeyVault
var keyVault = builder.AddConnectionString("KeyVault");

#endif
#if (AIChat && !KeyVault)
// https://learn.microsoft.com/en-us/dotnet/aspire/azureai/azureai-openai-integration
var openAi = builder.AddAzureOpenAI("OpenAI");
openAi.AddDeployment(name: "chat", modelName: "gpt-4.1", modelVersion: "2025-04-14")
      .WithProperties(c => c.SkuName = "GlobalStandard");

#endif
var migrations = builder.AddProject<Coalesce_Starter_Vue_Migrations>("migrations")
    .WithReference(sqlDb, "DefaultConnection")
    .WaitFor(sqlDb)
    .WithParentRelationship(sqlDb);

var app = builder.AddProject<Coalesce_Starter_Vue_Web>("app")
    .WithReference(sqlDb, "DefaultConnection")
    .WaitForCompletion(migrations)
#if BlobStorage
    .WithReference(blobs)
#endif
#if KeyVault
    .WithReference(keyVault, optional: true)
#endif
#if (AIChat && !KeyVault)
    .WithReference(openAi)
    .WaitFor(openAi)
#endif
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
