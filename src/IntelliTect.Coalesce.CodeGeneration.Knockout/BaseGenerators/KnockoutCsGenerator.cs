using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutCsGenerator : KnockoutGenerator
    {
        public KnockoutCsGenerator(GeneratorServices services) : base(services) { }
        
        public string Namespace => GenerationContext.OutputNamespaceRoot;

        [GeneratorConfig]
        public int IndentationSize { get; set; } = 4;

        public override Task<string> BuildOutputAsync()
        {
            var b = new CSharpCodeBuilder();
            BuildOutput(b);
            string output = b.ToString();

            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(output));
            var root = syntaxTree.GetRoot();

            using (var workspace = new AdhocWorkspace())
            {
                var options = workspace.Options
                    .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, Environment.NewLine)
                    .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
                    .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, IndentationSize)
                    .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
                    .WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, true);

                root = Formatter.Format(root, workspace, options);

                return Task.FromResult(root.ToFullString());
            }
        }

        public abstract void BuildOutput(CSharpCodeBuilder b);
    }
}
