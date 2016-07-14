using System.Collections.Generic;
using System.Security.Claims;

namespace Intellitect.ComponentModel.Interfaces
{
    public interface IClassDto<T, TDto>
    {
        void Update(T obj, ClaimsPrincipal user, string includes);
        void SecurityTrim(ClaimsPrincipal user, string includes);

        TDto CreateInstance(T obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<string, object> objects = null);
    }
}
