using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation;

public class DirectoryCleaner : ICleaner
{
    private readonly ILogger logger;

    public DirectoryCleaner(ILogger<DirectoryCleaner> logger)
    {
        this.logger = logger;
    }

    public IGenerator Owner { get; set; }
    public string TargetPath { get; set; }
    public bool DryRun { get; set; }
    public SearchOption Depth { get; set; } = SearchOption.TopDirectoryOnly;

    public DirectoryCleaner WithDepth(SearchOption depth)
    {
        Depth = depth;
        return this;
    }

    public Task CleanupAsync(ICollection<string> knownGoodFiles)
    {
        return Task.Run(() =>
        {
            logger.LogTrace($"Cleaning {this}");

            if (!Directory.Exists(TargetPath))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(TargetPath, "*", Depth))
            {
                if (!knownGoodFiles.Any(goodFile => Path.GetFullPath(goodFile) == Path.GetFullPath(file)))
                {
                    Owner.ActionPerformed();
                    logger.LogWarning(
                        (DryRun ? " What if: " : "") + 
                        $"Deleting {file} because it was not in the generation outputs.");
                    if (DryRun) continue;

                    File.Delete(file);

                    // See if the directory that held this file is completely empty
                    // (i.e. it has no child files at any depth). If so, delete it.
                    var dir = Path.GetDirectoryName(file);
                    if (!Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Any())
                    {
                        Directory.Delete(dir, true);
                        logger.LogInformation($"Deleting empty directory {dir}");
                    }
                }
            }
        });
    }

    public override string ToString() => $"{TargetPath}, {Depth}";
}
