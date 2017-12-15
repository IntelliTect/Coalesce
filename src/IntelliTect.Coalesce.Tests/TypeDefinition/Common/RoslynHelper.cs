using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.Common
{
    public static class RoslynHelper
    {
        public static INamedTypeSymbol GetSymbolFromFile(string filename)
        {
            // Load the bool.cs file. It should be in the bin folder
            var cs = File.ReadAllText(filename);
            var tree = CSharpSyntaxTree.ParseText(cs);
            var compilation = CSharpCompilation.Create("MyCompilation", new[] { tree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            var symbol = semanticModel.GetDeclaredSymbol(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());
            return symbol;
        }
    }
}
