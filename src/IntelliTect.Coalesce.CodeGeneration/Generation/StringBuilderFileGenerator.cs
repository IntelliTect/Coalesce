using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class StringBuilderFileGenerator<TModel> : FileGenerator, IFileGenerator<TModel>
    {
        public StringBuilderFileGenerator(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType().Name);
        }

        public TModel Model { get; set; }

        public abstract Task<string> BuildOutputAsync();

        public override async Task<Stream> GetOutputAsync()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(await BuildOutputAsync()));
        }

        public override string ToString()
        {
            if (OutputPath != null)
            {
                return $"{GetType().FullName} => {OutputPath}";
            }
            return GetType().FullName;
        }
    }
}
