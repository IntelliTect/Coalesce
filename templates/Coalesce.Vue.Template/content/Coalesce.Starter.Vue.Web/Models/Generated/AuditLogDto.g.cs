using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class AuditLogParameter : GeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.AuditLog>
    {
        public AuditLogParameter() { }

        private long? _Id;
        private string _UserId;
        private string _Type;
        private string _KeyValue;
        private string _Description;
        private IntelliTect.Coalesce.AuditLogging.AuditEntryState? _State;
        private System.DateTimeOffset? _Date;
        private string _ClientIp;
        private string _Referrer;
        private string _Endpoint;

        public long? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string UserId
        {
            get => _UserId;
            set { _UserId = value; Changed(nameof(UserId)); }
        }
        public string Type
        {
            get => _Type;
            set { _Type = value; Changed(nameof(Type)); }
        }
        public string KeyValue
        {
            get => _KeyValue;
            set { _KeyValue = value; Changed(nameof(KeyValue)); }
        }
        public string Description
        {
            get => _Description;
            set { _Description = value; Changed(nameof(Description)); }
        }
        public IntelliTect.Coalesce.AuditLogging.AuditEntryState? State
        {
            get => _State;
            set { _State = value; Changed(nameof(State)); }
        }
        public System.DateTimeOffset? Date
        {
            get => _Date;
            set { _Date = value; Changed(nameof(Date)); }
        }
        public string ClientIp
        {
            get => _ClientIp;
            set { _ClientIp = value; Changed(nameof(ClientIp)); }
        }
        public string Referrer
        {
            get => _Referrer;
            set { _Referrer = value; Changed(nameof(Referrer)); }
        }
        public string Endpoint
        {
            get => _Endpoint;
            set { _Endpoint = value; Changed(nameof(Endpoint)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Starter.Vue.Data.Models.AuditLog entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(UserId))) entity.UserId = UserId;
            if (ShouldMapTo(nameof(Type))) entity.Type = Type;
            if (ShouldMapTo(nameof(KeyValue))) entity.KeyValue = KeyValue;
            if (ShouldMapTo(nameof(Description))) entity.Description = Description;
            if (ShouldMapTo(nameof(State))) entity.State = (State ?? entity.State);
            if (ShouldMapTo(nameof(Date))) entity.Date = (Date ?? entity.Date);
            if (ShouldMapTo(nameof(ClientIp))) entity.ClientIp = ClientIp;
            if (ShouldMapTo(nameof(Referrer))) entity.Referrer = Referrer;
            if (ShouldMapTo(nameof(Endpoint))) entity.Endpoint = Endpoint;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Starter.Vue.Data.Models.AuditLog MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Starter.Vue.Data.Models.AuditLog();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class AuditLogResponse : GeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.AuditLog>
    {
        public AuditLogResponse() { }

        public long? Id { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string KeyValue { get; set; }
        public string Description { get; set; }
        public IntelliTect.Coalesce.AuditLogging.AuditEntryState? State { get; set; }
        public System.DateTimeOffset? Date { get; set; }
        public string ClientIp { get; set; }
        public string Referrer { get; set; }
        public string Endpoint { get; set; }
        public Coalesce.Starter.Vue.Web.Models.UserResponse User { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Web.Models.AuditLogPropertyResponse> Properties { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Starter.Vue.Data.Models.AuditLog obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.UserId = obj.UserId;
            this.Type = obj.Type;
            this.KeyValue = obj.KeyValue;
            this.Description = obj.Description;
            this.State = obj.State;
            this.Date = obj.Date;
            this.ClientIp = obj.ClientIp;
            this.Referrer = obj.Referrer;
            this.Endpoint = obj.Endpoint;
            if (tree == null || tree[nameof(this.User)] != null)
                this.User = obj.User.MapToDto<Coalesce.Starter.Vue.Data.Models.User, UserResponse>(context, tree?[nameof(this.User)]);

            var propValProperties = obj.Properties;
            if (propValProperties != null && (tree == null || tree[nameof(this.Properties)] != null))
            {
                this.Properties = propValProperties
                    .OrderBy(f => f.Id)
                    .Select(f => f.MapToDto<IntelliTect.Coalesce.AuditLogging.AuditLogProperty, AuditLogPropertyResponse>(context, tree?[nameof(this.Properties)])).ToList();
            }
            else if (propValProperties == null && tree?[nameof(this.Properties)] != null)
            {
                this.Properties = new AuditLogPropertyResponse[0];
            }

        }
    }
}
