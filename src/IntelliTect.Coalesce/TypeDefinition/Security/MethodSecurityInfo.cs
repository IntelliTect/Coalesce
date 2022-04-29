using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class MethodSecurityInfo
    {
        public MethodSecurityInfo(SecurityPermission execute)
        {
            Execute = execute;
        }

        public SecurityPermission Execute { get; }

        public bool IsExecuteAllowed(ClaimsPrincipal? user) => Execute.IsAllowed(user);

        public override string ToString() => $"Execute: {Execute}";
    }
}
