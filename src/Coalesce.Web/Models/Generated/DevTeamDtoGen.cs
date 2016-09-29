    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using IntelliTect.Coalesce.Helpers.IncludeTree;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>
        , IClassDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>
        {
        public DevTeamDtoGen() { }

             public Int32? DevTeamId { get; set; }
             public String Name { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static DevTeamDtoGen Create(Coalesce.Domain.External.DevTeam obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null, IncludeTree tree = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (user != null)
			{
			}



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && objects.ContainsKey(obj)) 
                return (DevTeamDtoGen)objects[obj];

            var newObject = new DevTeamDtoGen();
            if (tree == null) objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.DevTeamId = obj.DevTeamId;
            newObject.Name = obj.Name;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public DevTeamDtoGen CreateInstance(Coalesce.Domain.External.DevTeam obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null) {
            return Create(obj, user, includes, objects, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.External.DevTeam entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (user != null)
			{
			}

			entity.Name = Name;
        }

	}
}
