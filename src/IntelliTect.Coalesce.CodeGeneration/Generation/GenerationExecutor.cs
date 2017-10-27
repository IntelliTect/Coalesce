using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class GenerationExecutor
    {
        public GenerationExecutor(CoalesceConfiguration config)
        {
            Config = config;
        }

        public CoalesceConfiguration Config { get; }

        public async Task GenerateAsync<TGenerator>()
            where TGenerator : IRootGenerator
        {
            var services = new ServiceCollection();
            // TODO: configure logging level from command line or config file.
            services.AddLogging(builder => builder
                //.SetMinimumLevel(LogLevel.Debug)
                .AddProvider(new SimpleConsoleLoggerProvider()));

            services.AddSingleton(Config);
            services.AddSingleton<RazorTemplateCompiler>();
            services.AddSingleton<ITemplateResolver, TemplateResolver>();
            services.AddSingleton<RazorTemplateServices>();
            services.AddSingleton<CompositeGeneratorServices>();
            services.AddSingleton<GenerationContext>();
            services.AddTransient<IProjectContextFactory, RoslynProjectContextFactory>();
            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<GenerationExecutor>>();
            var genContext = provider.GetRequiredService<GenerationContext>();

            logger.LogInformation("Loading Projects");

            genContext.WebProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.WebProject);

            // Now that we have the web project, we should be able to precompile our templates while the data project analyzes.
            var precompileTask = Task.Run(() => provider.GetRequiredService<RazorTemplateCompiler>()
                .PrecompileAssemblyTemplates(typeof(TGenerator).Assembly));

            genContext.DataProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.DataProject);
            genContext.DbContextType = FindDbContextType(Config, genContext.DataProject);


            List<ClassViewModel> models = ReflectionRepository
                                .AddContext(genContext.DbContextType)
                                .ToList();

            var validationResult = ValidateContext.Validate(models);
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

            var generator = ActivatorUtilities.CreateInstance<TGenerator>(provider)
                .WithModel(models)
                .WithOutputPath(genContext.WebProject.ProjectPath);

            logger.LogInformation("Starting Generation");

            await generator.GenerateAsync();
            await precompileTask;

            logger.LogInformation("Generation Complete");

        }

        private TypeViewModel FindDbContextType(CoalesceConfiguration config, ProjectContext dataProject)
        {
            var dataContextName = config.Input.DbContextName;
            if (string.IsNullOrWhiteSpace(dataContextName))
            {
                var candidates = dataProject.TypeLocator
                    .FindDerivedTypes(typeof(Microsoft.EntityFrameworkCore.DbContext).FullName)
                    .ToList();
                if (candidates.Count() != 1)
                {
                    throw new InvalidOperationException($"Couldn't find a single DbContext to generate from. " +
                        "Specify the name of your DbContext by adding \"input: {dbContextName: 'MyDbContextClassName'}\" to coalesce.json.");
                }
                return candidates.Single();
            }
            else
            {
                return dataProject.TypeLocator.FindType(dataContextName, throwWhenNotFound: false);
            }
        }
    }
}
