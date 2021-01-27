using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public static class GeneratorExtensions
    {
        /// <summary>
        /// Fluent configuration of the output path of a generator.
        /// </summary>
        /// <typeparam name="T">The type of the generator</typeparam>
        /// <param name="generator">The generator being configured.</param>
        /// <param name="outputPath">
        ///     The output path to be used by the generator.
        ///     For composite generators, this is probably a path.
        ///     For file generators, this is a path + file name.
        /// </param>
        /// <returns></returns>
        public static T WithOutputPath<T>(this T generator, string outputPath)
            where T : IGenerator
        {
            generator.DefaultOutputPath = outputPath;
            return generator;
        }

        /// <summary>
        /// Fluent configuration of the output path of a generator.
        /// </summary>
        /// <typeparam name="T">The type of the generator</typeparam>
        /// <param name="generator">The generator being configured.</param>
        /// <param name="outputPath">
        ///     The output path to be appended to the current output path of the generator.
        ///     Generators returned from CompositeGenerator.Generator() are configured by default to use the composite generator's path.
        /// </param>
        /// <returns></returns>
        public static T AppendOutputPath<T>(this T generator, string outputPath)
            where T : IGenerator
        {
            generator.DefaultOutputPath = Path.Combine(generator.DefaultOutputPath, outputPath);
            return generator;
        }


        /// <summary>
        /// Fluent configuration of a generator's model.
        /// </summary>
        /// <typeparam name="T">The type of the generator</typeparam>
        /// <typeparam name="TModel">The type of the model</typeparam>
        /// <param name="generator">The generator being configured.</param>
        /// <param name="model">The model to configure the generator with.</param>
        /// <returns></returns>
        public static T WithModel<T, TModel>(this T generator, TModel model)
            // This is doubly qualified because Intellisense seems to want to list this method
            // EVERYWHERE unless we have the IGenerator (non-generic) constraint as well. (VS2017 15.3.5)
            where T : IGenerator<TModel>, IGenerator
        {
            if (model == null)
            {
#pragma warning disable IDE0016 // Use 'throw' expression - TModel is not strictly a reference type.
                // This is supposed to be fixed in https://github.com/dotnet/roslyn/issues/22926
                throw new ArgumentNullException(nameof(model), $"Cannot set null model to {generator}");
#pragma warning restore IDE0016 // Use 'throw' expression
            }

            generator.Model = model;
            return generator;
        }


        /// <summary>
        /// Flatten out all generators.
        /// This includes all FileGenerators and all CompositeGenerators in the hierarchy.
        /// </summary>
        public static IEnumerable<IGenerator> GetGeneratorsFlattened(this ICompositeGenerator generator, ILogger logger = null)
        {
            IEnumerable<IGenerator> Flatten(ICompositeGenerator composite, int depth = 0)
            {
                var prefix = string.Concat(Enumerable.Repeat("  |", depth));

                if (composite.IsDisabled)
                {
                    logger?.LogDebug($"{prefix} {composite.GetType().FullName} => DISABLED");
                    yield break;
                }

                logger?.LogDebug($"{prefix} {composite.GetType().FullName} => {composite.EffectiveOutputPath}");

                prefix = string.Concat(Enumerable.Repeat("  |", depth + 1));

                foreach (var generator in composite.GetGenerators().OrderBy(g => g.GetType().FullName))
                {
                    if (generator.IsDisabled)
                    {
                        logger?.LogDebug($"{prefix} {generator.GetType().FullName} => DISABLED");
                        continue;
                    }
                    else if (generator is ICompositeGenerator childComposite)
                    {
                        foreach (var childGen in Flatten(childComposite, depth + 1)) yield return childGen;
                    }
                    else
                    {
                        logger?.LogDebug($"{prefix} {generator.GetType().FullName} => {generator.EffectiveOutputPath}");
                    }

                    yield return generator;
                }
            }

            return Flatten(generator);
        }
    }

    public static class CleanerExtensions
    {
        /// <summary>
        /// Fluent configuration of the taget path of a cleaner.
        /// </summary>
        /// <typeparam name="T">The type of the cleaner</typeparam>
        /// <param name="cleaner">The cleaner being configured.</param>
        /// <param name="targetPath">
        ///     The target path to be appended to the current target path of the cleaner.
        /// </param>
        /// <returns></returns>
        public static T AppendTargetPath<T>(this T cleaner, string targetPath)
            where T : ICleaner
        {
            cleaner.TargetPath = Path.Combine(cleaner.TargetPath, targetPath);
            return cleaner;
        }
    }
}
