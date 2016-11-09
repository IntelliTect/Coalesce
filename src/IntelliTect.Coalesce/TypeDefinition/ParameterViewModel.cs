using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ParameterViewModel
    {
        public string Name { get; }
        public TypeViewModel Type { get; }

        public ParameterViewModel(string name, TypeViewModel type)
        {
            Name = name;
            Type = type;
        }

        public bool IsDI
        {
            get
            {
                return IsAContext || IsAUser || IsAnIncludeTree;
            }
        }

        public bool IsAContext
        {
            get
            {
                return Type.IsA<DbContext>();
            }
        }

        public bool IsAUser
        {
            get
            {
                return Type.IsA<ClaimsPrincipal>();
            }
        }

        public bool IsAnIncludeTree
        {
            get
            {
                return Type.IsA<IncludeTree>();
            }
        }

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

        public string PascalCaseName
        {
            get
            {
                return Name.ToPascalCase();
            }
        }

        public bool ConvertsFromJsString
        {
            get
            {
                return Type.IsNumber || Type.IsString || Type.IsDate || Type.IsBool || Type.IsEnum;
            }
        }

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
    }
}
