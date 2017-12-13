
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class CompanyDtoGen : GeneratedDto<Coalesce.Domain.Company, CompanyDtoGen>
        , IClassDto<Coalesce.Domain.Company, CompanyDtoGen>
    {
        public CompanyDtoGen() { }

        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Models.PersonDtoGen> Employees { get; set; }
        public string AltName { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CompanyDtoGen Create(Coalesce.Domain.Company obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for Company


            // Applicable excludes for Company


            // Applicable roles for Company



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out CompanyDtoGen existing)) return existing;

            var newObject = new CompanyDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.CompanyId = obj.CompanyId;
            newObject.Name = obj.Name;
            newObject.Address1 = obj.Address1;
            newObject.Address2 = obj.Address2;
            newObject.City = obj.City;
            newObject.State = obj.State;
            newObject.ZipCode = obj.ZipCode;
            newObject.AltName = obj.AltName;
            var propValEmployees = obj.Employees;
            if (propValEmployees != null && (tree == null || tree[nameof(newObject.Employees)] != null))
            {
                newObject.Employees = propValEmployees.AsQueryable().OrderBy("PersonId ASC").ToList().Select(f => PersonDtoGen.Create(f, context, tree?[nameof(newObject.Employees)])).ToList();
            }
            else if (propValEmployees == null && tree?[nameof(newObject.Employees)] != null)
            {
                newObject.Employees = new PersonDtoGen[0];
            }

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CompanyDtoGen CreateInstance(Coalesce.Domain.Company obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.Company entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for Company


            // Applicable excludes for Company


            // Applicable roles for Company


            entity.Name = Name;
            entity.Address1 = Address1;
            entity.Address2 = Address2;
            entity.City = City;
            entity.State = State;
            entity.ZipCode = ZipCode;
        }

    }
}
