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
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.ProjectModel;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public class ScriptsGenerator
    {
        public const string ScriptsFolderName = "Scripts";

        protected RazorTemplateProvider TemplateProvider { get; }
        public ProjectContext WebProject { get; }
        public ProjectContext DataProject { get; }

        public ScriptsGenerator(ProjectContext webProject, ProjectContext dataProject)
        {
            TemplateProvider = new RazorTemplateProvider(webProject);

            WebProject = webProject;
            DataProject = dataProject;
        }

        internal Task Generate(CommandLineGeneratorModel model)
        {
            Dictionary<string, Dictionary<int, string>> enumValues = new Dictionary<string, Dictionary<int, string>>();

            using (StreamWriter streamWriter = new StreamWriter("output.txt", false))
            {
                Console.WriteLine($"Starting Generator");
                string targetNamespace;
                if (!string.IsNullOrEmpty(model.TargetNamespace))
                {
                    targetNamespace = model.TargetNamespace;
                }
                else
                {
                    var startupSymbol = ValidationUtil.ValidateType("Startup", "", WebProject.TypeLocator, throwWhenNotFound: false);
                    targetNamespace = startupSymbol.ContainingNamespace.ToDisplayString();
                }
                Console.WriteLine($"Namespace: {targetNamespace}");

                var dataContextSymbol = ValidationUtil.ValidateType(model.DataContextClass, "dataContext", DataProject.TypeLocator, throwWhenNotFound: false);

                // var assembly = DataProject.TypeLocator.GetAssembly();

                if (model.ValidateOnly)
                {
                    Console.WriteLine($"Validating model for: {dataContextSymbol.ToDisplayString()}");
                }
                else
                {
                    Console.WriteLine($"Building scripts for: {dataContextSymbol.ToDisplayString()}");
                }

                List<ClassViewModel> models = null;
                try
                {
                    models = ReflectionRepository
                                    .AddContext(dataContextSymbol)
                                    //.Where(m => m.PrimaryKey != null)
                                    .ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to generate models\n   {ex.Message}\n{ex.StackTrace}");
                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press enter to quit");
                        Console.Read();
                    }
                    Environment.Exit(1);
                }


                ValidationHelper validationResult = null;

                try
                {
                    validationResult = ValidateContext.Validate(models);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An Exception occurred during validation\n   {ex.Message}\n{ex.StackTrace}");
                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press enter to quit");
                        Console.Read();
                    }
                    Environment.Exit(1);
                }

                bool foundIssues = false;
                foreach (var validation in validationResult.Where(f => !f.WasSuccessful))
                {
                    foundIssues = true;
                    streamWriter.WriteLine(validation.ToString());
                    Console.WriteLine("--- " + validation.ToString());
                }
                if (!foundIssues)
                {
                    Console.WriteLine("Model validated successfully");
                }

                streamWriter.WriteLine($" {"Name",-15}  {"Type",-15}  {"Pure Type",-15} {"Col",-5} {"Array",-5} {"Key",-5} {"DisplayName",-15} {"Null?",-5} {"Many",-5} {"Internal",-5} {"FileDL",-5} {"IsNum",-5} {"IsDT",-5} {"IsDTO",-5} {"IsBool",-5} {"IsStr",-5} {"IsEnum",-8} {"JsKoType",-25} {"TsKoType",-50} {"TsType",-15} {"DateOnly",-10} {"Hidden",-8} {"Required",-8} {"KeyName",-15} {"MinLength",-8} {"MaxLength",-10} {"Range",-10}");

                foreach (var obj in models.Where(p => p.HasDbSet || p.IsDto))
                {
                    //Console.WriteLine($"{obj.Name}  dB:{obj.HasDbSet}");
                    streamWriter.WriteLine($"{obj.Name}  dB:{obj.HasDbSet}    Edit:{obj.SecurityInfo.IsEditAllowed()}   Create:{obj.SecurityInfo.IsCreateAllowed()}    Delete:{obj.SecurityInfo.IsDeleteAllowed()}");

                    foreach (var prop in obj.Properties.Where(f => !f.IsInternalUse))
                    {
                        streamWriter.WriteLine($@" {prop.Name,-15}  {prop.TypeName,-15}  {prop.PureType.Name,-15} {prop.Type.IsCollection,-5} {prop.Type.IsArray,-5} {prop.IsPrimaryKey,-5} {prop.DisplayName,-15} {prop.Type.IsNullable,-5} {prop.IsManytoManyCollection,-5} {prop.IsInternalUse,-5}    {prop.IsFileDownload,-5}  {prop.Type.IsNumber,-5} {prop.Type.IsDateTime,-5} {prop.Type.IsDateTimeOffset,-5} {prop.Type.IsBool,-5}  {prop.Type.IsString,-5} {prop.Type.IsEnum,-8} {prop.Type.JsKnockoutType,-25} {prop.Type.TsKnockoutType,-50} {prop.Type.TsType,-15} {prop.IsDateOnly,-10} {prop.IsHidden(HiddenAttribute.Areas.Edit),-8}  {prop.IsRequired,-8} {prop.ObjectIdPropertyName,-15} {prop.MinLength,-8} {prop.MaxLength,-10} {prop.Range?.Item1 + " " + prop.Range?.Item2,-10}");
                        if (prop.Type.IsEnum && !enumValues.ContainsKey(prop.Name))
                        {
                            enumValues.Add(prop.Name, prop.Type.EnumValues);
                        }
                    }

                    foreach (var method in obj.Methods.Where(f => !f.IsInternalUse))
                    {
                        streamWriter.WriteLine($@" {method.Name,-15}  {method.ReturnType.Name,-15}  {method.ReturnType.PureType.Name,-15} {method.ReturnType.IsCollection,-5} {method.ReturnType.IsArray,-5} {null,-5} {null,-7} {method.DisplayName,-15} {method.ReturnType.IsNullable,-5} {null,-5} {null,-5}    {null,-5}  {method.ReturnType.IsNumber,-5} {method.ReturnType.IsDateTime,-5} {method.ReturnType.IsDateTimeOffset,-5} {method.ReturnType.IsBool,-5}  {method.ReturnType.IsString,-5} {method.ReturnType.IsEnum,-8} {method.ReturnType.JsKnockoutType,-25} {method.ReturnType.TsKnockoutType,-50} {method.ReturnType.TsType,-15} {null,-10} {method.IsHidden(HiddenAttribute.Areas.Edit),-8}  {null,-8} {null,-15} {null,-8} {null,-10} {"",-10}");
                    }

                }

                streamWriter.WriteLine("-------- Enumerations --------");
                foreach (string propertyKey in enumValues.Keys)
                {
                    streamWriter.WriteLine(propertyKey);
                    foreach (var enumValue in enumValues[propertyKey])
                    {
                        streamWriter.WriteLine($"\t{enumValue.Key} : {enumValue.Value}");
                    }
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
                }

                if (model.ValidateOnly)
                {
                    return Task.FromResult(0);
                }
                else
                {
                    var contextInfo = new ContextInfo(dataContextSymbol.Name, targetNamespace);
                    return GenerateScripts(model, models, contextInfo, targetNamespace);
                }
            }
        }

        private string ActiveTemplatesPath => Path.Combine(
                WebProject.ProjectFullPath,
                "Coalesce",
                "Templates");

        private bool _hasExplainedCopying = false;
        private void CopyToOriginalsAndDestinationIfNeeded(string fileName, string sourcePath, string originalsPath, string destinationPath, bool alertIfNoCopy = true)
        {
            string originalsFile = Path.Combine(originalsPath, fileName);
            string destinationFile = Path.Combine(destinationPath, fileName.Replace(".template", ""));

            if ((File.Exists(originalsFile) || !File.Exists(destinationFile)) && !FileUtilities.HasDifferences(originalsFile, destinationFile))
            {
                // The original file and the active file are the same,
                // and either the original does exist, or the destination doesn't. (this prevents overwriting of an existing destination file when the original doesn't exist).
                // Overwrite the active file with the new template.
                CopyToDestination(fileName, sourcePath, destinationPath, destinationIsTemplate: false);
            }
            else if (alertIfNoCopy)
            {
                Console.WriteLine($"Skipping copy to {destinationFile.Replace(WebProject.ProjectFullPath, "")} because it has been modified from the original.");
                if (!_hasExplainedCopying)
                {
                    _hasExplainedCopying = true;
                    Console.WriteLine("    If you would like for your templates to be updated by the CLI, restore the copies of ");
                    Console.WriteLine("    your templates in Coalesce/Templates with those from Coalesce/Originals/Templates.");
                    Console.WriteLine("    If you experience issues with your templates, compare your template with the original to see what might need changing.");
                }
            }

            string originalFile = Path.Combine(originalsPath, fileName);


            FileAttributes attr;
            if (File.Exists(originalFile))
            {
                // unset read-only
                attr = File.GetAttributes(originalFile);
                attr = attr & ~FileAttributes.ReadOnly;
                File.SetAttributes(originalFile, attr);
            }

            CopyToDestination(fileName, sourcePath, originalsPath);

            // set read-only
            if (File.Exists(originalFile))
            {
                attr = File.GetAttributes(originalFile);
                attr = attr | FileAttributes.ReadOnly;
                File.SetAttributes(originalFile, attr);
            }
        }

        private void CopyToDestination(string fileName, string sourcePath, string destinationPath, bool destinationIsTemplate = true)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string sourceFile = assembly.GetName().Name + "." + Path.Combine(sourcePath, fileName).Replace('/', '.').Replace('\\', '.');

            Directory.CreateDirectory(destinationPath);

            string destinationFile = destinationIsTemplate
                ? Path.Combine(destinationPath, fileName)
                : Path.Combine(destinationPath, fileName.Replace(".template", ""));

            var inputStream = assembly.GetManifestResourceStream(sourceFile);
            if (inputStream == null)
            {
                throw new FileNotFoundException("Embedded resource not found: " + sourceFile);
            }

            if (!File.Exists(destinationFile) || FileUtilities.HasDifferences(inputStream, destinationFile))
            {
                const int tries = 3;
                for (int i = 1; i <= tries; i++)
                {
                    FileStream fileStream = null;
                    try
                    {
                        fileStream = File.Create(destinationFile);
                        inputStream.Seek(0, SeekOrigin.Begin);
                        inputStream.CopyTo(fileStream);
                        if (i > 1) Console.WriteLine($"Attempt {i} succeeded.");
                        break;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Attempt {i} of {tries} failed: {ex.Message}");
                        if (i == tries)
                            throw;

                        // Errors here are almost always because a file is in use. Just wait a second and it probably won't be in use anymore.
                        Thread.Sleep(1000);
                    }
                    finally
                    {
                        fileStream?.Dispose();
                    }
                }
            }
        }

        private async Task CopyStaticFiles(CommandLineGeneratorModel commandLineGeneratorModel)
        {
            Console.WriteLine("Copying Static Files");
            string areaLocation = "";

            if (!string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                areaLocation = Path.Combine("Areas", commandLineGeneratorModel.AreaLocation);
            }

            // Our gulp tasks don't like it if the areas folder doesn't exist, so just create it by default.
            Directory.CreateDirectory(Path.Combine(WebProject.ProjectFullPath, "Areas"));

            // Directory location for all "original" files from intellitect
            var baseCoalescePath = Path.Combine(
                WebProject.ProjectFullPath,
                areaLocation,
                "Coalesce");

            var coalescePathExisted = Directory.Exists(baseCoalescePath);

            var originalsPath = Path.Combine(baseCoalescePath, "Originals");
            var originalTemplatesPath = Path.Combine(originalsPath, "Templates");
            var activeTemplatesPath = ActiveTemplatesPath;


            Directory.CreateDirectory(baseCoalescePath);

            // We need to preserve the old intelliTect folder so that we don't overwrite any custom files,
            // since the contents of this folder are what is used to determine if changes have been made.
            // If the Coalesce folder isn't found, we will assume this is effectively a new installation of Coalesce.
            var oldOriginalsPath = Path.Combine(
                 WebProject.ProjectFullPath,
                 areaLocation,
                 "intelliTect");
            if (Directory.Exists(oldOriginalsPath))
                Directory.Move(oldOriginalsPath, originalsPath);// TODO: remove this at some point after all projects are upgraded.

            Directory.CreateDirectory(originalsPath);
            Directory.CreateDirectory(originalTemplatesPath);
            Directory.CreateDirectory(activeTemplatesPath);

            // Copy over Api Folder and Files
            var apiViewOutputPath = Path.Combine(
                WebProject.ProjectFullPath,
                areaLocation,
                "Views", "Api");

            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "Docs.cshtml",
                    sourcePath: "Templates/Views/Api",
                    originalsPath: originalsPath,
                    destinationPath: apiViewOutputPath);

            // Copy over Shared Folder (_EditorHtml, _Master - always, _Layout only if it doesn't exist)
            var sharedViewOutputPath = Path.Combine(
                WebProject.ProjectFullPath,
                areaLocation,
                "Views", "Shared");

            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "_EditorHtml.cshtml",
                    sourcePath: "Templates/Views/Shared",
                    originalsPath: originalsPath,
                    destinationPath: sharedViewOutputPath);

            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "_Master.cshtml",
                    sourcePath: "Templates/Views/Shared",
                    originalsPath: originalsPath,
                    destinationPath: sharedViewOutputPath);

            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "_ViewStart.cshtml",
                    sourcePath: "Templates/Views",
                    originalsPath: originalsPath,
                    destinationPath: Path.Combine(WebProject.ProjectFullPath, areaLocation, "Views"));

            if (string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                CopyToOriginalsAndDestinationIfNeeded(
                        alertIfNoCopy: false,
                        fileName: "_Layout.cshtml",
                        sourcePath: "Templates/Views/Shared",
                        originalsPath: originalsPath,
                        destinationPath: sharedViewOutputPath);



                // only copy the intellitect scripts when generating the root site, this isn't needed for areas since it will already exist at the root
                // Copy files for the scripts folder
                var scriptsOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    "Scripts", "Coalesce");

                var oldScriptsOutputPath = Path.Combine(
                  WebProject.ProjectFullPath,
                    "Scripts", "IntelliTect");
                if (Directory.Exists(oldScriptsOutputPath)) Directory.Delete(oldScriptsOutputPath, true); // TODO: remove this at some point after all projects are upgraded.

                string[] intellitectScripts =
                {
                    "coalesce.ko.base.ts",
                    "coalesce.ko.bindings.ts",
                    "coalesce.ko.utilities.ts",
                    "coalesce.utilities.ts",
                };
                // These were renamed from intellitect.* to coalesce.*. Delete the old ones.
                if (System.IO.Directory.Exists(scriptsOutputPath))
                {
                    foreach (var oldName in Directory.EnumerateFiles(scriptsOutputPath, "intellitect.*"))
                    {
                        File.Delete(oldName);
                    }
                }
                foreach (var fileName in intellitectScripts)
                {
                    CopyToDestination(
                            fileName: fileName,
                            sourcePath: "Templates/Scripts/Coalesce",
                            destinationPath: scriptsOutputPath);
                }

                if (!coalescePathExisted)
                {
                    // Only copy the typings on a brand new project. Never attempt to update them, since the end user
                    // may want to update their libraries, get rid of some (well, you probably can't), or update versions.
                    CopyToDestination(
                        fileName: "tsd.d.ts",
                        sourcePath: "Templates/typings",
                        destinationPath: Path.Combine(WebProject.ProjectFullPath, "typings"));

                    string[] typings =
                    {
                        "bootstrap",
                        "bootstrap.v3.datetimepicker",
                        "jquery",
                        "knockout",
                        "knockout.validation",
                        "moment",
                    };
                    foreach (var fileName in typings)
                    {
                        CopyToDestination(
                                fileName: fileName + ".d.ts",
                                sourcePath: "Templates/typings/" + fileName,
                                destinationPath: Path.Combine(
                                    WebProject.ProjectFullPath,
                                    "typings",
                                    fileName));
                    }

                    // This one is a special snowflake
                    CopyToDestination(
                       fileName: "moment-node.d.ts",
                       sourcePath: "Templates/typings/moment",
                       destinationPath: Path.Combine(WebProject.ProjectFullPath, "typings/moment"));
                }


                string[] generationTemplates =
                {
                    "ApiController.cshtml",
                    "CardView.cshtml",
                    "ClassDto.cshtml",
                    "CreateEditView.cshtml",
                    "KoExternalType.cshtml",
                    "KoListViewModel.cshtml",
                    "KoViewModel.cshtml",
                    "KoViewModelPartial.cshtml",
                    "LocalBaseApiController.cshtml",
                    "TableView.cshtml",
                    "ViewController.cshtml",
                };
                foreach (var fileName in generationTemplates)
                {
                    CopyToOriginalsAndDestinationIfNeeded(
                            fileName: fileName,
                            sourcePath: "Templates",
                            originalsPath: originalTemplatesPath,
                            destinationPath: activeTemplatesPath);
                }
            }

            var stylesOutputPath = Path.Combine(
                WebProject.ProjectFullPath,
                areaLocation,
                "Styles");
            CopyToOriginalsAndDestinationIfNeeded(
                    alertIfNoCopy: false,
                    fileName: "site.scss",
                    sourcePath: "Templates/Styles",
                    originalsPath: originalsPath,
                    destinationPath: stylesOutputPath);

            // if generating an area, assume the root site already has all "plumbing" needed
            if (string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                // Copy files from ProjectConfig if they don't already exist
                string[] configFiles =
                {
                    "bower.json",
                    ".bowerrc",
                    "gulpfile.js",
                    "package.json",
                    "tsconfig.template.json",
                    "tsd.json",
                };
                foreach (var fileName in configFiles)
                {
                    CopyToOriginalsAndDestinationIfNeeded(
                            alertIfNoCopy: false,
                            fileName: fileName,
                            sourcePath: "Templates/ProjectConfig",
                            originalsPath: originalsPath,
                            destinationPath: WebProject.ProjectFullPath);
                }

                CopyToDestination(
                        fileName: "CoalesceScaffoldingReadme.txt",
                        sourcePath: "Templates",
                        destinationPath: WebProject.ProjectFullPath);
            }


            CopyToDestination(
                    fileName: "CoalesceScaffoldingReadme.txt",
                    sourcePath: "Templates",
                    destinationPath: WebProject.ProjectFullPath);

            string destPath = Path.Combine(WebProject.ProjectFullPath, areaLocation, "Views", "_ViewImports.cshtml");
            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "_ViewImports.cshtml",
                    sourcePath: "Templates/Views",
                    originalsPath: originalTemplatesPath,
                    destinationPath: activeTemplatesPath);
            await Generate("_ViewImports.cshtml", destPath, null);



            destPath = Path.Combine(sharedViewOutputPath, "_AdminLayout.cshtml");
            CopyToOriginalsAndDestinationIfNeeded(
                    fileName: "_AdminLayout.cshtml",
                    sourcePath: "Templates/Views/Shared",
                    originalsPath: originalTemplatesPath,
                    destinationPath: activeTemplatesPath);
            await Generate("_AdminLayout.cshtml", destPath, commandLineGeneratorModel.AreaLocation);
        }

        private Task Generate(string templateName, string outputPath, object model)
        {
            var template = TemplateProvider.GetCompiledTemplate(Path.Combine(ActiveTemplatesPath, templateName));
            return TemplateProvider.RunTemplateAsync(template, model, outputPath);
        }

        class GenerationOutputContext : IDisposable
        {
            private string OutputDir { get; }

            private ScriptsGenerator Generator { get; }

            private List<string> GeneratedFiles { get; } = new List<string>();

            public GenerationOutputContext(ScriptsGenerator generator, string outputDir)
            {
                Generator = generator;
                OutputDir = outputDir;
            }

            public Task Generate(string templateName, string outputName, object model)
            {
                string outputPath = Path.Combine(OutputDir, outputName);
                GeneratedFiles.Add(outputPath);

                return Generator.Generate(templateName, outputPath, model);
            }

            public void Cleanup()
            {
                //foreach ( var generatedFile in GeneratedFiles )
                //{
                //    Console.WriteLine($"Generated {generatedFile}");
                //}
                foreach (var file in Directory.EnumerateFiles(OutputDir, "*", SearchOption.AllDirectories))
                {
                    if (!GeneratedFiles.Contains(file))
                    {
                        Console.WriteLine($"   Deleting {file} because it seems to be unused.");
                        File.Delete(file);
                    }
                }
            }

            public void Dispose()
            {
                Cleanup();
            }
        }

        private async Task GenerateScripts(
            CommandLineGeneratorModel controllerGeneratorModel,
            List<ClassViewModel> models,
            ContextInfo dataContext,
            string targetNamespace)
        {
            string areaLocation = "";

            if (!string.IsNullOrWhiteSpace(controllerGeneratorModel.AreaLocation))
            {
                areaLocation = Path.Combine("Areas", controllerGeneratorModel.AreaLocation);
            }

            // TODO: do we need this anymore?
            //var layoutDependencyInstaller = ActivatorUtilities.CreateInstance<MvcLayoutDependencyInstaller>(ServiceProvider);
            //await layoutDependencyInstaller.Execute();

            ViewModelForTemplates apiModels = new ViewModelForTemplates
            {
                Models = models,
                ContextInfo = dataContext,
                Namespace = targetNamespace,
                AreaName = controllerGeneratorModel.AreaLocation,
                ModulePrefix = controllerGeneratorModel.TypescriptModulePrefix
            };

            // Copy over the static files
            await CopyStaticFiles(controllerGeneratorModel);

            var apiViewOutputPath = Path.Combine(
                WebProject.ProjectFullPath,
                areaLocation,
                "Views", "Api");

            if (!Directory.Exists(apiViewOutputPath))
            {
                Directory.CreateDirectory(apiViewOutputPath);
            }

            Console.WriteLine("Generating Code");
            Console.WriteLine("-- Generating DTOs");
            Console.Write("   ");
            string modelOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    areaLocation,
                    "Models", "Generated");
            using (var output = new GenerationOutputContext(this, modelOutputPath))
            {
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => !f.Model.IsDto))
                {
                    Console.Write($"{model.Model.Name}  ");

                    await output.Generate("ClassDto.cshtml", Path.Combine(modelOutputPath, model.Model.Name + "DtoGen.cs"), model);
                }
                Console.WriteLine();
            }


            string scriptOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    areaLocation,
                    ScriptsFolderName, "Generated");
            using (var output = new GenerationOutputContext(this, scriptOutputPath))
            {
                Console.WriteLine("-- Generating TypeScript Models");
                Console.Write("   ");
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext || f.Model.IsDto))
                {
                    Console.Write($"{model.Model.Name}  ");

                    var fileName = "Ko";
                    if (!string.IsNullOrWhiteSpace(model.ModulePrefix)) fileName += "." + model.ModulePrefix;
                    fileName += "." + model.Model.Name;
                    if (model.Model.HasTypeScriptPartial) fileName += ".Partial";
                    fileName += ".ts";
                    await output.Generate("KoViewModel.cshtml", fileName, model);

                    //Console.WriteLine("   Added Script: " + viewOutputPath);

                    fileName = (string.IsNullOrWhiteSpace(model.ModulePrefix)) ? $"Ko.{model.Model.ListViewModelClassName}.ts" : $"Ko.{model.ModulePrefix}.{model.Model.ListViewModelClassName}.ts";
                    await output.Generate("KoListViewModel.cshtml", fileName, model);

                    //Console.WriteLine("   Added Script: " + viewOutputPath);
                }
                Console.WriteLine();



                if (apiModels.ViewModelsForTemplates.Any(f => !f.Model.OnContext))
                {
                    Console.WriteLine("-- Generating TypeScript External Types");
                    Console.Write("   ");
                    foreach (var externalType in apiModels.ViewModelsForTemplates.Where(f => !f.Model.OnContext))
                    {
                        var fileName = "Ko";
                        if (!string.IsNullOrWhiteSpace(externalType.ModulePrefix)) fileName += "." + externalType.ModulePrefix;
                        fileName += "." + externalType.Model.Name;
                        if (externalType.Model.HasTypeScriptPartial) fileName += ".Partial";
                        fileName += ".ts";
                        await output.Generate("KoExternalType.cshtml", fileName, externalType);

                        Console.Write(externalType.Model.Name + "  ");
                    }
                    Console.WriteLine();
                }

            }


            if (apiModels.ViewModelsForTemplates.Any(f => f.Model.HasTypeScriptPartial))
            {
                string partialOutputPath = Path.Combine(
                        WebProject.ProjectFullPath,
                        areaLocation,
                        ScriptsFolderName, "Partials");
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.HasTypeScriptPartial))
                {
                    var fileName = (string.IsNullOrWhiteSpace(model.ModulePrefix)) ? $"Ko.{model.Model.Name}.partial.ts" : $"Ko.{model.ModulePrefix}.{model.Model.Name}.partial.ts";
                    var fullName = Path.Combine(partialOutputPath, fileName);

                    if (!File.Exists(fullName))
                    {
                        await Generate("KoViewModelPartial.cshtml", fullName, model);

                        Console.Write($"    Generated Partial stub for {model.Model.Name}  ");
                    }
                }
                Console.WriteLine();
            }





            Console.WriteLine("-- Generating API Controllers");
            string apiOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    areaLocation,
                    "Api", "Generated");
            using (var output = new GenerationOutputContext(this, apiOutputPath))
            {
                // Generate base api controller if it doesn't already exist
                {
                    var model = apiModels.ViewModelsForTemplates.First(f => f.Model.OnContext);

                    await output.Generate("LocalBaseApiController.cshtml", "LocalBaseApiController.cs", model);
                }

                // Generate model api controllers
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
                {
                    await output.Generate("ApiController.cshtml", model.Model.Name + "ControllerGen.cs", model);
                }
            }


            Console.WriteLine("-- Generating View Controllers");
            string controllerOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    areaLocation,
                    "Controllers", "Generated");
            using (var output = new GenerationOutputContext(this, controllerOutputPath))
            {
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
                {
                    await output.Generate("ViewController.cshtml", model.Model.Name + "ControllerGen.cs", model);
                }
            }

            Console.WriteLine("-- Generating Views");
            string viewOutputPath = Path.Combine(
                    WebProject.ProjectFullPath,
                    areaLocation,
                    "Views", "Generated");
            using (var output = new GenerationOutputContext(this, viewOutputPath))
            {
                foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
                {
                    var filename = Path.Combine(model.Model.Name, "Table.cshtml");
                    await output.Generate("TableView.cshtml", filename, model);

                    filename = Path.Combine(model.Model.Name, "Cards.cshtml");
                    await output.Generate("CardView.cshtml", filename, model);

                    filename = Path.Combine(model.Model.Name, "CreateEdit.cshtml");
                    await output.Generate("CreateEditView.cshtml", filename, model);
                }
            }

            //await layoutDependencyInstaller.InstallDependencies();

            var tsReferenceOutputPath = Path.Combine(WebProject.ProjectFullPath, ScriptsFolderName);
            GenerateTSReferenceFiles(tsReferenceOutputPath);

            //await GenerateTypeScriptDocs(scriptOutputPath);

            Console.WriteLine("-- Generation Complete --");
        }

        private async Task GenerateTypeScriptDocs(string path)
        {
            var dir = new DirectoryInfo(path);
            Console.WriteLine($"-- Creating Doc Files");
            foreach (var file in dir.GetFiles("*.ts"))
            {
                // don't gen json documentation for definition files
                if (!file.FullName.EndsWith(".d.ts"))
                {
                    var reader = file.OpenText();
                    var doc = new TypeScriptDocumentation();
                    doc.TsFilename = file.Name;
                    doc.Generate(await reader.ReadToEndAsync());
                    var serializer = Newtonsoft.Json.JsonSerializer.Create();
                    // Create the doc file.
                    FileInfo docFile = new FileInfo(file.FullName.Replace(".ts", ".json"));
                    // Remove it if it exists.
                    try
                    {
                        if (docFile.Exists) docFile.Delete();
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(3000);
                        try
                        {
                            if (docFile.Exists) docFile.Delete();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Could not delete file {docFile.FullName}");
                        }
                    }
                    using (var tw = docFile.CreateText())
                    {
                        serializer.Serialize(tw, doc);
                        tw.Close();
                    }
                }
            }
        }

        private void GenerateTSReferenceFiles(string path)
        {
            var fileName = "coalesce.dependencies.d.ts";
            var generateDeps = Path.Combine(path, fileName);
            if (!File.Exists(generateDeps))
            {
                CopyToDestination(
                    fileName: fileName,
                    sourcePath: "Templates/Scripts",
                    destinationPath: path);
            }


            var fileContents = new List<string>
            {
                "\n\n",
                "// This file is automatically generated.",
                "// It is not in the generated folder for ease-of-use (no relative paths).",
                "// This file must remain in place relative to the generated scripts (<WebProject>/Scripts/Generated).",
                "\n\n",
                $"/// <reference path=\"coalesce.dependencies.d.ts\" />"
            };

            // Do files in the Generated folder.
            var dir = new DirectoryInfo(path + "\\Generated");
            foreach (var file in dir.GetFiles("*.ts"))
            {
                if ((file.Name.StartsWith("intellitect", true, CultureInfo.InvariantCulture) || file.Name.StartsWith("ko.", true, CultureInfo.InvariantCulture)) &&
                    !file.Name.EndsWith(".d.ts"))
                {
                    fileContents.Add($"/// <reference path=\"Generated\\{file.Name}\" />");
                }
            }

            // Do files in the Partials folder.
            dir = new DirectoryInfo(path + "\\Partials");
            if (dir.Exists)
            { 
                foreach (var file in dir.GetFiles("*.ts"))
                {
                    fileContents.Add($"/// <reference path=\"Partials\\{file.Name}\" />");
                }
            }

            var old = Path.Combine(path, "intellitect.references.d.ts");
            if (File.Exists(old)) File.Delete(old);
            // Write the file with the array list of content.
            File.WriteAllLines(Path.Combine(path, "viewmodels.generated.d.ts"), fileContents);
        }
    }
}
