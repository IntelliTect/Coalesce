using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlDb = 
    builder.ExecutionContext.IsPublishMode ? builder
        // Publish to Azure SQL.
        .AddAzureSqlServer("sql")
        .AddDatabase("sqldb")
    : OperatingSystem.IsWindows() ? builder
        // Assume that localdb is available on Windows
        .AddConnectionString("sqldb")
    : builder
        // Fall back to a container
        .AddSqlServer("sql")
        .WithLifetime(ContainerLifetime.Persistent)
        .AddDatabase("sqldb");

var app = builder.AddProject<Coalesce_Starter_Vue_Web>("app")
    .WithReference(sqlDb)
    .WaitFor(sqlDb)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
