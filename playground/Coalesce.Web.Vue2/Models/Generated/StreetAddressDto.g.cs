using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class StreetAddressParameter : GeneratedParameterDto<Coalesce.Domain.StreetAddress>
    {
        public StreetAddressParameter() { }

        private string _Address;
        private string _City;
        private string _State;
        private string _PostalCode;

        public string Address
        {
            get => _Address;
            set { _Address = value; Changed(nameof(Address)); }
        }
        public string City
        {
            get => _City;
            set { _City = value; Changed(nameof(City)); }
        }
        public string State
        {
            get => _State;
            set { _State = value; Changed(nameof(State)); }
        }
        public string PostalCode
        {
            get => _PostalCode;
            set { _PostalCode = value; Changed(nameof(PostalCode)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.StreetAddress entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Address))) entity.Address = Address;
            if (ShouldMapTo(nameof(City))) entity.City = City;
            if (ShouldMapTo(nameof(State))) entity.State = State;
            if (ShouldMapTo(nameof(PostalCode))) entity.PostalCode = PostalCode;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.StreetAddress MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.StreetAddress();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class StreetAddressResponse : GeneratedResponseDto<Coalesce.Domain.StreetAddress>
    {
        public StreetAddressResponse() { }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.StreetAddress obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Address = obj.Address;
            this.City = obj.City;
            this.State = obj.State;
            this.PostalCode = obj.PostalCode;
        }
    }
}
