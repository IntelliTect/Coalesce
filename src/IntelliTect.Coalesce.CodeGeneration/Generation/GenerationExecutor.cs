using IntelliTect.Coalesce.CodeGeneration.Analysis;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class GenerationExecutor
    {
        private readonly LogLevel logLevel;

        public GenerationExecutor(CoalesceConfiguration config, LogLevel logLevel)
        {
            Config = config;
            this.logLevel = logLevel;
        }

        public CoalesceConfiguration Config { get; }

        public Task GenerateAsync<TGenerator>()
            where TGenerator : IRootGenerator
        {
            return GenerateAsync(typeof(TGenerator));
        }
        
        public async Task GenerateAsync(Type rootGenerator)
        {
            if (rootGenerator == null)
            {
                throw new ArgumentNullException(nameof(rootGenerator));
            }

            if (!typeof(IRootGenerator).IsAssignableFrom(rootGenerator))
            {
                throw new ArgumentException("type is not an IRootGenerator");
            }

            var services = new ServiceCollection();
            services.AddLogging(builder => builder
                .SetMinimumLevel(logLevel)
                .AddProvider(new SimpleConsoleLoggerProvider()));

            services.AddSingleton(Config);
            services.AddSingleton(ReflectionRepository.Global);
            services.AddSingleton<ITemplateResolver, TemplateResolver>();
            services.AddSingleton<GeneratorServices>();
            services.AddSingleton<CompositeGeneratorServices>();
            services.AddSingleton<GenerationContext>();
            services.AddTransient<IProjectContextFactory, RoslynProjectContextFactory>();


            // TODO: extension point for people implementing their own generators?
            // would allow for overriding any of the services used. For example, the template resolver.

            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<GenerationExecutor>>();
            var genContext = provider.GetRequiredService<GenerationContext>();

            logger.LogInformation("Loading Projects:");
            await LoadProjects(provider, logger, genContext);

            // TODO: make GetAllTypes return TypeViewModels, and move this to the TypeLocator base class.
            logger.LogInformation("Gathering Types");
            var rr = ReflectionRepository.Global;
            var types = (genContext.DataProject.TypeLocator as RoslynTypeLocator).GetAllTypes();

            logger.LogInformation($"Analyzing {types.Count()} Types");
            rr.DiscoverCoalescedTypes(types.Select(t => new SymbolTypeViewModel(t)));



            var validationResult = ValidateContext.Validate(rr);
            var issues = validationResult.Where(r => !r.WasSuccessful);
            foreach (var issue in issues)
            {
                if (issue.IsWarning) logger.LogWarning(issue.ToString());
                else logger.LogError(issue.ToString());
            }
            if (issues.Any(i => !i.IsWarning))
            {
                logger.LogError("Model validation failed. Exiting.");
                return;
            }

            string outputPath = genContext.WebProject.ProjectPath;
            if (Config.Output.TargetDirectory != null)
            {
                outputPath = Path.Combine(outputPath, Config.Output.TargetDirectory);
            }

            var generator =
                (ActivatorUtilities.CreateInstance(provider, rootGenerator) as IRootGenerator)
                .WithModel(rr)
                .WithOutputPath(outputPath);

            logger.LogInformation("Starting Generation");

            await generator.GenerateAsync();

            logger.LogInformation("Generation Complete");

        }

        private async Task LoadProjects(ServiceProvider provider, ILogger<GenerationExecutor> logger, GenerationContext genContext)
        {
            const int maxProjectLoadRetries = 3;
            const int projectLoadRetryDelayMs = 2000;
            for (int retry = 1; retry <= maxProjectLoadRetries; retry++)
            {
                try
                {
                    await Task.WhenAll(
                        Task.Run(() =>
                        {
                            genContext.WebProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.WebProject);
                        }),
                        Task.Run(() =>
                        {
                            genContext.DataProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.DataProject);
                        })
                    );

                    // If we got here, it worked. Stop looping.
                    break;
                }
                catch (ProjectAnalysisException ex)
                {
                    // If we reached max retries, bail.
                    if (retry == maxProjectLoadRetries) throw;

                    // Dumbly check what the error looks like, attempt retries on errors that are possibly recoverable.
                    // Usually, these are because some other run of msbuild is doing bad things to files that we need.
                    // Related: There are way to many phrasings of "file not there" used by MSBuild.
                    string lastLine = ex.LastOutputLine.ToLowerInvariant();
                    if (lastLine.Contains("not copy the file")
                     || lastLine.Contains("not found")
                     || lastLine.Contains("could not find")
                     || lastLine.Contains("could not be found")
                     || lastLine.Contains("msb3491")
                     || lastLine.Contains("being used by another process")
                    )
                    {
                        // Error message looks like something that might go away if we try again. Lets try again in a second.

                        logger.LogWarning(ex, $"Error analyzing projects. Attempting retry {retry} of {maxProjectLoadRetries} in {projectLoadRetryDelayMs}ms");
                        await Task.Delay(projectLoadRetryDelayMs);
                    }
                    else
                    {
                        // Doesn't look like anything to me. Just bail.
                        throw;
                    }

                }
            }
        }
    }
}
