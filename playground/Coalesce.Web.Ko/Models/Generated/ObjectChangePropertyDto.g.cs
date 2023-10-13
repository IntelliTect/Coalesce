using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Ko.Models
{
    public partial class ObjectChangePropertyDtoGen : GeneratedDto<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty>
    {
        public ObjectChangePropertyDtoGen() { }

        private long? _Id;
        private long? _ParentId;
        private string _PropertyName;
        private string _OldValue;
        private string _NewValue;

        public long? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public long? ParentId
        {
            get => _ParentId;
            set { _ParentId = value; Changed(nameof(ParentId)); }
        }
        public string PropertyName
        {
            get => _PropertyName;
            set { _PropertyName = value; Changed(nameof(PropertyName)); }
        }
        public string OldValue
        {
            get => _OldValue;
            set { _OldValue = value; Changed(nameof(OldValue)); }
        }
        public string NewValue
        {
            get => _NewValue;
            set { _NewValue = value; Changed(nameof(NewValue)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.ParentId = obj.ParentId;
            this.PropertyName = obj.PropertyName;
            this.OldValue = obj.OldValue;
            this.NewValue = obj.NewValue;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(ParentId))) entity.ParentId = (ParentId ?? entity.ParentId);
            if (ShouldMapTo(nameof(PropertyName))) entity.PropertyName = PropertyName;
            if (ShouldMapTo(nameof(OldValue))) entity.OldValue = OldValue;
            if (ShouldMapTo(nameof(NewValue))) entity.NewValue = NewValue;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty MapToNew(IMappingContext context)
        {
            var entity = new IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty();
            MapTo(entity, context);
            return entity;
        }
    }
}
