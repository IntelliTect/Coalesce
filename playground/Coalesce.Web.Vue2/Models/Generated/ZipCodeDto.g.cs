using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class ZipCodeDtoGen : GeneratedDto<Coalesce.Domain.ZipCode>
    {
        public ZipCodeDtoGen() { }

        private string _Zip;
        private string _State;

        public string Zip
        {
            get => _Zip;
            set { _Zip = value; Changed(nameof(Zip)); }
        }
        public string State
        {
            get => _State;
            set { _State = value; Changed(nameof(State)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.ZipCode obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Zip = obj.Zip;
            this.State = obj.State;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.ZipCode entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Zip))) entity.Zip = Zip;
            if (ShouldMapTo(nameof(State))) entity.State = State;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.ZipCode MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.ZipCode();
            MapTo(entity, context);
            return entity;
        }
    }
}
