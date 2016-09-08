using IntelliTect.Coalesce.Helpers;
using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Interfaces
{
    public interface IClassDto<T, TDto>
    {
        void Update(T obj, ClaimsPrincipal user, string includes);

        TDto CreateInstance(T obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null);
    }
}
