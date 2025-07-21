using IntelliTect.Coalesce.CodeGeneration.Generation;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

namespace IntelliTect.Coalesce.CodeGeneration.Analysis;

internal class NpmDependencyAnalayzer(GenerationContext context, ILogger<NpmDependencyAnalayzer> logger)
{
    public async Task<string?> GetNpmPackageVersion(string packageName)
    {
        var packageLockPath = Path.Combine(context.WebProject.ProjectPath, "package-lock.json");
        var fileInfo = new FileInfo(packageLockPath);
        if (!fileInfo.Exists)
        {
            logger.LogDebug("Unable to determine installed coalesce-vue version: package-lock.json not found.");
            return null;
        }

        try
        {
            var content = await JsonSerializer.DeserializeAsync<JsonElement>(fileInfo.OpenRead());
            var version = content
                .GetProperty("packages")
                .GetProperty("node_modules/" + packageName)
                .GetProperty("version")
                .GetString();

            return version;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Unable to determine installed coalesce-vue version");
            return null;
        }
    }
}
