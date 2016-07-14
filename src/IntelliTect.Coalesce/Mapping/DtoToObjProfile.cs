using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Mapping
{
    public class DtoToObjProfile: Profile
    {
        private ClassViewModel _sourceVm;
        private ClaimsPrincipal _user;
        public DtoToObjProfile(ClassViewModel sourceVm, ClaimsPrincipal user)
        {
            _sourceVm = sourceVm;
            _user = user;
        }

        protected override void Configure()
        {
            ClassViewModel destinationVm = _sourceVm;
            AutoMapper.IMappingExpression dtoToObjMap;

            // TODO: These should only be set up once.
            dtoToObjMap = CreateMap(destinationVm.Wrapper.Info, _sourceVm.Wrapper.Info);

            // Ignore read-only properties.
            foreach (var prop in _sourceVm.Properties.Where(f => f.IsReadOnly))
            {
                dtoToObjMap = dtoToObjMap.ForSourceMember(prop.Name, opt => opt.Ignore());
            }
            // Don't assign objects, only their IDs.
            foreach (var prop in _sourceVm.Properties.Where(f => f.IsPOCO && !f.IsComplexType))
            {
                dtoToObjMap = dtoToObjMap.ForMember(prop.Name, opt => opt.Ignore());
            }
            // Remove many to many relationships.
            foreach (var prop in _sourceVm.Properties.Where(f => f.IsManytoManyCollection))
            {
                dtoToObjMap = dtoToObjMap.ForMember(prop.Name, opt => opt.Ignore());
            }
            // Remove collections.
            foreach (var prop in _sourceVm.Properties.Where(f => f.Type.IsCollection))
            {
                dtoToObjMap = dtoToObjMap.ForMember(prop.Name, opt => opt.Ignore());
            }
            // Remove internal use
            foreach (var prop in _sourceVm.Properties.Where(f => f.IsInternalUse))
            {
                dtoToObjMap = dtoToObjMap.ForMember(prop.Name, opt => opt.Ignore());
                //objToDtoMap = objToDtoMap.ForMember(prop.Name, opt => opt.Ignore());
            }

            // Set up incoming security
            foreach (var prop in _sourceVm.Properties.Where(f => !f.SecurityInfo.IsEditable(_user)))
            {
                dtoToObjMap = dtoToObjMap.ForMember(prop.Name, opt => opt.Ignore());
            }

            // Explicitly map name.
            foreach (var prop in _sourceVm.Properties)
            {
                //objToDtoMap = objToDtoMap.(prop.Name);
            }
        }
    }
}
