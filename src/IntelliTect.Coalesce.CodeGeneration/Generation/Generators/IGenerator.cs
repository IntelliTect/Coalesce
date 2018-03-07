using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.TypeDefinition;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public interface IGenerator
    {
        string DefaultOutputPath { get; set; }

        string EffectiveOutputPath { get; }

        bool IsDisabled { get; }

        Task GenerateAsync();

        void Configure(JObject obj);
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
        // Composite interface - no additional members; just shorthand for the two sub-interfaces.
    }

    public interface IRootGenerator : ICompositeGenerator<ReflectionRepository>
    {
        // Nothing special about this right now, but there may be in the future.
    }

    public interface IFileGenerator : IGenerator
    {
        Task<Stream> GetOutputAsync();
    }

    public interface IFileGenerator<TModel> : IGenerator<TModel>, IFileGenerator
    {
    }
}
