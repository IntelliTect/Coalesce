// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    //Todo: Make this internal and expose as a sevice
    public class RazorTemplating : ITemplating
    {
        private ICompilationService _compilationService;

        public RazorTemplating(ICompilationService compilationService)
        {
            if (compilationService == null)
            {
                throw new ArgumentNullException(nameof(compilationService));
            }

            _compilationService = compilationService;
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

                var templateResult = _compilationService.Compile(generatorResults.GeneratedCode);
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
    }
}