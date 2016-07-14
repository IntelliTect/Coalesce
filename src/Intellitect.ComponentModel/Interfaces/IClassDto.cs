using System.Security.Claims;

namespace Intellitect.ComponentModel.Interfaces
{
    public interface IClassDto
    {
        void Update(object obj, ClaimsPrincipal user, string includes);
        void SecurityTrim(ClaimsPrincipal user, string includes);
    }
}
