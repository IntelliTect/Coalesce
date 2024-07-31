using System;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Analyzer
{
    internal static class SymbolMixins
    {
        public static bool IsNativeMethodsClass(this ISymbol type)
        {
            return type.Kind == SymbolKind.NamedType && string.Equals(type.Name, "NativeMethods", StringComparison.Ordinal);
        }
    }
}
