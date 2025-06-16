using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class TenantParameter : SparseDto, IGeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.Tenant>
    {
        public TenantParameter() { }

        private string _TenantId;
        private string _Name;

        public string TenantId
        {
            get => _TenantId;
            set { _TenantId = value; Changed(nameof(TenantId)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Starter.Vue.Data.Models.Tenant entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(TenantId))) entity.TenantId = TenantId;
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Starter.Vue.Data.Models.Tenant MapToNew(IMappingContext context)
        {
            var includes = context.Includes;

            var entity = new Coalesce.Starter.Vue.Data.Models.Tenant()
            {
                Name = Name,
            };
            if (ShouldMapTo(nameof(TenantId))) entity.TenantId = TenantId;

            return entity;
        }

        public Coalesce.Starter.Vue.Data.Models.Tenant MapToModelOrNew(Coalesce.Starter.Vue.Data.Models.Tenant obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class TenantResponse : IGeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.Tenant>
    {
        public TenantResponse() { }

        public string TenantId { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Starter.Vue.Data.Models.Tenant obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.TenantId = obj.TenantId;
            this.Name = obj.Name;
            this.ExternalId = obj.ExternalId;
        }
    }
}
