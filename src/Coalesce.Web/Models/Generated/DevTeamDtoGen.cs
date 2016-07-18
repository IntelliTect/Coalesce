
using System;
using System.Collections.Generic;
using System.Security.Claims;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Mapping;
using System.Linq;
using Newtonsoft.Json;
// Model Namespaces
    using Coalesce.Domain;
    using Coalesce.Domain.External;
using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<DevTeam, DevTeamDtoGen>
        , IClassDto<DevTeam, DevTeamDtoGen>
        {
        public DevTeamDtoGen() { }

             public Int32? DevTeamId { get; set; }
             public String Name { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static DevTeamDtoGen Create(DevTeam obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (user != null)
			{
			}



            // See if the object is already created.
            if (objects.ContainsKey(obj)) 
                return (DevTeamDtoGen)objects[obj];

            var newObject = new DevTeamDtoGen();
            objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.DevTeamId = obj.DevTeamId;
            newObject.Name = obj.Name;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public DevTeamDtoGen CreateInstance(DevTeam obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(DevTeam entity, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

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

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
        if (OnSecurityTrim(user, includes)) return;

        // Applicable includes for DevTeam
        

        // Applicable excludes for DevTeam
        

        // Applicable roles for DevTeam
        if (user != null)
			{
			}

        }
        }
        }
