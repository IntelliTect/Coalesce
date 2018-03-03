using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace CshtmlConvert
{

    class Program
    {
        static void Main(string[] args)
        {
            var di = new DirectoryInfo("D:/Work/Coalesce - 20/src/IntelliTect.Coalesce.CodeGeneration.Knockout/Templates");

            Parallel.ForEach(di.GetFiles().Where(f => f.Extension == ".cshtml"), file =>
            {
                //if (file.Name != "KoViewModel.cshtml") continue;

                var contents = File.ReadAllText(file.FullName);


                var sourceDoc = RazorSourceDocument.Create(contents, file.FullName);
                var codeDoc = RazorCodeDocument.Create(sourceDoc);

                RazorTemplateEngine engine = new CoalesceRazorTemplateEngine(
                    RazorEngine.Create(options =>
                    {
                        InheritsDirective.Register(options);
                    }),
                    RazorProject.Create(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()))
                );
                var generatorResults = engine.GenerateCode(codeDoc);

                var code = generatorResults.GeneratedCode
                    .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries )
                    .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                    .Select(l => l.Replace("WriteLiteral", "Write"))
                    ;

                var newCode = string.Join("\r\n", code);
                void RepeatReplace(string pattern, string replace, RegexOptions options = RegexOptions.None)
                {
                    var oldCode = newCode;
                    do
                    {
                        oldCode = newCode;
                        newCode = Regex.Replace(oldCode, pattern, replace, options);
                    } while (newCode != oldCode);
                    newCode = Format(newCode);
                }

                void Replace(string pattern, string replace, RegexOptions options = RegexOptions.None)
                {
                    newCode = Regex.Replace(newCode, pattern, replace, options);
                }

                // test
                // newCode = newCode.Replace("entire collections are loaded", "entire collect\\ions \"\"are loaded");

                // Mark escape sequences in verbatim strings 
                foreach (Match match in Regex.Matches(newCode, @"Write\(@""(.*?)""\);", RegexOptions.Singleline))
                    newCode = newCode.Replace(match.Value, match.Value.Replace("\"\"", "QUOTEMARKINVERBATIM"));

                foreach (Match match in Regex.Matches(newCode, @"Write\(@""(.*?)""\);", RegexOptions.Singleline))
                    newCode = newCode.Replace(match.Value, match.Value.Replace("\\", "SLASHINVERBATIM"));

                foreach (Match match in Regex.Matches(newCode, @"Write\(@""(.*?)""\);", RegexOptions.Singleline))
                    newCode = newCode.Replace(match.Value, match.Value.Replace("\r\n", "\\r\\n\");\r\nWrite(@\""));

                // Done with verbatim string splitting. Reconstruct them now.
                RepeatReplace("Write\\(@\"", "Write(\"");
                RepeatReplace("QUOTEMARKINVERBATIM", "\\\"");
                RepeatReplace("SLASHINVERBATIM", "\\\\");

                //RepeatRegex(@"Write\(@""(.*?)""""(.*?)""\);", @"Write(@""$1QUOTEMARKINVERBATIM$2"");", RegexOptions.Multiline | RegexOptions.Singleline);
                //RepeatRegex(@"Write\(@""(.*?)\\?(.*?)""\);", @"Write(@""$1SLASHINVERBATIM$2"");", RegexOptions.Multiline | RegexOptions.Singleline);

                // Split newlines in verbatim strings to multiple write calls.
                //RepeatRegex("Write\\(@\"(.*?)\r\n(\\s+)", "Write(@\"$1\\r\\n\");\r\nWrite(@\"$2", RegexOptions.Multiline | RegexOptions.Singleline);



                // Escape braces in writes for string interpolation
                RepeatReplace(@"(Write\(.*?[^\{])\{([^\{]|$)", "$1{{$2");
                RepeatReplace(@"(Write\(.*?[^\}])\}([^\}]|$)", "$1}}$2");

                // Split writes of multiple lines into multiple statements of single lines.
                RepeatReplace(@"Write\((.*?)\\r\\n", "b.Line($1\");\r\nWrite(\"");

                // Remove empty Write("") calls that were created by the previous replacement where a newline was at the end of the string.
                RepeatReplace(@"Write\(""""\);", "");


                // Replace non-string write calls with interpolated string write calls
                RepeatReplace(@"Write\(([^""].*?[^""])\);", @"Write($""{$1}"");");

                // Replace non-interpolated strings with interpolated strings
                RepeatReplace(@"Write\(""", @"Write($""");
                RepeatReplace(@"b.Line\(""", @"b.Line($""");

                // Replace multiple consecutive Write calls with single write calls
                RepeatReplace(@"Write\(\$""(.*?)""\);\s+Write\(\$""(.*?)""\);", @"Write($$""$1$2"");", RegexOptions.Multiline);
                RepeatReplace(@"Write\(\$""(.*?)""\);\s+b.Line\(\$""(.*?)""\);", @"b.Line($$""$1$2"");", RegexOptions.Multiline);

                // Un-interpolate strings that have no business being interpolated.
                newCode = Regex.Replace(
                    newCode, 
                    @"^(?!.*[^{]{[^{])(?!.*[^}]}[^}])\s*b\.Line\(\$""(.*)""\);\s*$", 
                    match => match.Value.Replace("{{", "{").Replace("}}", "}").Replace("b.Line($", "b.Line("), 
                    RegexOptions.Multiline);
                

                RepeatReplace("\r\n\r\n", "\r\n", RegexOptions.Multiline);

                Replace("\\\\t", "    ");
                Replace(@"b\.Line\(\$?""""\);", "b.Line();");
                Replace("public class Template", $"public class {file.Name.Replace(file.Extension, "")}Template");
                Replace(@"(ExecuteAsync\(\)
        {)", "$1\r\n            var b = new CodeBuilder();");

                newCode = @"using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using IntelliTect.Coalesce.Utilities;
" + newCode;

                File.WriteAllText(file.FullName + ".cs", newCode);

            });
        }

        static string Format(string text){
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(text));
            var root = syntaxTree.GetRoot();

            using (var workspace = new AdhocWorkspace())
            {
                var options = workspace.Options
                    .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, Environment.NewLine)
                    .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
                    .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4)
                    .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp,
                        FormattingOptions.IndentStyle.Smart)
                    .WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, true);

                root = Formatter.Format(root, workspace, options);
                return root.ToFullString();

            }
        }
    }

    internal class CoalesceRazorTemplateEngine : RazorTemplateEngine
    {
        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "System.Dynamic",
        };

        public CoalesceRazorTemplateEngine(RazorEngine engine, RazorProject project) : base(engine, project)
        {
            Options.DefaultImports = GetDefaultImports();
        }

        private static RazorSourceDocument GetDefaultImports()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (var ns in _defaultNamespaces)
                {
                    writer.WriteLine($"@using {ns}");
                }
                writer.Flush();

                stream.Position = 0;
                return RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
            }
        }
    }
}
