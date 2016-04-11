using AutoMapper;
using Intellitect.ComponentModel.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Mapping
{
    public class ObjToDtoProfile: Profile
    {
        private ClassViewModel _sourceVm;
        private ClaimsPrincipal _user;
        public ObjToDtoProfile(ClassViewModel sourceVm, ClaimsPrincipal user)
        {
            _sourceVm = sourceVm;
            _user = user;
        }

        protected override void Configure()
        {
            ClassViewModel destinationVm = _sourceVm;
            AutoMapper.IMappingExpression objToDtoMap;

            // TODO: These should only be set up once.
            objToDtoMap = CreateMap(_sourceVm.Wrapper.Info, destinationVm.Wrapper.Info);

            // Set up outgoing security
            foreach (var prop in _sourceVm.Properties.Where(f => !f.SecurityInfo.IsReadable(_user)))
            {
                objToDtoMap = objToDtoMap.ForMember(prop.Name, opt => opt.Ignore());
            }
        }
    }
}
