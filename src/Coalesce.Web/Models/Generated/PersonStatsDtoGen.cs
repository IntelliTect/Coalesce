using Coalesce.Domain;
using Coalesce.Domain.External;
using Coalesce.Web.Models;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

using static Coalesce.Domain.PersonStats;

namespace Coalesce.Web.Models
{
    public partial class PersonStatsDtoGen : GeneratedDto<Coalesce.Domain.PersonStats, PersonStatsDtoGen>
        , IClassDto<Coalesce.Domain.PersonStats, PersonStatsDtoGen>
    {
        public PersonStatsDtoGen() { }

        public Int32? PersonStatsId { get; set; }
        public Double? Height { get; set; }
        public Double? Weight { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static PersonStatsDtoGen Create(Coalesce.Domain.PersonStats obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for PersonStats


            // Applicable excludes for PersonStats


            // Applicable roles for PersonStats



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out PersonStatsDtoGen existing)) return existing;

            var newObject = new PersonStatsDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.PersonStatsId = obj.PersonStatsId;
            newObject.Height = obj.Height;
            newObject.Weight = obj.Weight;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public PersonStatsDtoGen CreateInstance(Coalesce.Domain.PersonStats obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.PersonStats entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for PersonStats


            // Applicable excludes for PersonStats


            // Applicable roles for PersonStats


            entity.Height = (Double)(Height ?? 0);
            entity.Weight = (Double)(Weight ?? 0);
        }

    }
}
