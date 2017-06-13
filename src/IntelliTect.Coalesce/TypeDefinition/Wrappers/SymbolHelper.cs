using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    public static class SymbolHelper
    {
        public static AttributeData GetAttribute<TAttribute>(this ISymbol symbol)
        {
            return symbol.GetAttributes().SingleOrDefault(a => a.AttributeClass.Name == typeof(TAttribute).Name);

        }

        public static bool HasAttribute<TAttribute>(this ISymbol symbol)
        {
            return symbol.GetAttribute<TAttribute>() != null;
        }

        public static Object GetAttributeValue<TAttribute>(this ISymbol symbol, string valueName) where TAttribute : Attribute
        {
            var attributeData = symbol.GetAttribute<TAttribute>();
            return attributeData.GetPropertyValue(valueName, null);
        }

        public static object GetPropertyValue(this AttributeData attributeData, string propertyName, object defaultValue)
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
                .FirstOrDefault(t => t.Item1.Name.ToLowerInvariant() == propertyName.ToLowerInvariant())
                ?.Item2
                // If we didn't find one, see if there is just a single ctor param. If so, this is almost certainly what we were looking for.
                ?? (attributeData.ConstructorArguments.Length == 1 ? attributeData.ConstructorArguments.SingleOrDefault() : default(TypedConstant));

            if (constructorArgument.IsNull) return defaultValue;

            return constructorArgument.Value;
        }

        // Not sure if these even work...
        public static T? GetAttributeValue<TAttribute, T>(this ISymbol symbol, Expression<Func<TAttribute, T>> propertySelector) where TAttribute : Attribute where T : struct
        {
            var result = symbol.GetAttributeValue<TAttribute>(propertySelector.Name);

            return (T)result;
        }

        public static T GetAttributeObject<TAttribute, T>(this ISymbol symbol, Expression<Func<TAttribute, T>> propertySelector) where TAttribute : Attribute where T : class
        {
            var result = symbol.GetAttributeValue<TAttribute>(propertySelector.Name);

            return result as T;
        }

        public static string ExtractXmlComments(ISymbol symbol)
        {
            string returnValue = "";
            XmlDocument xmlDocumentation = new XmlDocument();
            string xmlDocs = symbol.GetDocumentationCommentXml();
            if (xmlDocs.Length > 0)
            {
                xmlDocumentation.LoadXml(xmlDocs);
                returnValue = xmlDocumentation.SelectSingleNode("/member/summary").InnerText.Trim();
            }

            return returnValue;
        }
    }
}
