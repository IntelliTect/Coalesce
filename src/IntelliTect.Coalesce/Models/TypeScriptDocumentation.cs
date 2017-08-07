using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public class TypeScriptDocumentation
    {
        public string Name { get; set; }
        public List<TypeScriptFunction> Functions { get; } = new List<TypeScriptFunction>();
        public string Namespace { get; set; }
        public string TsFilename { get; set; }

        private List<string> Namespaces { get; } = new List<string>();




        /// <summary>
        /// Pass in the text of a typescript file to build the documentation
        /// </summary>
        /// <param name="file"></param>
        /// <param name="className">A specific class name to look for.</param>
        public void Generate(string file, string className = null)
        {
            // Read it a line at a time and build the docs.
            StringBuilder relevantLines = new StringBuilder();

            file = file.Replace("\r", "");
            foreach (var rawLine in file.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();
                //Console.WriteLine(line);
                // Determine what type of line it is.
                if (line.StartsWith("module "))
                {
                    Namespaces.Add(CleanVariable(line, "module"));
                }
                else if (line.StartsWith("export class "))
                {
                    Name = CleanVariable(line, "export class");
                }
                else if (line.StartsWith("class "))
                {
                    Name = CleanVariable(line, "class");
                }
                if (className != null && Name != null && Name != className)
                    continue;

                // AppendLine uses a CRLF. We just want a LF.
                relevantLines.Append(line);
                relevantLines.Append('\n');
            }
            Namespace = string.Join(".", Namespaces);

            var classContents = relevantLines.ToString();


            // Common capture group for member names
            var nameCapture = @"public\s+(?<name>[^\s:={}()]+)\s*";

            // Matches any public member and any comments preceding it. This includes a single block comment (/* ... */ or /** ... */), or multiple single-line comments (// ...).
            var classMemberRegex = new Regex($@"(?<comment>(?:(?:\/\/.*?\s*)|(?s:\/\*(?s:[^*]|\*+(?=[^\/]))*\*\/\s*))*?)\s*{nameCapture}", RegexOptions.Multiline);

            // Matches any public member that is explicitly typed. Examples:
            //      public isChecked: KnockoutObservable<boolean>
            //      public loadFromDto: (data: any, force?: boolean, allowCollectionDeletes?: boolean) => void;
            var explicitTypesRegex = new Regex($@"{nameCapture}:\s*(?<type>.*?)\s*(?:=(?!>)|;)", RegexOptions.Multiline);

            // Matches any public member that is defined as an arrow function. Examples:
            //      public save = (callback?: (self: T) => void) => { ... }
            //      public nextPage = () => { ... }
            var arrowFunctionRegex = new Regex($@"{nameCapture}=\s*(?<type>.*?)\s*=>\s*{{", RegexOptions.Multiline);

            // Matches any public member that is not explicitly typed and is not an arrow function. Examples:
            //      public includes = "";
            //      public parent = null;
            var untypedNonFunctionRegex = new Regex($@"{nameCapture}=\s*[^\s(]", RegexOptions.Multiline);



            IEnumerable<Match> Matches(Regex regex) => regex.Matches(classContents).OfType<Match>();

            var allMembersWithComments = Matches(classMemberRegex)
                .ToDictionary(
                    m => m.Groups["name"].Value,
                    m => string.Join("\n", m.Groups["comment"].Value.Split('\n').Select(l => l.Trim(' ', '\t', '/', '*')))
                );

            var members = Matches(explicitTypesRegex)
                .Concat(Matches(arrowFunctionRegex))
                .Concat(Matches(untypedNonFunctionRegex))
                
                // Quick & dirty way of doing DistinctBy
                .GroupBy(m => m.Groups["name"].Value)
                .Select(x => x.First())
                
                .Select(m =>
                {
                    var name = m.Groups["name"].Value;
                    var type = m.Groups["type"]?.Value;
                    return new TypeScriptFunction
                    {
                        Name = name,
                        Comment = allMembersWithComments[name],
                        Type = type,
                        Observable = type.Contains("KnockoutObservable"),
                    };
                })
                .ToList();

            Functions.AddRange(members);
        }

        private string CleanVariable(string line, string pre)
        {
            var parts = line.Split(new[] { pre }, StringSplitOptions.None);
            var result = parts[1].Trim().Split(new[] { ' ', '{', ':', '<' })[0];
            return result;
        }

    }

    public class TypeScriptFunction
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Observable { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Type} Obs: {Observable} \n   {Comment}";
        }
    }
}
