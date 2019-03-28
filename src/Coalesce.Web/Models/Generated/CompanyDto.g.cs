using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class CompanyDtoGen : GeneratedDto<Coalesce.Domain.Company>
    {
        public CompanyDtoGen() { }

        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public bool? IsDeleted { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Models.PersonDtoGen> Employees { get; set; }
        public string AltName { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Company obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.CompanyId = obj.CompanyId;
            this.Name = obj.Name;
            this.Address1 = obj.Address1;
            this.Address2 = obj.Address2;
            this.City = obj.City;
            this.State = obj.State;
            this.ZipCode = obj.ZipCode;
            this.IsDeleted = obj.IsDeleted;
            this.AltName = obj.AltName;
            var propValEmployees = obj.Employees;
            if (propValEmployees != null && (tree == null || tree[nameof(this.Employees)] != null))
            {
                this.Employees = propValEmployees
                    .OrderBy(f => f.PersonId)
                    .Select(f => f.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.Employees)])).ToList();
            }
            else if (propValEmployees == null && tree?[nameof(this.Employees)] != null)
            {
                this.Employees = new PersonDtoGen[0];
            }

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Company entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.CompanyId = (CompanyId ?? entity.CompanyId);
            entity.Name = Name;
            entity.Address1 = Address1;
            entity.Address2 = Address2;
            entity.City = City;
            entity.State = State;
            entity.ZipCode = ZipCode;
            entity.IsDeleted = (IsDeleted ?? entity.IsDeleted);
        }
    }
}
