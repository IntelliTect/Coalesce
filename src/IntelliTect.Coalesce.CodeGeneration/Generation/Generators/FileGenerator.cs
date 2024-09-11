using IntelliTect.Coalesce.CodeGeneration.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{

    public abstract class FileGenerator : Generator, IFileGenerator
    {
        protected FileGenerator(GeneratorServices services) : base(services) { }

        public override string EffectiveOutputPath =>
            string.IsNullOrWhiteSpace(TargetDirectory)
            // No configured output - just use the normal output path.
            ? DefaultOutputPath
            // User has configured an output location - insert this at the end of the directory part of the path.
            : Path.Combine(Path.GetDirectoryName(DefaultOutputPath), TargetDirectory, Path.GetFileName(DefaultOutputPath));

        public override async Task GenerateAsync()
        {
            if (IsDisabled)
            {
                Logger?.LogDebug($"Skipped gen of {this}, generator disabled");
                return;
            }

            if (!ShouldGenerate())
            {
                Logger?.LogDebug($"Skipped gen of {this}, {nameof(ShouldGenerate)} returned false");
                return;
            }

            // Start reading the existing output file while we generate the contents.
            string outputFile = EffectiveOutputPath;
            var outputExistingContents = Task.Run(() =>
            {
                // Throw the creation of the directory on this thread too for max parallelization.
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                if (File.Exists(outputFile))
                {
                    return File.ReadAllText(outputFile);
                }
                else
                {
                    return String.Empty;
                }
            });

            using (var contents = await GetOutputAsync().ConfigureAwait(false))
            {
                Logger?.LogTrace($"Got output for {this}");

                if (!(await FileUtilities.HasDifferencesAsync(contents, await outputExistingContents)))
                {
                    Logger?.LogTrace($"Skipped write of {this}, existing file wasn't different");
                    return;
                }

                var isRegen = File.Exists(outputFile);

                if (!DryRun)
                {
                    using FileStream fileStream = new(outputFile, FileMode.Create, FileAccess.Write);
                    contents.Seek(0, SeekOrigin.Begin);
                    await contents.CopyToAsync(fileStream).ConfigureAwait(false);
                    await fileStream.FlushAsync();
                }

                Uri relPath = new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(outputFile));

                this.ActionPerformed();
                Logger?.LogInformation(
                    (DryRun ? " What if: " : "") + 
                    $"{(isRegen ? "Reg" : "G")}enerated: {Uri.UnescapeDataString(relPath.OriginalString)}"
                );
            }
        }

        /// <summary>
        /// Override to add logic that determines whether or not the generator needs to run or not.
        /// 
        /// Generators that are conditional on the state of the filesytem should perform that check in here.
        /// Checking the filesystem should not be done inside GetGenerators() on an ICompositeGenerator.
        /// </summary>
        /// <returns>False if the generator should not generate output nor persiste it to disk.</returns>
        public virtual bool ShouldGenerate() => true;

        public abstract Task<Stream> GetOutputAsync();

        public override string ToString()
        {
            if (EffectiveOutputPath != null)
            {
                return $"{GetType().Name} => {EffectiveOutputPath}";
            }
            return GetType().Name;
        }
    }
}
