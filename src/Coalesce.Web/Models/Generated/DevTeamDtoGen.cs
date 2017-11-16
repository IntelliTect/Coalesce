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

using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>
        , IClassDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>
    {
        public DevTeamDtoGen() { }

        public Int32? DevTeamId { get; set; }
        public System.String Name { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static DevTeamDtoGen Create(Coalesce.Domain.External.DevTeam obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for DevTeam


            // Applicable excludes for DevTeam


            // Applicable roles for DevTeam



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out DevTeamDtoGen existing)) return existing;

            var newObject = new DevTeamDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.DevTeamId = obj.DevTeamId;
            newObject.Name = obj.Name;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public DevTeamDtoGen CreateInstance(Coalesce.Domain.External.DevTeam obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.External.DevTeam entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for DevTeam


            // Applicable excludes for DevTeam


            // Applicable roles for DevTeam


            entity.Name = Name;
        }

    }
}
