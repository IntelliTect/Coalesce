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
            var constructorArgument = attributeData.ConstructorArguments.FirstOrDefault();

            if (namedArgument.Key == null && constructorArgument.IsNull) return defaultValue;

            var propertyValue = namedArgument.Key != null ? namedArgument.Value : constructorArgument;

            return propertyValue.Value;
        }

        // Not sure if these even work...
        public static Nullable<T> GetAttributeValue<TAttribute, T>(this ISymbol symbol, Expression<Func<TAttribute, T>> propertySelector) where TAttribute : Attribute where T : struct
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
