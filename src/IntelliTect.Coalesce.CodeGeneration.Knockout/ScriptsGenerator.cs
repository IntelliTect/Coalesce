using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.Models;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using System.Globalization;
using System.Reflection;
using System.Threading;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using Microsoft.Extensions.DependencyInjection;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using Microsoft.Extensions.Logging;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout
{
    public class ScriptsGenerator
    {
        public const string ScriptsFolderName = "Scripts";

        public ProjectContext WebProject { get; }
        public ProjectContext DataProject { get; }

        public ScriptsGenerator(ProjectContext webProject, ProjectContext dataProject)
        {
            WebProject = webProject;
            DataProject = dataProject;
        }

        public async Task Generate(CoalesceConfiguration config)
        {
            Console.WriteLine($"Starting Generator");
            string targetNamespace = WebProject.RootNamespace;
            Console.WriteLine($"Target Namespace: {targetNamespace}");

            var dataContextName = config.Input.DbContextName;
            TypeViewModel dataContextType;
            if (string.IsNullOrWhiteSpace(dataContextName))
            {
                var candidates = DataProject.TypeLocator
                    .FindDerivedTypes(typeof(Microsoft.EntityFrameworkCore.DbContext).FullName)
                    .ToList();
                if (candidates.Count() != 1)
                {
                    throw new InvalidOperationException($"Couldn't find a single DbContext to generate from. " +
                        $"Specify the name of your DbContext with the '-dc MyDbContext' command line param.");
                }
                dataContextType = candidates.Single();
            }
            else
            {
                dataContextType = DataProject.TypeLocator.FindType(dataContextName, throwWhenNotFound: false);
            }

            Console.WriteLine($"Building scripts for: {dataContextType.FullName}");

            List<ClassViewModel> models = ReflectionRepository
                                .AddContext(dataContextType)
                                .ToList();

            ValidationHelper validationResult = ValidateContext.Validate(models);

            bool foundIssues = false;
            foreach (var validation in validationResult.Where(f => !f.WasSuccessful))
            {
                foundIssues = true;
                Console.WriteLine("--- " + validation.ToString());
            }
            if (!foundIssues)
            {
                Console.WriteLine("Model validated successfully");
            }

            if (foundIssues)
            {
                //throw new Exception("Model did not validate. " + validationResult.First(f => !f.WasSuccessful).ToString());
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Press enter to quit");
                    Console.Read();
                }
                Environment.Exit(1);
                return;
            }

            var generationContext = new GenerationContext(config)
            {
                DataProject = DataProject,
                WebProject = WebProject,
                DbContextType = dataContextType,
            };

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddProvider(new SimpleConsoleLoggerProvider()));
            services.AddSingleton(generationContext);
            services.AddSingleton(config);
            services.AddSingleton<RazorTemplateCompiler>();
            services.AddSingleton<ITemplateResolver, TemplateResolver>();
            services.AddSingleton<RazorTemplateServices>();
            services.AddSingleton<CompositeGeneratorServices>();
            var provider = services.BuildServiceProvider();

            var generator = ActivatorUtilities.CreateInstance<KnockoutSuite>(provider)
                .WithModel(models)
                .WithOutputPath(WebProject.ProjectPath);

            await generator.GenerateAsync();
        }
    }
}
