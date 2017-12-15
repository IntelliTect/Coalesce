using System.Security.Claims;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Concurrent;

namespace IntelliTect.Coalesce.Mapping
{
    public static class Mapper
    {
        public static TDto MapToDto<T, TDto>(this T obj, IMappingContext context, IncludeTree tree = null)
             where TDto : IClassDto<T>, new()
        {
            if (obj == null) return default(TDto);

            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out TDto existing)) return existing;

            var dto = new TDto();
            if (tree == null) context.AddMapping(obj, dto);

            dto.MapFrom(obj, context, tree);

            return dto;
        }

        public static void MapToEntity<T, TDto>(this TDto dto, T entity, IMappingContext context)
             where TDto : IClassDto<T>, new()
        {
            dto.MapTo(entity, context);
        }
    }
}
