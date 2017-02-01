// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.Extensions.ProjectModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using System.Collections.Generic;
using IntelliTect.Coalesce.CodeGeneration.Common;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    //Todo: Make this internal and expose as a sevice
    public class RazorTemplating : ITemplating
    {
        private ProjectContext _projectContext;

        public RazorTemplating(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        public RazorTemplateBase GetCompiledTemplate(string content)
        {
            RazorTemplatingHost host = new RazorTemplatingHost(typeof(RazorTemplateBase));
            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            using (var reader = new StringReader(content))
            {
                var generatorResults = engine.GenerateCode(reader);
                if (!generatorResults.Success)
                {
                    using (var newReader = new StringReader(content))
                    {
                        var readerText = newReader.ReadToEnd();
                        File.WriteAllText("c:\\temp\\badreader.txt", readerText);
                    }
                    return null;
                }

                var templateResult = Compile(generatorResults.GeneratedCode);
                if (templateResult.Messages.Any())
                {
                    File.WriteAllText("c:\\temp\\badtemplate.txt", generatorResults.GeneratedCode);
                    return null;
                }

                var compiledObject = Activator.CreateInstance(templateResult.CompiledType);
                var razorTemplate = compiledObject as RazorTemplateBase;

                return razorTemplate;
            }
        }

        public Task<TemplateResult> RunTemplateAsync(string content, dynamic templateModel)
        {
            throw new NotImplementedException();
        }

        public async Task<TemplateResult> RunTemplateAsync(RazorTemplateBase template, dynamic templateModel)
        {
            string result = String.Empty;
            if (template != null)
            {
                template.Model = templateModel;
                //ToDo: If there are errors executing the code, they are missed here.
                try
                {
                    result = await template.ExecuteTemplate();
                }
                catch (Exception ex)
                {
                    return new TemplateResult()
                    {
                        GeneratedText = "",
                        ProcessingException = new TemplateProcessingException(new string[] { ex.ToString() }, "")
                    };
                }
            }

            return new TemplateResult()
            {
                GeneratedText = result,
                ProcessingException = null
            };
        }


        public Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation.CompilationResult Compile(string content)
        {
            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(content) };

            var references = GetApplicationReferences();
            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        syntaxTrees: syntaxTrees,
                        references: references);


            var result = CommonUtilities.GetAssemblyFromCompilation(new DefaultAssemblyLoadContext(), compilation);
            if (result.Success)
            {
                var type = result.Assembly.GetExportedTypes()
                                   .First();

                return Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation.CompilationResult.Successful(string.Empty, type);
            }
            else
            {
                return Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation.CompilationResult.Failed(content, result.ErrorMessages);
            }
        }

        //Todo: This is using application references to compile the template,
        //perhaps that's not right, we should use the dependencies of the caller
        //who calls the templating API.
        private List<MetadataReference> GetApplicationReferences()
        {
            var references = new List<MetadataReference>();

            // Todo: When the target app references scaffolders as nuget packages rather than project references,
            // we need to ensure all dependencies for compiling the generated razor template.
            // This requires further thinking for custom scaffolders because they may be using
            // some other references which are not available in any of these closures.
            // As the above comment, the right thing to do here is to use the dependency closure of
            // the assembly which has the template.
            var exports = _projectContext.CompilationAssemblies;

            if (exports != null)
            {
                foreach (var metadataReference in exports.SelectMany(exp => exp.GetMetadataReference(throwOnError: false)))
                {
                    references.Add(metadataReference);
                }
            }

            return references;
        }


    }
}