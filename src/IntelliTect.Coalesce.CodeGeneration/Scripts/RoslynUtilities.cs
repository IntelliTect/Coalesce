using System;
using System.Collections.Generic;
#if NET451
using System.ComponentModel;
#endif
using System.Linq;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    internal static class RoslynUtilities
    {
        public static IEnumerable<ITypeSymbol> GetDirectTypesInCompilation(Compilation compilation)
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            var types = new List<ITypeSymbol>();
            CollectTypes(compilation.Assembly.GlobalNamespace, types);
            return types;
        }

        private static void CollectTypes(INamespaceSymbol ns, List<ITypeSymbol> types)
        {
            types.AddRange(ns.GetTypeMembers().Cast<ITypeSymbol>());

            foreach (var nestedNs in ns.GetNamespaceMembers())
            {
                CollectTypes(nestedNs, types);
            }
        }
    }
}
