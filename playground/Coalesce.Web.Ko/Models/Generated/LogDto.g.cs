using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Ko.Models
{
    public partial class LogDtoGen : GeneratedDto<Coalesce.Domain.Log>
    {
        public LogDtoGen() { }

        private int? _LogId;
        private string _Level;
        private string _Message;

        public int? LogId
        {
            get => _LogId;
            set { _LogId = value; Changed(nameof(LogId)); }
        }
        public string Level
        {
            get => _Level;
            set { _Level = value; Changed(nameof(Level)); }
        }
        public string Message
        {
            get => _Message;
            set { _Message = value; Changed(nameof(Message)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Log obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.LogId = obj.LogId;
            this.Level = obj.Level;
            this.Message = obj.Message;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Log entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(LogId))) entity.LogId = (LogId ?? entity.LogId);
            if (ShouldMapTo(nameof(Level))) entity.Level = Level;
            if (ShouldMapTo(nameof(Message))) entity.Message = Message;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.Log MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.Log();
            MapTo(entity, context);
            return entity;
        }
    }
}
