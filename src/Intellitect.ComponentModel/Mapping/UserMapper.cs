using AutoMapper;
using AutoMapper.Mappers;
using Intellitect.ComponentModel.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Mapping
{
    public class UserMapper
    {
        public readonly List<Profile> ObjToDtoProfiles = new List<Profile>();
        public readonly List<Profile> DtoToObjProfiles = new List<Profile>();
        public readonly IMapper ObjToDtoMapper;
        public readonly IMapper DtoToObjMapper;
        internal readonly MapperConfiguration  ObjToDtoConfiguration;
        internal readonly MapperConfiguration DtoToObjConfiguration;

        internal UserMapper(ClaimsPrincipal user)
        {
            //Debug.WriteLine($"Adding mapping for {user.Identity.Name}");
            // Create profiles
            foreach (var objVm in ReflectionRepository.Models)
            {
                Debug.WriteLine($"   mapping for {objVm.Name}");
                ObjToDtoProfiles.Add(new ObjToDtoProfile(objVm, user));
                DtoToObjProfiles.Add(new DtoToObjProfile(objVm, user));
            }

            // Create the Configurations
            ObjToDtoConfiguration = new MapperConfiguration(cfg => { ObjToDtoProfiles.ForEach(profile => cfg.AddProfile(profile)); });
            DtoToObjConfiguration = new MapperConfiguration(cfg => { DtoToObjProfiles.ForEach(profile => cfg.AddProfile(profile)); });

            // Create the mappers.
            ObjToDtoMapper = ObjToDtoConfiguration.CreateMapper();
            DtoToObjMapper = DtoToObjConfiguration.CreateMapper();

            ObjToDtoConfiguration.AssertConfigurationIsValid();
            DtoToObjConfiguration.AssertConfigurationIsValid();
            //Debug.WriteLine($"Done Adding mapping for {user.Identity.Name}");
        }

    }
}
