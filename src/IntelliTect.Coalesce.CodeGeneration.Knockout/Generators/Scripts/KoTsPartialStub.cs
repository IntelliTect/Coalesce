using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoTsPartialStub : KnockoutViewModelGenerator
    {
        public KoTsPartialStub(RazorTemplateServices razorServices) : base(razorServices)
        {
            ShouldWriteGeneratedBy = false;
        }

        public override bool ShouldGenerate()
        {
            return !File.Exists(OutputPath);
        }

        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ViewModelModuleName}"))
            {
                using (b.Block($"export class {Model.ViewModelClassName} extends {Model.ViewModelGeneratedClassName}"))
                {
                    b.Line();
                    using (b.Block($"constructor(newItem?: object, parent?: Coalesce.BaseViewModel | {ListViewModelModuleName}.{Model.ListViewModelClassName})"))
                    {
                        b.Line("super(newItem, parent);");
                        b.Line();
                    }
                }
            }
        }
    }
}
