using IntelliTect.Coalesce.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public class GeneratedDto<T, TDto>
    {
        public virtual bool OnUpdate(T entity, IMappingContext context)
        {
            return false;
        }
    }
}
