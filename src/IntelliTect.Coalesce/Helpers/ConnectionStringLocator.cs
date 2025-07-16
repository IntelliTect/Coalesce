using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IntelliTect.Coalesce.Helpers;

public static class DevelopmentConnectionStringLocator
{
    /// <summary>
    /// Helper for use in IDesignTimeDbContextFactory implementations.
    /// Do not use in regular production/runtime code.
    /// Attempts to find a connection string in sibling projects' config files.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to find (usually "DefaultConnection").</param>
    /// <param name="configFileNames">The config file names to search, in order of precedence. E.g. ["appsettings.json"].</param>
    /// <param name="projectDirectorySuffixes">The project directory suffixes to search for (e.g. [".AppHost", ".Web"]).</param>
    /// <returns>The connection string if found, otherwise null.</returns>
    public static string? Find(
        string connectionStringName = "DefaultConnection",
        string[]? configFileNames = null,
        string[]? projectDirectorySuffixes = null)
    {
        var curDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        configFileNames ??= [
            "appsettings.localhost.json",
            "appsettings.Development.json",
            "appsettings.json"
        ];
        projectDirectorySuffixes ??=
        [
            ".AppHost",
            ".Web"
        ];

        while (curDirectory != null)
        {
            foreach (var projectSuffix in projectDirectorySuffixes)
            {
                var projectDir = curDirectory
                    .EnumerateDirectories()
                    .FirstOrDefault(d => d.Name.EndsWith(projectSuffix, StringComparison.OrdinalIgnoreCase));

                if (projectDir == null) continue;

                var configPaths = configFileNames
                    .Select(f => Path.Combine(projectDir.FullName, f))
                    .Where(File.Exists);

                foreach (var configFile in configPaths)
                {
                    try
                    {
                        var json = File.ReadAllText(configFile);
                        var documentOptions = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };
                        var root = JsonNode.Parse(json, null, documentOptions);
                        var connStrings = root?["ConnectionStrings"];
                        if (connStrings is JsonObject)
                        {
                            var conn = connStrings[connectionStringName]?.GetValue<string>();
                            if (!string.IsNullOrWhiteSpace(conn))
                            {
                                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), configFile);
                                Console.WriteLine($"Found connection string {connectionStringName} in {relativePath}");
                                return conn;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore and continue searching
                    }
                }
            }

            if (
                curDirectory != null && (
                curDirectory.EnumerateDirectories(".git").Any() ||
                curDirectory.EnumerateFiles("*.sln").Any() ||
                curDirectory.EnumerateFiles("*.slnx").Any()
            ))
            {
                // We reached the top of the project. Stop looking.
                return null;
            }

            curDirectory = curDirectory?.Parent;
        }

        return null;
    }
}