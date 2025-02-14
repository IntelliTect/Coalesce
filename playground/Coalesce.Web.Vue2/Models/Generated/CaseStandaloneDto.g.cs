using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class CaseStandaloneParameter : GeneratedParameterDto<Coalesce.Domain.CaseStandalone>
    {
        public CaseStandaloneParameter() { }



        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.CaseStandalone entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.CaseStandalone MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.CaseStandalone();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class CaseStandaloneResponse : GeneratedResponseDto<Coalesce.Domain.CaseStandalone>
    {
        public CaseStandaloneResponse() { }

        public int? Id { get; set; }
        public Coalesce.Web.Vue2.Models.PersonResponse AssignedTo { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.CaseStandalone obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            if (tree == null || tree[nameof(this.AssignedTo)] != null)
                this.AssignedTo = obj.AssignedTo.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.AssignedTo)]);

        }
    }
}
