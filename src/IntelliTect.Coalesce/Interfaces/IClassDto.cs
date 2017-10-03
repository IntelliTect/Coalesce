using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Mapping;
using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Interfaces
{
    public interface IClassDto<T, TDto>
    {
        void Update(T obj, IMappingContext context);

        TDto CreateInstance(T obj, IMappingContext context, IncludeTree tree = null);
    }
}
