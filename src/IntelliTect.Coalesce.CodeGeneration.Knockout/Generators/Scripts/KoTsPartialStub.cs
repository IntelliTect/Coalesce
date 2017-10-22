﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoTsPartialStub : RazorTemplateGenerator<ClassViewModel>
    {
        public KoTsPartialStub(
            CoalesceConfiguration coalesceConfig,
            ITemplateResolver resolver,
            RazorTemplateCompiler compiler)
            : base(coalesceConfig, resolver, compiler)
        { }

        public override bool ShouldGenerate()
        {
            return !File.Exists(OutputPath);
        }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "KoViewModelPartial.cshtml");
    }
}