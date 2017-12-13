using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Mapping;
using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public interface IClassDto<T, TDto>
    {
        void Update(T obj, IMappingContext context);

        TDto CreateInstance(T obj, IMappingContext context, IncludeTree tree = null);
    }
}
