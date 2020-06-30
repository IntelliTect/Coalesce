using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Mapping;
using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public interface IClassDto<in T>
         where T : class
    {
        void MapTo(T obj, IMappingContext context);

        void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null);
    }
}
