﻿using IntelliTect.Coalesce.CodeGeneration.Analysis;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            ServiceProvider = services.BuildServiceProvider();
        }

        public CoalesceConfiguration Config { get; }
        public ILogger<GenerationExecutor> Logger { get; private set; }
        public ServiceProvider ServiceProvider { get; private set; }

        public Task GenerateAsync<TGenerator>()
            where TGenerator : IRootGenerator
        {
            return GenerateAsync(typeof(TGenerator));
        }

        public TGenerator CreateRootGenerator<TGenerator>()
            where TGenerator : IRootGenerator
        {
            return (TGenerator)CreateRootGenerator(typeof(TGenerator));
        }

        public IRootGenerator CreateRootGenerator(Type rootGenerator)
        {
            if (!typeof(IRootGenerator).IsAssignableFrom(rootGenerator))
            {
                throw new ArgumentException("type is not an IRootGenerator");
            }

            return ActivatorUtilities.CreateInstance(ServiceProvider, rootGenerator) as IRootGenerator;
        }

        public async Task GenerateAsync(Type rootGenerator)
        {
            if (rootGenerator == null)
            {
                throw new ArgumentNullException(nameof(rootGenerator));
            }


            // TODO: extension point for people implementing their own generators?
            // would allow for overriding any of the services used. For example, the template resolver.


            Logger = ServiceProvider.GetRequiredService<ILogger<GenerationExecutor>>();
            var genContext = ServiceProvider.GetRequiredService<GenerationContext>();

            Logger.LogInformation("Loading Projects:");
            await LoadProjects(Logger, genContext);

            // TODO: make GetAllTypes return TypeViewModels, and move this to the TypeLocator base class.
            Logger.LogInformation("Gathering Types");
            var rr = ReflectionRepository.Global;
            IEnumerable<Microsoft.CodeAnalysis.INamedTypeSymbol> types = 
                (genContext.DataProject.TypeLocator as RoslynTypeLocator).GetAllTypes();

            Logger.LogInformation($"Analyzing {types.Count()} Types");
            if (Config.RootTypesWhitelist?.Any() == true)
            {
                rr.SetRootTypeWhitelist(Config.RootTypesWhitelist);
                Logger.LogInformation($"Whitelisted Root Types: {string.Join(", ", Config.RootTypesWhitelist)}");
            }

            rr.DiscoverCoalescedTypes(
                types.Select(t => new SymbolTypeViewModel(rr, t))
            );

#if NET5_0_OR_GREATER
            ResolveEventHandler assemblyResolver = (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);
                var allLoaded = AppDomain.CurrentDomain.GetAssemblies();
                var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
                if (alreadyLoaded != null)
                {
                    Logger.LogWarning($"Resolved {assemblyName.Name} to {alreadyLoaded.GetName().Version} ({assemblyName.Version} was requested by {genContext.DataProject.ProjectFileName})");
                    return alreadyLoaded;
                }
                foreach (var extension in new[] { ".dll", ".exe" })
                {
                    var match = (genContext.DataProject as RoslynProjectContext).MsBuildProjectContext.CompilationAssemblies
                        .FirstOrDefault(a => a.Name == assemblyName.Name + extension);
                    if (match != null)
                    {
                        return Assembly.LoadFrom(match.ResolvedPath);
                    }
                }

                return null;
            };

            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver;
            try
            {
                foreach (var dbContext in rr.DbContexts)
                {
                    // WARNING: This stuff is extremely fragile and implodes on the slightest version mismatch.
                    // It currently works if you launch a .net6 tool against a .net6 target project (e.g. coakesce-vue2.json).
                    // We may need to: Launch a second executable of the generator, which since it will be a fresh process, won't have any EF assemblies loaded,
                    // so we can load the Data project and all its dependencies into that fresh process,
                    // then extract and serialize the EF model (the bits we carea bout) and send it back into the generator process.
                    // Thought: Coalesce has roslyn in it, so create this second executable on the fly with the Data project as a dependency.
                    // Thought: What if the data project already has an entry point? Can we just use that? How did the old EF tooling work when it used to require an entry point?


                    // Load Microsoft.EntityFrameworkCore.Abstractions.
                    // If we don't load our own, local copy,
                    // then DB context construction will fail because we will have loaded our own version of .Relational
                    // and therefore the version of .Relational and .Abstractions won't match.
                    typeof(IndexAttribute).GetTypeInfo();

                    // See efcore\src\ef\ReflectionOperationExecutor.cs
                    var dataAsmPath = (genContext.DataProject as RoslynProjectContext).MsBuildProjectContext.AssemblyFullPath;
                    var assembly = Assembly.LoadFrom(dataAsmPath);

                    var contextOperations = new Microsoft.EntityFrameworkCore.Design.Internal.DbContextOperations(
                        reporter: new LoggerReporter(Logger),
                        assembly: assembly,
                        startupAssembly: assembly,
                        projectDir: Environment.CurrentDirectory,
                        rootNamespace: null,
                        language: null,
                        nullable: false,
                        args: null);

                    using var context = contextOperations.CreateContext(dbContext.ClassViewModel.FullyQualifiedName);
                    dbContext.Model = context.Model;
                    // TODO: Use the Model during code gen.
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolver;
            }
#endif



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
                Environment.Exit(-1);
                return;
            }

            string outputPath = genContext.WebProject.ProjectPath;
            if (Config.Output.TargetDirectory != null)
            {
                outputPath = Path.Combine(outputPath, Config.Output.TargetDirectory);
            }

            var generator = CreateRootGenerator(rootGenerator)
                .WithModel(rr)
                .WithOutputPath(outputPath);

            Logger.LogInformation("Starting Generation");

            await generator.GenerateAsync();

            Logger.LogInformation("Generation Complete");

        }

        private async Task LoadProjects(ILogger<GenerationExecutor> logger, GenerationContext genContext)
        {
            const int maxProjectLoadRetries = 10;
            const int projectLoadRetryDelayMs = 1000;

            // We used to do these in parallel.
            // However, that ends up causing more headache than its worth because
            // it seems that more and more often, the projects step on 
            // one another when they're building and end up failing due
            // to file contention of some form or another.
            genContext.DataProject = await TryLoadProject(Config.DataProject);
            genContext.WebProject = await TryLoadProject(Config.WebProject);

            //await Task.WhenAll(
            //    Task.Run(async () =>
            //    {
            //        genContext.WebProject = await TryLoadProject(Config.WebProject);
            //    }),
            //    Task.Run(async () =>
            //    {
            //        genContext.DataProject = await TryLoadProject(Config.DataProject);
            //    })
            //);

            async Task<ProjectContext> TryLoadProject(ProjectConfiguration config)
            {
                bool restorePackages = false;
                for (int retry = 1; retry <= maxProjectLoadRetries; retry++)
                {
                    try
                    {
                        var projectContext = ServiceProvider
                            .GetRequiredService<IProjectContextFactory>()
                            .CreateContext(config, restorePackages);

                        // Warn if the Coalesce versions referenced in the project don't
                        // match the version of the code generation being ran.
                        if (projectContext is RoslynProjectContext roslynContext)
                        {
                            var coalescePackages = roslynContext
                                .MsBuildProjectContext
                                .PackageDependencies
                                .Where(r => !string.IsNullOrEmpty(r.Version) && r.Name.StartsWith($"{nameof(IntelliTect)}.{nameof(Coalesce)}"))
                                .GroupBy(r => new { r.Name, r.Version }).Select(g => g.First())
                                .ToList();

                            var generationVersion = FileVersionInfo
                                .GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                                .ProductVersion
                                // SourceLink will append the commit hash to the version, using '+' as a delimiter.
                                .Split('+').First();

                            foreach (var coalescePkg in coalescePackages)
                            {
                                if (coalescePkg.Version != generationVersion)
                                {
                                    logger.LogWarning($"Running Coalesce CLI {generationVersion}, but {projectContext.ProjectFileName} references {coalescePkg.Name} {coalescePkg.Version}");
                                }
                            }
                        }

                        return projectContext;
                    }
                    catch (ProjectAnalysisException ex)
                    {
                        bool shouldRetry = false;
                        restorePackages = false;

                        // Matches e.g: "...Person.cs(251,13): warning CS0612..."
                        // We don't output warnings because they're inconsequential to the failure of this part of Coalesce code gen
                        // and don't help the user solve the immediate problem.
                        var warningLineRegex = new Regex(@"\.cs\(\d+,\d+\): warning ");
                        void LogAllOutputAsError()
                        {
                            foreach (var line in ex.OutputLines.Where(l => !warningLineRegex.IsMatch(l)))
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
                            // "Error CSxxxx" - don't trigger retries on any C# errors.
                            // This will include "type or namespace 'x' could not be found".
                            "error cs"
                        ))
                        {
                            // Don't retry
                        }
                        else if (TestLinesForRetry(
                             "not copy the file",
                             "not found",
                             "could not find",
                             "could not be found", // Caution: "type or namespace 'x' could not be found" looks like this.
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
