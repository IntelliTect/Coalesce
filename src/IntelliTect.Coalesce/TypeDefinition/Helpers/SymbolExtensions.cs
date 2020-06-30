using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public static class SymbolExtensions
    {
        public static AttributeData? GetAttribute<TAttribute>(this ISymbol symbol)
        {
            return symbol.GetAttributes().SingleOrDefault(a => a.AttributeClass.Name == typeof(TAttribute).Name);
        }

        public static bool HasAttribute<TAttribute>(this ISymbol symbol)
        {
            return symbol.GetAttribute<TAttribute>() != null;
        }

        public static object? GetAttributeValue<TAttribute>(this ISymbol symbol, string valueName) where TAttribute : Attribute
        {
            var attributeData = symbol.GetAttribute<TAttribute>();
            return attributeData?.GetPropertyValue(valueName, null);
        }

        public static object? GetPropertyValue(this AttributeData attributeData, string propertyName, object? defaultValue)
        {
            if (attributeData == null) return defaultValue;
            var namedArgument = attributeData.NamedArguments.SingleOrDefault(na => na.Key == propertyName);

            if (namedArgument.Key != null)
            {
                return namedArgument.Value.Value;
            }

            TypedConstant constructorArgument = attributeData
                .AttributeConstructor.Parameters
                .Zip(attributeData.ConstructorArguments, Tuple.Create)
                // Look for ctor params with a matching name (case insensitive)
                .FirstOrDefault(t => string.Equals(t.Item1.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                ?.Item2
                // If we didn't find one, see if there is just a single ctor param. If so, this is almost certainly what we were looking for.
                ?? (attributeData.ConstructorArguments.Length == 1 ? attributeData.ConstructorArguments.SingleOrDefault() : default);

            if (constructorArgument.IsNull) return defaultValue;

            return constructorArgument.Value;
        }

        public static string ExtractXmlComments(this ISymbol symbol)
        {
            string returnValue = "";
            XmlDocument xmlDocumentation = new XmlDocument();
            var xmlDocs = symbol.GetDocumentationCommentXml();
            if (!string.IsNullOrEmpty(xmlDocs))
            {
                try
                {
                    xmlDocumentation.LoadXml(xmlDocs);
                    returnValue = xmlDocumentation.SelectSingleNode("/member/summary")?.InnerText.Trim() ?? "";
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
            }

            return Regex.Replace(returnValue, "\n( +)", "\n");
        }
    }
}
