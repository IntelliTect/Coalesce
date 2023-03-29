using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public abstract class GeneratedDto<T> : IClassDto<T>, ISparseDto
        where T : class
    {
        public abstract void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null);
        public abstract void MapTo(T obj, IMappingContext context);
        public abstract T MapToNew(IMappingContext context);

        private readonly HashSet<string> _changedProperties = new HashSet<string>();
        [InternalUse]
        ISet<string> ISparseDto.ChangedProperties => _changedProperties;

        protected void Changed(string propName) => _changedProperties.Add(propName);
        protected bool ShouldMapTo(string propName) => _changedProperties.Contains(propName);

        public virtual bool OnUpdate(T entity, IMappingContext context)
        {
            return false;
        }

        public T MapToModelOrNew(T obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public interface ISparseDto
    {
        ISet<string> ChangedProperties { get; }
    }

}
