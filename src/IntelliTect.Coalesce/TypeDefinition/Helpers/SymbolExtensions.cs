using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace IntelliTect.Coalesce.TypeDefinition;

public class SymbolAttributeViewModel<TAttribute> : AttributeViewModel<TAttribute>
    where TAttribute : Attribute
{
    public AttributeData AttributeData { get; }

    public SymbolAttributeViewModel(AttributeData attributeData, ReflectionRepository? rr) : base(rr)
    {
        AttributeData = attributeData;
    }

    public override TypeViewModel Type => ReflectionRepository.GetOrAddType(AttributeData.AttributeClass!);

    private object? MapValue(TypedConstant arg) => arg.Kind == TypedConstantKind.Array
        ? arg.Values.Select(v => MapScalar(v.Value)).ToArray()
        : MapScalar(arg.Value);

    private object? MapScalar(object? ret) => ret switch
    {
        ITypeSymbol symbolValue => ReflectionRepository.Global.GetOrAddType(symbolValue),
        _ => ret
    };

    private IEnumerable<KeyValuePair<string, object?>> EnumerateValues()
    {
        foreach (var named in AttributeData.NamedArguments)
        {
            yield return new(named.Key, MapValue(named.Value));
        }

        if (AttributeData.AttributeConstructor is { } ctor)
        {
            var args = AttributeData.ConstructorArguments;
            for (int i = 0; i < ctor.Parameters.Length && i < args.Length; i++)
            {
                if (!args[i].IsNull)
                    yield return new(ctor.Parameters[i].Name, MapValue(args[i]));
            }
        }
    }

    public override IReadOnlyList<KeyValuePair<string, object?>> GetAllValues()
        => [.. EnumerateValues()];

    public override object? GetValue(string propertyName)
        => EnumerateValues()
            .FirstOrDefault(kv => string.Equals(kv.Key, propertyName, StringComparison.OrdinalIgnoreCase))
            .Value;
}

internal class SymbolAttributeProvider(ISymbol symbol) : IAttributeProvider
{
    public IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
        where TAttribute : Attribute
        => symbol.GetAttributes<TAttribute>();
}

public static class SymbolExtensions
{
    private static bool IsInherited(AttributeData attribute)
    {
        // From https://stackoverflow.com/a/67601737

        if (attribute.AttributeClass == null)
        {
            return false;
        }

        foreach (var attributeAttribute in attribute.AttributeClass.GetAttributes())
        {
            var attrClass = attributeAttribute.AttributeClass;
            if ((attrClass?.Name) != nameof(AttributeUsageAttribute) ||
                (attrClass.ContainingNamespace?.Name) != "System")
            {
                continue;
            }

            foreach (var kvp in attributeAttribute.NamedArguments)
            {
                if (kvp.Key == nameof(AttributeUsageAttribute.Inherited))
                {
                    return (bool)kvp.Value.Value!;
                }
            }

            // Default value of Inherited is true
            return true;
        }

        // Inheritance is also automatic if the attribute has no [AttributeUsage].
        return true;
    }

    private static IEnumerable<AttributeData> GetAttributesWithInherited(this INamedTypeSymbol typeSymbol)
    {
        // From https://stackoverflow.com/a/67601737

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            yield return attribute;
        }

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            foreach (var attribute in baseType.GetAttributes())
            {
                if (IsInherited(attribute))
                {
                    yield return attribute;
                }
            }

            baseType = baseType.BaseType;
        }
    }

    public static IAttributeProvider GetAttributeProvider(this ISymbol symbol)
        => new SymbolAttributeProvider(symbol);

    public static IEnumerable<SymbolAttributeViewModel<TAttribute>> GetAttributes<TAttribute>(
        this ISymbol symbol,
        ReflectionRepository? rr = null)
        where TAttribute : Attribute
    {
        return (symbol is INamedTypeSymbol named
            ? named.GetAttributesWithInherited()
            : symbol.GetAttributes())
            // GetAttributesWithInherited iterates backwards in the inheritance hierarchy,
            // so this will return the attributes from the most specific type first.
            .Where(a => (rr ?? ReflectionRepository.Global).GetOrAddType(a.AttributeClass!).IsA<TAttribute>())
            .Select(a => new SymbolAttributeViewModel<TAttribute>(a, rr));
    }

    public static string? ExtractXmlComments(this ISymbol symbol)
    {
        string? xmlDocs = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrEmpty(xmlDocs))
        {
            return null;
        }

        try
        {
            XmlDocument xmlDocumentation = new XmlDocument();
            xmlDocumentation.LoadXml(xmlDocs);
            foreach (var see in xmlDocumentation.SelectNodes("//see")!.OfType<XmlNode>())
            {
                string value = see.Attributes?["cref"]?.Value ?? "";
                var idx = value.LastIndexOf('.');
                if (idx < 0) continue;

                value = value.Substring(idx + 1);
                see.ParentNode!.ReplaceChild(xmlDocumentation.CreateTextNode(value), see);
            }
            string summary = xmlDocumentation.SelectSingleNode("/member/summary")?.InnerText.Trim() ?? "";
            return Regex.Replace(summary, "\n( +)", "\n");
        }
        catch (Exception)
        {
            // Non-critical error. Write it out and ignore.
            // Usually this is because of a badly-formed XML comment in the source code.
            Console.Error.WriteLine($"Error trying to parse XML Comments for symbol {symbol.ToDisplayString()}:");
            Console.Error.WriteLine(xmlDocs);
            // The full exception isn't really important. Usually the error will be an XML comment inside of xmlDocs.
            //Console.Error.WriteLine(ex);
        }

        return null;
    }
}
