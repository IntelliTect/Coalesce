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

var app = builder.AddProject<Coalesce_Starter_Vue_Web>("app")
    .WithReference(sqlDb, "DefaultConnection")
    .WaitFor(sqlDb)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
