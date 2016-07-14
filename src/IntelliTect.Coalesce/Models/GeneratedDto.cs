using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public class GeneratedDto<T, TDto>
    {
        public virtual bool OnSecurityTrim(ClaimsPrincipal user, string includes)
        {
            return false;
        }

        public virtual bool OnUpdate(T entity, ClaimsPrincipal user, string includes)
        {
            return false;
        }
    }
}
