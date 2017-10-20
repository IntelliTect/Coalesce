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
            generator.OutputPath = outputPath;
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
            generator.OutputPath = Path.Combine(generator.OutputPath, outputPath);
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
            where T : IGenerator<TModel>
        {
            generator.Model = model;
            return generator;
        }
    }
}
