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
        private readonly LogLevel logLevel;

        public GenerationExecutor(CoalesceConfiguration config, LogLevel logLevel)
        {
            Config = config;
            this.logLevel = logLevel;
        }

        public CoalesceConfiguration Config { get; }

        public async Task GenerateAsync<TGenerator>()
            where TGenerator : IRootGenerator
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder
                .SetMinimumLevel(logLevel)
                .AddProvider(new SimpleConsoleLoggerProvider()));

            services.AddSingleton(Config);
            services.AddSingleton(ReflectionRepository.Global);
            services.AddSingleton<RazorTemplateCompiler>();
            services.AddSingleton<ITemplateResolver, TemplateResolver>();
            services.AddSingleton<RazorTemplateServices>();
            services.AddSingleton<CompositeGeneratorServices>();
            services.AddSingleton<GenerationContext>();
            services.AddTransient<IProjectContextFactory, RoslynProjectContextFactory>();


            // TODO: extension point for people implementing their own generators?
            // would allow for overriding any of the services used. For example, the template resolver.

            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<GenerationExecutor>>();
            var genContext = provider.GetRequiredService<GenerationContext>();

            logger.LogInformation("Loading Projects");

            genContext.WebProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.WebProject);

            // Now that we have the web project, we should be able to precompile our templates while the data project analyzes.
            var precompileTask = Task.Run(() => provider.GetRequiredService<RazorTemplateCompiler>()
                .PrecompileAssemblyTemplates(typeof(TGenerator).Assembly));

            genContext.DataProject = provider.GetRequiredService<IProjectContextFactory>().CreateContext(Config.DataProject);

            // TODO: make GetAllTypes return TypeViewModels, and move this to the TypeLocator base class.
            var rr = ReflectionRepository.Global;
            var types = (genContext.DataProject.TypeLocator as RoslynTypeLocator).GetAllTypes();
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

            var generator = ActivatorUtilities.CreateInstance<TGenerator>(provider)
                .WithModel(rr)
                .WithOutputPath(genContext.WebProject.ProjectPath);

            logger.LogInformation("Starting Generation");

            await generator.GenerateAsync();
            await precompileTask;

            logger.LogInformation("Generation Complete");

        }
    }
}
