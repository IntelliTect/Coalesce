using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class CompositeGenerator<TModel> : ICompositeGenerator<TModel>
    {
        public IServiceProvider ServiceProvider { get; }

        public CompositeGenerator(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected TGenerator Generator<TGenerator>()
            where TGenerator : IGenerator
        {
            var generator = ActivatorUtilities.CreateInstance<TGenerator>(ServiceProvider);
            generator.OutputPath = OutputPath;
            return generator;
        }

        public string OutputPath { get; set; }
        public TModel Model { get; set; }

        public async Task GenerateAsync()
        {
            var generators = GetGenerators().ToList();

            // MUST evaluate the enumerable here before passing to Task.WhenAll. Otherwise, these will only be processed one by one.
            List<Task> tasks = generators.Select(g => g.GenerateAsync()).ToList();

            await Task.WhenAll(tasks);
        }

        public abstract IEnumerable<IGenerator> GetGenerators();

       // public virtual bool ShouldGenerate(IFileSystem fileSystem) => true;

        public virtual bool Validate() => true;
    }
}
