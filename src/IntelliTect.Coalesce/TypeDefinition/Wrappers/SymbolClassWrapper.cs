using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Interfaces;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolClassWrapper : ClassWrapper
    {
        public SymbolClassWrapper(ITypeSymbol symbol)
        {
            Symbol = symbol;
        }


        public override string Name
        {
            get
            {
                return Symbol.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return FullNamespace(Symbol.ContainingNamespace);
            }
        }

        /// <summary>
        /// Recursive function to get the full namespace.
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        private string FullNamespace(INamespaceSymbol ns)
        {
            if (ns.ContainingNamespace != null && !string.IsNullOrWhiteSpace(ns.Name))
            {
                var rest = FullNamespace(ns.ContainingNamespace);
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    return FullNamespace(ns.ContainingNamespace) + "." + ns.Name;
                }
                return ns.Name;
            }
            return "";
        }

        public override string Comment { get { return SymbolHelper.ExtractXmlComments(Symbol); } }

        public override List<PropertyWrapper> Properties
        {
            get
            {
                var result = new List<PropertyWrapper>();
                var properties = Symbol.GetMembers().Where(f => f.Kind == SymbolKind.Property);
                foreach (var prop in properties)
                {
                    result.Add(new SymbolPropertyWrapper((IPropertySymbol)prop));
                }
                // Add properties from the base class
                if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
                {
                    var parentSymbol = new SymbolClassWrapper(Symbol.BaseType);
                    result.AddRange(parentSymbol.Properties);
                }
                return result;
            }
        }

        public override List<MethodWrapper> RawMethods
        {
            get
            {
                var result = new List<MethodWrapper>();
                var methods = Symbol.GetMembers()
                    .Where(f => f.Kind == SymbolKind.Method && f.DeclaredAccessibility == Accessibility.Public)
                    .Cast<IMethodSymbol>()
                    .Where(f => f.MethodKind == MethodKind.Ordinary);
                foreach (var methodInfo in methods)
                {
                    result.Add(new SymbolMethodWrapper(methodInfo));
                }

                // Add methods from the base class
                if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
                {
                    var parentSymbol = new SymbolClassWrapper(Symbol.BaseType);
                    result.AddRange(parentSymbol.Methods
                        .Cast<SymbolMethodWrapper>()
                        // Don't add overriden methods
                        .Where(baseMethod => !methods.Any(method => method.OverriddenMethod == baseMethod.Symbol)
                    ));
                }
                return result;
            }
        }

        public override bool IsComplexType
        {
            get
            {
                return HasAttribute<ComplexTypeAttribute>();
            }
        }

        public override bool IsDto
        {
            get
            {
                return Symbol.AllInterfaces.Any(f => f.Name.Contains("IClassDto"));
            }
        }

        public override ClassViewModel DtoBaseType
        {
            get
            {
                var iDto = Symbol.AllInterfaces.FirstOrDefault(f => f.Name.Contains("IClassDto"));
                if (iDto != null)
                {
                    ClassViewModel baseModel = ReflectionRepository.GetClassViewModel(iDto.TypeArguments[0].Name);
                    return baseModel;
                }
                return null;
            }
        }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Symbol.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Symbol.HasAttribute<TAttribute>();
        }
        public override AttributeWrapper GetSecurityAttribute<TAttribute>() 
        {
            return new AttributeWrapper
            {
                AttributeData = Symbol.GetAttribute<TAttribute>()
            };
        }
    }
}
