using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{

    public abstract class CompositeGenerator<TModel> : Generator, ICompositeGenerator<TModel>
    {
        private readonly CompositeGeneratorServices _services;

        public TModel Model { get; set; }

        public List<ICleaner> Cleaners { get; protected set; } = new List<ICleaner>();

        public CompositeGenerator(CompositeGeneratorServices services) : base(services)
        {
            this._services = services;
        }

        protected TGenerator Generator<TGenerator>()
            where TGenerator : IGenerator
        {
            var generator = ActivatorUtilities.CreateInstance<TGenerator>(_services.ServiceProvider);
            generator.OutputPath = OutputPath;
            return generator;
        }

        protected TCleaner Cleaner<TCleaner>()
            where TCleaner : ICleaner
        {
            var cleaner = ActivatorUtilities.CreateInstance<TCleaner>(_services.ServiceProvider);
            cleaner.Owner = this;
            cleaner.TargetPath = OutputPath;
            return cleaner;
        }

        public sealed override async Task GenerateAsync()
        {
            // Flatten out all generators.
            // This includes all FileGenerators and all CompositeGenerators in the hierarchy.
            IEnumerable<IGenerator> Flatten(ICompositeGenerator composite, int depth = 0)
            {
                var prefix = string.Concat(Enumerable.Repeat("  |", depth));

                if (composite.IsDisabled)
                {
                    Logger.LogDebug($"{prefix} {composite.GetType().FullName} => DISABLED");
                    yield break;
                }

                Logger.LogDebug($"{prefix} {composite.GetType().FullName} => {composite.OutputPath}");

                prefix = string.Concat(Enumerable.Repeat("  |", depth + 1));

                foreach (var generator in composite.GetGenerators().OrderBy(g => g.GetType().FullName))
                {
                    if (generator.IsDisabled)
                    {
                        Logger.LogDebug($"{prefix} {generator.GetType().FullName} => DISABLED");
                        yield break;
                    }
                    else if (generator is ICompositeGenerator childComposite)
                    {
                        foreach (var childGen in Flatten(childComposite, depth + 1)) yield return childGen;
                    }
                    else
                    {
                        Logger.LogDebug($"{prefix} {generator.GetType().FullName} => {generator.OutputPath}");
                    }

                    yield return generator;
                }
            }

            var allGenerators = Flatten(this).ToList();
            Logger.LogDebug("---");

            var fileGenerators = allGenerators.OfType<IFileGenerator>().ToList();
            var compositeGenerators = allGenerators.OfType<ICompositeGenerator>().ToList();
            var outputtedFiles = fileGenerators.Select(g => g.OutputPath).ToList();
            var cleaners = compositeGenerators.SelectMany(g => g.GetCleaners()).ToList();

            await Task.WhenAll(fileGenerators
                // Kick off generate tasks in parallel. 
                // This is especially essential for any generators that complete synchronously
                // (including all string builder generators)
                .AsParallel()
                .Select(g => g.GenerateAsync())

                // Coerce all of our generators to a collection before awiting them all. If we don't do this, 
                // then for some reason, the first few tasks are completing in sequence instead of in parallel.
                .ToArray()
            );


            Logger.LogDebug($"Running {cleaners.Count} cleaners");
            await Task.WhenAll(cleaners.Select(c => c.CleanupAsync(outputtedFiles)).ToList());
        }

        public abstract IEnumerable<IGenerator> GetGenerators();
        public virtual IEnumerable<ICleaner> GetCleaners() => Enumerable.Empty<ICleaner>();

        public override string ToString()
        {
            if (OutputPath != null)
            {
                return $"{this.GetType().Name} => {OutputPath}";
            }

            return this.GetType().Name;
        }
    }
}
