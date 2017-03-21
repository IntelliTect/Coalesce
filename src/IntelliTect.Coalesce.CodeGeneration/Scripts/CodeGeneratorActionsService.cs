// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGeneration.Core;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System.Security.Cryptography;
using System.Linq;
using IntelliTect.Coalesce.CodeGeneration.Common;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public class CodeGeneratorActionsService : ICodeGeneratorActionsService
    {
        private readonly IFilesLocator _filesLocator;
        private readonly IFileSystem _fileSystem;
        private readonly RazorTemplating _templatingService;

        private Dictionary<string, RazorTemplateBase> _templateCache = new Dictionary<string, RazorTemplateBase>();

        public CodeGeneratorActionsService(
            RazorTemplating templatingService,
            IFilesLocator filesLocator)
            : this(templatingService, filesLocator, new DefaultFileSystem())
        {
        }

        internal CodeGeneratorActionsService(
            RazorTemplating templatingService,
            IFilesLocator filesLocator,
            IFileSystem fileSystem)
        {
            _templatingService = templatingService;
            _filesLocator = filesLocator;
            _fileSystem = fileSystem;
        }

        public async Task AddFileAsync(string outputPath, string sourceFilePath)
        {
            ExceptionUtilities.ValidateStringArgument(outputPath, "outputPath");
            ExceptionUtilities.ValidateStringArgument(sourceFilePath, "sourceFilePath");

            if (!_fileSystem.FileExists(sourceFilePath))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    MessageStrings.FileNotFound,
                    sourceFilePath));
            }

            using (var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                await AddFileHelper(outputPath, fileStream);
            }
        }

        public async Task AddFileFromTemplateAsync(string outputPath, string templateName,
            IEnumerable<string> templateFolders,
            object templateModel)
        {
            if (templateFolders == null)
            {
                throw new ArgumentNullException(nameof(templateFolders));
            }

            ExceptionUtilities.ValidateStringArgument(outputPath, "outputPath");
            ExceptionUtilities.ValidateStringArgument(templateName, "templateName");

            var templatePath = _filesLocator.GetFilePath(templateName, templateFolders);
            if (string.IsNullOrEmpty(templatePath))
            {
                throw new InvalidOperationException(string.Format(
                    MessageStrings.TemplateFileNotFound,
                    templateName,
                    string.Join(";", templateFolders)));
            }

            Debug.Assert(_fileSystem.FileExists(templatePath));

            RazorTemplateBase razorTemplate;
            TemplateResult templateResult;
            if (!_templateCache.TryGetValue(templatePath, out razorTemplate))
            {
                var templateContent = _fileSystem.ReadAllText(templatePath);
                razorTemplate = _templatingService.GetCompiledTemplate(templateContent);
                _templateCache.Add(templatePath, razorTemplate);
            }

            if (razorTemplate == null) throw new Exception("Compiled template does not exist. Probably means that compile failed due to missing references.");
            if (templateModel == null) throw new Exception("Model missing for generating code.");

            templateResult = await _templatingService.RunTemplateAsync(razorTemplate, templateModel);

            if (templateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(string.Format(
                    MessageStrings.TemplateProcessingError,
                    templatePath,
                    templateResult.ProcessingException.Message));
            }

            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(templateResult.GeneratedText)))
            {
                await AddFileHelper(outputPath, sourceStream);
            }
        }

        private async Task AddFileHelper(string outputPath, Stream sourceStream)
        {
            _fileSystem.CreateDirectory(Path.GetDirectoryName(outputPath));
            bool hasChange = true;

            if (_fileSystem.FileExists(outputPath))
            {
                if (!FileUtilities.HasDifferences(sourceStream, outputPath))
                {
                    hasChange = false;
                }
                else
                {
                    _fileSystem.MakeFileWritable(outputPath);
                }

            }

            if (hasChange)
            {
                sourceStream.Seek(0, SeekOrigin.Begin);
                await _fileSystem.AddFileAsync(outputPath, sourceStream);
            }
        }
    }
}