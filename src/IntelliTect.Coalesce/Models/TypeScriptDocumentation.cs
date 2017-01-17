using System;
using System.Collections.Generic;
using System.Linq;
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
            List<string> comments = new List<string>() ;
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
                if (className != null && Name != null && Name != className)
                    continue;

                if (line.StartsWith("public "))
                {
                    var fn = new TypeScriptFunction();
                    fn.Name = CleanVariable(line, "public");
                    fn.Comment = string.Join(Environment.NewLine, comments);
                    if (string.IsNullOrWhiteSpace(fn.Comment)) fn.Comment = "None";
                    fn.Observable = line.Contains("KnockoutObservable<") || line.Contains("KnockoutObservableArray<");
                    if (line.Contains(":"))
                    {
                        var second = line.Split(new[] { ":"}, 2, StringSplitOptions.None)[1].Trim();

                        var parts = second.Split(new[] {"= ", ";" }, StringSplitOptions.None);
                        var type = parts[0].Trim();
                        // If this is a function, make it clear.
                        if (type.StartsWith("(")) type = "function" + type;  
                        fn.Type = type;
                        Functions.Add(fn);
                    }else
                    {
                        fn.Type = "";
                    }
                    //Console.WriteLine(fn);
                }


                if (line.StartsWith("//")) {
                    comments.Add(line.Replace("//", "").Trim());
                }
                else
                {
                    comments.Clear();
                }


            }
            Namespace = string.Join(".", Namespaces);
            //Console.WriteLine($"Name: {Namespace}.{Name}");
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
