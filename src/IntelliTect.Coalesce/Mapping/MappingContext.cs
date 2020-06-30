using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IntelliTect.Coalesce.Mapping
{
    public class MappingContext : IMappingContext
    {
        public ClaimsPrincipal? User { get; }

        public string? Includes { get; }

        public Dictionary<object, object> MappedObjects { get; }
            = new Dictionary<object, object>();

        private readonly Dictionary<string, bool> _roleCache = new Dictionary<string, bool>();

        public MappingContext(ClaimsPrincipal? user = null, string? includes = null)
        {
            User = user;
            Includes = includes;
        }

        public MappingContext(ClaimsPrincipal? user, IDataSourceParameters parameters)
            : this(user, parameters.Includes) { }

        public bool IsInRoleCached(string role)
        {
            if (_roleCache.TryGetValue(role, out bool inRole)) return inRole;

            return _roleCache[role] = User?.IsInRole(role) ?? false;
        }

        public void AddMapping(object sourceObject, object mappedObject)
        {
            MappedObjects[sourceObject] = mappedObject;
        }

        public bool TryGetMapping<TDto>(
            object sourceObject,
    #if NETCOREAPP3_1
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
    #endif
            out TDto? mappedObject
        )
            where TDto : class
        {
            if (!MappedObjects.TryGetValue(sourceObject, out object? existingMapped) || !(existingMapped is TDto))
            {
                mappedObject = default;
                return false; 
            }
            mappedObject = (TDto)existingMapped;
            return true;
        }
    }
}
