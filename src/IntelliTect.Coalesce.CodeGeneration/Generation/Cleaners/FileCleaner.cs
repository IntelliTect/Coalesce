using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class FileCleaner : ICleaner
    {
        private readonly ILogger logger;

        public FileCleaner(ILogger<DirectoryCleaner> logger)
        {
            this.logger = logger;
        }

        public IGenerator Owner { get; set; }
        public string TargetPath { get; set; }
        public bool DryRun { get; set; }

        public Task CleanupAsync(ICollection<string> knownGoodFiles)
        {
            return Task.Run(() =>
            {
                logger.LogTrace($"Cleaning {this}");
                if (File.Exists(TargetPath))
                {
                    logger.LogWarning(
                        (DryRun ? " What if: " : "") + 
                        $"Deleting {TargetPath} because it was explicitly flagged for removal."
                    );
                    if (!DryRun)
                    {
                        File.Delete(TargetPath);
                    }
                }
            });
        }

        public override string ToString() => $"{TargetPath}";
    }
}
