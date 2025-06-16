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
    public partial class AuditLogPropertyParameter : SparseDto, IGeneratedParameterDto<IntelliTect.Coalesce.AuditLogging.AuditLogProperty>
    {
        public AuditLogPropertyParameter() { }

        private long? _Id;
        private long? _ParentId;
        private string _PropertyName;
        private string _OldValue;
        private string _OldValueDescription;
        private string _NewValue;
        private string _NewValueDescription;

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
        public string OldValueDescription
        {
            get => _OldValueDescription;
            set { _OldValueDescription = value; Changed(nameof(OldValueDescription)); }
        }
        public string NewValue
        {
            get => _NewValue;
            set { _NewValue = value; Changed(nameof(NewValue)); }
        }
        public string NewValueDescription
        {
            get => _NewValueDescription;
            set { _NewValueDescription = value; Changed(nameof(NewValueDescription)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(IntelliTect.Coalesce.AuditLogging.AuditLogProperty entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(ParentId))) entity.ParentId = (ParentId ?? entity.ParentId);
            if (ShouldMapTo(nameof(PropertyName))) entity.PropertyName = PropertyName;
            if (ShouldMapTo(nameof(OldValue))) entity.OldValue = OldValue;
            if (ShouldMapTo(nameof(OldValueDescription))) entity.OldValueDescription = OldValueDescription;
            if (ShouldMapTo(nameof(NewValue))) entity.NewValue = NewValue;
            if (ShouldMapTo(nameof(NewValueDescription))) entity.NewValueDescription = NewValueDescription;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public IntelliTect.Coalesce.AuditLogging.AuditLogProperty MapToNew(IMappingContext context)
        {
            var includes = context.Includes;

            var entity = new IntelliTect.Coalesce.AuditLogging.AuditLogProperty()
            {
                PropertyName = PropertyName,
            };
            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(ParentId))) entity.ParentId = (ParentId ?? entity.ParentId);
            if (ShouldMapTo(nameof(OldValue))) entity.OldValue = OldValue;
            if (ShouldMapTo(nameof(OldValueDescription))) entity.OldValueDescription = OldValueDescription;
            if (ShouldMapTo(nameof(NewValue))) entity.NewValue = NewValue;
            if (ShouldMapTo(nameof(NewValueDescription))) entity.NewValueDescription = NewValueDescription;

            return entity;
        }

        public IntelliTect.Coalesce.AuditLogging.AuditLogProperty MapToModelOrNew(IntelliTect.Coalesce.AuditLogging.AuditLogProperty obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class AuditLogPropertyResponse : IGeneratedResponseDto<IntelliTect.Coalesce.AuditLogging.AuditLogProperty>
    {
        public AuditLogPropertyResponse() { }

        public long? Id { get; set; }
        public long? ParentId { get; set; }
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string OldValueDescription { get; set; }
        public string NewValue { get; set; }
        public string NewValueDescription { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(IntelliTect.Coalesce.AuditLogging.AuditLogProperty obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.ParentId = obj.ParentId;
            this.PropertyName = obj.PropertyName;
            this.OldValue = obj.OldValue;
            this.OldValueDescription = obj.OldValueDescription;
            this.NewValue = obj.NewValue;
            this.NewValueDescription = obj.NewValueDescription;
        }
    }
}
