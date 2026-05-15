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
        // Read version directly from the installed package's package.json.
        // This works regardless of the package manager (npm, pnpm, yarn).
        var packageJsonPath = Path.Combine(context.WebProject.ProjectPath, "node_modules", packageName, "package.json");
        var fileInfo = new FileInfo(packageJsonPath);
        if (!fileInfo.Exists)
        {
            logger.LogDebug("Unable to determine installed {Package} version: {Path} not found.", packageName, packageJsonPath);
            return null;
        }

        try
        {
            using var stream = fileInfo.OpenRead();
            var content = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
            return content.GetProperty("version").GetString();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Unable to determine installed {Package} version", packageName);
            return null;
        }
    }
}
