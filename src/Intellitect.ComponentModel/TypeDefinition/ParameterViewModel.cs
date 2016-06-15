using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition
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

        public bool IsDI { get
            {
                return IsAContext || IsAUser;
            } }

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
        public object CsArgumentName
        {
            get
            {
                if (IsAContext) return "Db";
                if (IsAUser) return "User";
                return Name;
            }
        }
    }
}
