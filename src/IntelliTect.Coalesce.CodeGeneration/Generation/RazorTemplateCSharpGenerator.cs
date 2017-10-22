using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateCSharpGenerator<TModel> : RazorTemplateGenerator<TModel>
    {
        public RazorTemplateCSharpGenerator(RazorServices razorServices) : base(razorServices)
        {
        }

        public string Namespace => GenerationContext.OutputNamespaceRoot;

        public override async Task<Stream> GetOutputAsync()
        {
            using (var output = await base.GetOutputAsync())
            {
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(SourceText.From(output));
                var root = syntaxTree.GetRoot();
                root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());

                Stream formattedOutput = new MemoryStream((int)output.Length);
                root.SerializeTo(formattedOutput);
                await formattedOutput.FlushAsync();
                return formattedOutput;
            }
        }
    }
}
