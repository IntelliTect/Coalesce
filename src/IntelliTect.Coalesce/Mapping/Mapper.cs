using System.Security.Claims;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Concurrent;

namespace IntelliTect.Coalesce.Mapping
{
    public static class Mapper<T, TDto> where TDto : IClassDto<T, TDto>, new()
    {
        public static TDto ObjToDtoMapper(T obj, IMappingContext context, IncludeTree tree = null)
        {
            var creator = new TDto();
            var dto = creator.CreateInstance(obj, context, tree);
            
            return dto;
        }

        public static void DtoToObjMapper(TDto dto, T entity, IMappingContext context)
        {
            dto.Update(entity, context);
        }
    }
}
