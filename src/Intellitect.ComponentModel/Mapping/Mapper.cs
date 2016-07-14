using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using System.Security.Claims;
using Intellitect.ComponentModel.TypeDefinition;
using System.Collections.Concurrent;
using Intellitect.ComponentModel.Interfaces;
using System.Reflection;

namespace Intellitect.ComponentModel.Mapping
{
    public static class Mapper<T, TDto> where TDto : IClassDto<T, TDto>, new()
    {
        private static MethodInfo _createMap = typeof(AutoMapper.Mapper)
                                    .GetMethods()
                                    .Single(m => m.Name == "CreateMap" &&
                                                    m.IsGenericMethod == true &&
                                                    m.GetParameters().Count() == 0);

        public static TDto ObjToDtoMapper(T obj, ClaimsPrincipal user, string includes)
        {
            var creator = new TDto();
            var dto = creator.CreateInstance(obj, user, includes);
            
            //TDto dto = AutoMapper.Mapper.Map<TDto>(obj);
            //dto.SecurityTrim(user, includes);
            return dto;
        }

        public static void DtoToObjMapper(TDto dto, T entity, ClaimsPrincipal user, string includes)
        {
            dto.Update(entity, user, includes);
        }

        public static void AddMap(Type entityType, Type dtoType)
        {
            MethodInfo createMapGeneric = _createMap.MakeGenericMethod(new Type[] { entityType, dtoType });
            createMapGeneric.Invoke(null, null);
        }
    }
}
