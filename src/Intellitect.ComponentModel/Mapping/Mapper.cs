using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using System.Security.Claims;
using Intellitect.ComponentModel.TypeDefinition;
using System.Collections.Concurrent;

namespace Intellitect.ComponentModel.Mapping
{
    public static class Mapper
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, UserMapper> _userMappers =
            new ConcurrentDictionary<string, UserMapper>();
        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> _users =
            new ConcurrentDictionary<string, string>();

        public static IMapper ObjToDtoMapper(ClaimsPrincipal user)
        {
            return GetUserMapper(user).ObjToDtoMapper;
        }

        public static IMapper DtoToObjMapper(ClaimsPrincipal user)
        {
            return GetUserMapper(user).DtoToObjMapper;
        }

        public static UserMapper GetUserMapper(ClaimsPrincipal user)
        {
            if (user != null)
            {
                string username = string.IsNullOrWhiteSpace(user.Identity.Name) ? "" : user.Identity.Name;
                var name = _users.GetOrAdd(username, username);
                lock (name)
                {
                    return _userMappers.GetOrAdd(username, f => new UserMapper(user));
                }
            }
            else
            {
                string username = "Unknown";
                var name = _users.GetOrAdd(username, username);
                lock (name)
                {
                    return _userMappers.GetOrAdd(username, f => new UserMapper(user));
                }
            }
        }

    }
}
