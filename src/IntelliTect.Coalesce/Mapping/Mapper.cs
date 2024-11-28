using System.Security.Claims;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Concurrent;

namespace IntelliTect.Coalesce.Mapping
{
    public static class Mapper
    {
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("obj")]
        public static TDto? MapToDto<T, TDto>(this T? obj, IMappingContext context, IncludeTree? tree = null)
            where T : class
            where TDto : class, IResponseDto<T>, new()
        {
            if (obj == null) return default;

            // See if we already mapped this object:
            if (context.TryGetMapping(obj, tree, out TDto? existing)) return existing;

            var dto = new TDto();
            context.AddMapping(obj, tree, dto);

            dto.MapFrom(obj, context, tree);

            return dto;
        }

        public static T MapToModel<T, TDto>(this TDto dto, T entity, IMappingContext context)
            where T : class
            where TDto : class, IParameterDto<T>, new()
        {
            dto.MapTo(entity, context);
            return entity;
        }
    }
}
