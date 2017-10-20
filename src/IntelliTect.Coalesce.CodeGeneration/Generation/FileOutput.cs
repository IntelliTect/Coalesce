using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class FileOutput : IGenerationOutput
    {
        public string OutputPath { get; private set; }
        public string Contents { get; private set; }

        public FileOutput(string outputPath, Stream stream)
        {
            OutputPath = outputPath;
            using (var reader = new StreamReader(stream))
            {
                Contents = reader.ReadToEnd();
            }
        }

        public FileOutput(string outputPath, string contents)
        {
            OutputPath = outputPath;
            Contents = contents;
        }

        public void Persist(IFileSystem fileSystem)
        {
            fileSystem.WriteAllText(OutputPath, Contents);
        }

        public bool ShouldPersist(IFileSystem fileSystem)
        {
            return true;
        }
    }
}
