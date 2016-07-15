using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;

namespace Intellitect.ComponentModel.Mapping
{
    public static class Mapper<T, TDto> where TDto : IClassDto<T, TDto>, new()
    {
        public static TDto ObjToDtoMapper(T obj, ClaimsPrincipal user, string includes)
        {
            var creator = new TDto();
            var dto = creator.CreateInstance(obj, user, includes);
            
            return dto;
        }

        public static void DtoToObjMapper(TDto dto, T entity, ClaimsPrincipal user, string includes)
        {
            dto.Update(entity, user, includes);
        }
    }
}
