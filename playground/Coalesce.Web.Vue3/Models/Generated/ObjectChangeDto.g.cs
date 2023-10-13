using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.Models
{
    public partial class ObjectChangeDtoGen : GeneratedDto<Coalesce.Domain.ObjectChange>
    {
        public ObjectChangeDtoGen() { }

        private string _Message;
        private int? _UserId;
        private Coalesce.Web.Vue3.Models.PersonDtoGen _User;
        private long? _Id;
        private string _Type;
        private string _KeyValue;
        private IntelliTect.Coalesce.AuditLogging.AuditEntryState? _State;
        private System.DateTimeOffset? _Date;
        private System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.ObjectChangePropertyDtoGen> _Properties;

        public string Message
        {
            get => _Message;
            set { _Message = value; Changed(nameof(Message)); }
        }
        public int? UserId
        {
            get => _UserId;
            set { _UserId = value; Changed(nameof(UserId)); }
        }
        public Coalesce.Web.Vue3.Models.PersonDtoGen User
        {
            get => _User;
            set { _User = value; Changed(nameof(User)); }
        }
        public long? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
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
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.ObjectChangePropertyDtoGen> Properties
        {
            get => _Properties;
            set { _Properties = value; Changed(nameof(Properties)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.ObjectChange obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Message = obj.Message;
            this.UserId = obj.UserId;
            this.Id = obj.Id;
            this.Type = obj.Type;
            this.KeyValue = obj.KeyValue;
            this.State = obj.State;
            this.Date = obj.Date;
            if (tree == null || tree[nameof(this.User)] != null)
                this.User = obj.User.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.User)]);

            var propValProperties = obj.Properties;
            if (propValProperties != null && (tree == null || tree[nameof(this.Properties)] != null))
            {
                this.Properties = propValProperties
                    .OrderBy(f => f.Id)
                    .Select(f => f.MapToDto<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty, ObjectChangePropertyDtoGen>(context, tree?[nameof(this.Properties)])).ToList();
            }
            else if (propValProperties == null && tree?[nameof(this.Properties)] != null)
            {
                this.Properties = new ObjectChangePropertyDtoGen[0];
            }

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.ObjectChange entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Message))) entity.Message = Message;
            if (ShouldMapTo(nameof(UserId))) entity.UserId = UserId;
            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(Type))) entity.Type = Type;
            if (ShouldMapTo(nameof(KeyValue))) entity.KeyValue = KeyValue;
            if (ShouldMapTo(nameof(State))) entity.State = (State ?? entity.State);
            if (ShouldMapTo(nameof(Date))) entity.Date = (Date ?? entity.Date);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.ObjectChange MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.ObjectChange();
            MapTo(entity, context);
            return entity;
        }
    }
}
