using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public interface IGenerator
    {
        string OutputPath { get; set; }

        Task GenerateAsync();
    }

    public interface IGenerator<TModel> : IGenerator
    {
        TModel Model { get; set; }
    }

    public interface ICompositeGenerator : IGenerator
    {
        IEnumerable<IGenerator> GetGenerators();

        IEnumerable<ICleaner> GetCleaners();
    }

    public interface ICompositeGenerator<TModel> : IGenerator<TModel>, ICompositeGenerator
    {
    }

    public interface IFileGenerator : IGenerator
    {
        Task<Stream> GetOutputAsync();
    }

    public interface IFileGenerator<TModel> : IGenerator<TModel>, IFileGenerator
    {
    }
}
