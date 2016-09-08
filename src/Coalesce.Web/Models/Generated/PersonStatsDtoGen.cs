    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using IntelliTect.Coalesce.Helpers;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

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
        public static PersonStatsDtoGen Create(Coalesce.Domain.PersonStats obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null, IncludeTree tree = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for PersonStats
            

            // Applicable excludes for PersonStats
            

            // Applicable roles for PersonStats
            if (user != null)
			{
			}



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && objects.ContainsKey(obj)) 
                return (PersonStatsDtoGen)objects[obj];

            var newObject = new PersonStatsDtoGen();
            if (tree == null) objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.PersonStatsId = obj.PersonStatsId;
            newObject.Height = obj.Height;
            newObject.Weight = obj.Weight;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public PersonStatsDtoGen CreateInstance(Coalesce.Domain.PersonStats obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null) {
            return Create(obj, user, includes, objects, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.PersonStats entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for PersonStats
            

            // Applicable excludes for PersonStats
            

            // Applicable roles for PersonStats
            if (user != null)
			{
			}

			entity.Height = (Double)(Height ?? 0);
			entity.Weight = (Double)(Weight ?? 0);
        }

	}
}
