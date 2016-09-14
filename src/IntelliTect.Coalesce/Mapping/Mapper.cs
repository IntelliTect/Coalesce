using System.Security.Claims;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Concurrent;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Helpers.IncludeTree;

namespace IntelliTect.Coalesce.Mapping
{
    public static class Mapper<T, TDto> where TDto : IClassDto<T, TDto>, new()
    {
        public static TDto ObjToDtoMapper(T obj, ClaimsPrincipal user, string includes, IncludeTree tree = null)
        {
            var creator = new TDto();
            var dto = creator.CreateInstance(obj, user, includes, null, tree);
            
            return dto;
        }

        public static void DtoToObjMapper(TDto dto, T entity, ClaimsPrincipal user, string includes)
        {
            dto.Update(entity, user, includes);
        }
    }
}
