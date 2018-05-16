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
        public ILogger<GenerationExecutor> Logger { get; private set; }

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

            Logger = provider.GetRequiredService<ILogger<GenerationExecutor>>();
            var genContext = provider.GetRequiredService<GenerationContext>();

            Logger.LogInformation("Loading Projects:");
            await LoadProjects(provider, Logger, genContext);

            // TODO: make GetAllTypes return TypeViewModels, and move this to the TypeLocator base class.
            Logger.LogInformation("Gathering Types");
            var rr = ReflectionRepository.Global;
            var types = (genContext.DataProject.TypeLocator as RoslynTypeLocator).GetAllTypes();

            Logger.LogInformation($"Analyzing {types.Count()} Types");
            rr.DiscoverCoalescedTypes(types.Select(t => new SymbolTypeViewModel(t)));



            var validationResult = ValidateContext.Validate(rr);
            var issues = validationResult.Where(r => !r.WasSuccessful);
            foreach (var issue in issues)
            {
                if (issue.IsWarning) Logger.LogWarning(issue.ToString());
                else Logger.LogError(issue.ToString());
            }
            if (issues.Any(i => !i.IsWarning))
            {
                Logger.LogError("Model validation failed. Exiting.");
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

            Logger.LogInformation("Starting Generation");

            await generator.GenerateAsync();

            Logger.LogInformation("Generation Complete");

        }

        private async Task LoadProjects(ServiceProvider provider, ILogger<GenerationExecutor> logger, GenerationContext genContext)
        {
            const int maxProjectLoadRetries = 10;
            const int projectLoadRetryDelayMs = 1000;

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    genContext.WebProject = await TryLoadProject(Config.WebProject);
                }),
                Task.Run(async () =>
                {
                    genContext.DataProject = await TryLoadProject(Config.DataProject);
                })
            );

            async Task<ProjectContext> TryLoadProject(ProjectConfiguration config)
            {
                bool restorePackages = false;
                for (int retry = 1; retry <= maxProjectLoadRetries; retry++)
                {
                    try
                    {
                        return provider.GetRequiredService<IProjectContextFactory>().CreateContext(config, restorePackages);
                    }
                    catch (ProjectAnalysisException ex)
                    {
                        bool shouldRetry = false;
                        restorePackages = false;

                        void LogAllOutputAsError()
                        {
                            foreach (var line in ex.OutputLines)
                            {
                                logger.LogError(line);
                            }
                        }

                        // If we reached max retries, bail.
                        if (retry == maxProjectLoadRetries)
                        {
                            LogAllOutputAsError();
                            throw;
                        }

                        bool TestLinesForRetry(params string[] lineSubstrings)
                        {
                            var triggers = ex.OutputLines
                                .Where(l => lineSubstrings.Any(sub => l.ToLowerInvariant().Contains(sub.ToLowerInvariant().Trim())))
                                .ToList();

                            if (triggers.Any())
                            {
                                foreach (var line in triggers)
                                {
                                    // Just log as debug - if all our retries ultimately fail,
                                    // all the output will get logged as errors anyway.
                                    logger.LogDebug(line);
                                }
                                return true;
                            }

                            return false;
                        }

                        // Dumbly check what the error looks like, attempt retries on errors that are possibly recoverable.
                        // Usually, these are because some other run of msbuild is doing bad things to files that we need.
                        // Related: There are way to many phrasings of "file not there" used by MSBuild.

                        // First, check for errors that can probably be fixed with a package restore.
                        if (TestLinesForRetry(
                            "run a nuget package restore",
                            "The type or namespace name 'System' could not be found in the global namespace"
                        )) {
                            shouldRetry = true;
                            restorePackages = true;
                        }
                        else if (TestLinesForRetry(
                             "not copy the file",
                             "not found",
                             "could not find",
                             "could not be found",
                             "msb3491",
                             "being used by another process"
                        )) {
                            shouldRetry = true;
                        }

                        if (shouldRetry)
                        {
                            // Error message looks like something that might go away if we try again. Lets try again in a second.
                            await Task.Delay(projectLoadRetryDelayMs);
                            logger.LogWarning(ex, $"Error analyzing {config.ProjectFile}, probably due to file contention. Attempting retry {retry} of {maxProjectLoadRetries}. (use '--verbosity debug' to see errors)");
                        }
                        else
                        {
                            // Doesn't look like anything to me. Log all output and bail.
                            LogAllOutputAsError();
                            throw;
                        }
                    }
                }

                // Not possible to reach, but need to satisfy the compiler.
                return null;
            }
        }
    }
}
