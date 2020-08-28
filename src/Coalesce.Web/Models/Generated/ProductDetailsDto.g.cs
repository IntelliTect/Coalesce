using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class ProductDetailsDtoGen : GeneratedDto<Coalesce.Domain.ProductDetails>
    {
        public ProductDetailsDtoGen() { }

        private Coalesce.Web.Models.StreetAddressDtoGen _ManufacturingAddress;
        private Coalesce.Web.Models.StreetAddressDtoGen _CompanyHqAddress;

        public Coalesce.Web.Models.StreetAddressDtoGen ManufacturingAddress
        {
            get => _ManufacturingAddress;
            set { _ManufacturingAddress = value; Changed(nameof(ManufacturingAddress)); }
        }
        public Coalesce.Web.Models.StreetAddressDtoGen CompanyHqAddress
        {
            get => _CompanyHqAddress;
            set { _CompanyHqAddress = value; Changed(nameof(CompanyHqAddress)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.ProductDetails obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.


            this.ManufacturingAddress = obj.ManufacturingAddress.MapToDto<Coalesce.Domain.StreetAddress, StreetAddressDtoGen>(context, tree?[nameof(this.ManufacturingAddress)]);


            this.CompanyHqAddress = obj.CompanyHqAddress.MapToDto<Coalesce.Domain.StreetAddress, StreetAddressDtoGen>(context, tree?[nameof(this.CompanyHqAddress)]);

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.ProductDetails entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(ManufacturingAddress))) entity.ManufacturingAddress = ManufacturingAddress?.MapToModel<Coalesce.Domain.StreetAddress, StreetAddressDtoGen>(entity.ManufacturingAddress ?? new Coalesce.Domain.StreetAddress(), context);
            if (ShouldMapTo(nameof(CompanyHqAddress))) entity.CompanyHqAddress = CompanyHqAddress?.MapToModel<Coalesce.Domain.StreetAddress, StreetAddressDtoGen>(entity.CompanyHqAddress ?? new Coalesce.Domain.StreetAddress(), context);
        }
    }
}
