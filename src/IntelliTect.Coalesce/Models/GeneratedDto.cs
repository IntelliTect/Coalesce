using IntelliTect.Coalesce.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public abstract class GeneratedDto<T> : IClassDto<T>
    {
        public abstract void MapFrom(T obj, IMappingContext context, IncludeTree tree = null);
        public abstract void MapTo(T obj, IMappingContext context);

        private HashSet<string> _changedProperties = new HashSet<string>();
        protected void Changed(string propName) => _changedProperties.Add(propName);
        protected bool ShouldMapTo(string propName) => _changedProperties.Contains(propName);

        public virtual bool OnUpdate(T entity, IMappingContext context)
        {
            return false;
        }
    }
}
