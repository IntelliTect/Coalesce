using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ParameterViewModel : IAttributeProvider
    {
        public abstract string Name { get; }

        public TypeViewModel Type { get; protected set; }

        public bool IsManualDI => IsAContext || IsAUser || IsAnIncludeTree;

        public bool IsInjected => HasAttribute<InjectAttribute>();

        public bool IsDI => IsManualDI || IsInjected;

        public bool IsAContext => Type.IsA<DbContext>();

        public bool IsAUser => Type.IsA<ClaimsPrincipal>();

        public bool IsAnIncludeTree => Type.IsA<IncludeTree>();

        /// <summary>
        /// Returns the parameter to pass to the actual method accounting for DI.
        /// </summary>
        public string CsArgumentName
        {
            get
            {
                if (IsAContext) return "Db";
                if (IsAUser) return "User";
                if (IsAnIncludeTree) return "out includeTree";
                return Name.ToCamelCase();
            }
        }

        public string PascalCaseName => Name.ToPascalCase();

        public bool ConvertsFromJsString => Type.IsNumber || Type.IsString || Type.IsDate || Type.IsBool || Type.IsEnum;

        public string CsDeclaration => $"{(IsInjected ? "[FromServices] " : "")}{Type.FullyQualifiedNameWithTypeParams} {Name.ToCamelCase()}";

        /// <summary>
        /// Additional conversion to serialize to send to server. For example a moment(Date) adds .toDate()
        /// </summary>
        public string TsConversion(string argument)
        {
            if (Type.IsDate)
            {
                return $"{argument} ? {argument}.format() : null";
            }
            return argument;
        }

        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
