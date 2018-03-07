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

        public override string EffectiveOutputPath =>
            string.IsNullOrWhiteSpace(TargetDirectory)
            // No configured output - just use the normal output path.
            ? DefaultOutputPath
            // User has configured an output location - insert this at the end of the path.
            : Path.Combine(DefaultOutputPath, TargetDirectory);

        public CompositeGenerator(CompositeGeneratorServices services) : base(services)
        {
            this._services = services;
        }

        protected TGenerator Generator<TGenerator>()
            where TGenerator : IGenerator
        {
            var generator = ActivatorUtilities.CreateInstance<TGenerator>(_services.ServiceProvider);
            generator.DefaultOutputPath = EffectiveOutputPath;
            return generator;
        }

        protected TCleaner Cleaner<TCleaner>()
            where TCleaner : ICleaner
        {
            var cleaner = ActivatorUtilities.CreateInstance<TCleaner>(_services.ServiceProvider);
            cleaner.Owner = this;
            cleaner.TargetPath = EffectiveOutputPath;
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

                Logger.LogDebug($"{prefix} {composite.GetType().FullName} => {composite.EffectiveOutputPath}");

                prefix = string.Concat(Enumerable.Repeat("  |", depth + 1));

                foreach (var generator in composite.GetGenerators().OrderBy(g => g.GetType().FullName))
                {
                    if (generator.IsDisabled)
                    {
                        Logger.LogDebug($"{prefix} {generator.GetType().FullName} => DISABLED");
                        continue;
                    }
                    else if (generator is ICompositeGenerator childComposite)
                    {
                        foreach (var childGen in Flatten(childComposite, depth + 1)) yield return childGen;
                    }
                    else
                    {
                        Logger.LogDebug($"{prefix} {generator.GetType().FullName} => {generator.EffectiveOutputPath}");
                    }

                    yield return generator;
                }
            }

            var allGenerators = Flatten(this).ToList();

            var fileGenerators = allGenerators.OfType<IFileGenerator>().ToList();
            var compositeGenerators = allGenerators.OfType<ICompositeGenerator>().ToList();
            var outputtedFiles = fileGenerators.Select(g => g.EffectiveOutputPath).ToList();
            var cleaners = compositeGenerators.SelectMany(g => g.GetCleaners()).ToList();

            await Task.WhenAll(fileGenerators
                .AsParallel()
                .Select(g => g.GenerateAsync()));


            Logger.LogDebug($"Running {cleaners.Count} cleaners");
            await Task.WhenAll(cleaners.Select(c => c.CleanupAsync(outputtedFiles)).ToList());
        }

        public abstract IEnumerable<IGenerator> GetGenerators();
        public virtual IEnumerable<ICleaner> GetCleaners() => Enumerable.Empty<ICleaner>();

        public override string ToString()
        {
            if (EffectiveOutputPath != null)
            {
                return $"{this.GetType().Name} => {EffectiveOutputPath}";
            }

            return this.GetType().Name;
        }
    }
}
