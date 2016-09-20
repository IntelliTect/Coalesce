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
                return IsAContext || IsAUser;
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

        /// <summary>
        /// Returns the parameter to pass to the actual method accounting for DI.
        /// </summary>
        public string CsArgumentName
        {
            get
            {
                if (IsAContext) return "Db";
                if (IsAUser) return "User";
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
        public string TsConversion
        {
            get
            {
                if (Type.IsDate)
                {
                    return ".format()";
                }
                return "";
            }
        }
    }
}
