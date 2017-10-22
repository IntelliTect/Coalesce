using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public interface IGenerator
    {
        string OutputPath { get; set; }

        bool Validate();

        Task GenerateAsync();

        //bool ShouldGenerate(IFileSystem fileSystem);
    }

    public interface IGenerator<TModel> : IGenerator
    {
        TModel Model { get; set; }
    }

    public interface ICompositeGenerator<TModel> : IGenerator<TModel>
    {
        IEnumerable<IGenerator> GetGenerators();
    }

    public interface IFileGenerator<TModel> : IGenerator<TModel>
    {
        TemplateDescriptor Template { get; }

        Task<Stream> GetOutputAsync();
    }
}
