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

        public override async Task GenerateAsync()
        {
            if (IsDisabled)
            {
                Logger?.LogDebug($"Skipped gen of {this}, {DisabledJsonPropertyName} was false");
                return;
            }

            if (!ShouldGenerate())
            {
                Logger?.LogDebug($"Skipped gen of {this}, {nameof(ShouldGenerate)} returned false");
                return;
            }

            using (var contents = await GetOutputAsync())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

                if (!await FileUtilities.HasDifferencesAsync(contents, OutputPath))
                {
                    Logger?.LogTrace($"Skipped write of {this}, existing file wasn't different");
                    return;
                }

                var isRegen = File.Exists(OutputPath);
                using (FileStream fileStream = new FileStream(OutputPath, FileMode.Create, FileAccess.Write))
                {
                    contents.Seek(0, SeekOrigin.Begin);
                    await contents.CopyToAsync(fileStream);

                    Uri relPath = new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(OutputPath));
                    Logger?.LogInformation($"{(isRegen ? "Reg" : "G")}enerated: {Uri.UnescapeDataString(relPath.OriginalString)}");
                };
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
    }
}
