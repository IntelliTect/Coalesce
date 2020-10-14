using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace IntelliTect.Coalesce.Api
{
    public interface IStandardCrudStrategy
    {
        /// <summary>
        /// The user making the request.
        /// </summary>
        ClaimsPrincipal? User { get; }

        /// <summary>
        /// A ClassViewModel representing the type T that is handled by these strategies.
        /// </summary>
        ClassViewModel ClassViewModel { get; }
    }
}
