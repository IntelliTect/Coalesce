using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models
{
    [JsonDerivedType(typeof(AbstractClassImplParameter), typeDiscriminator: "AbstractClassImpl")]
    public partial class AbstractClassParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.AbstractClass>
    {
        public AbstractClassParameter() { }

        private int? _Id;
        private string _AbstractClassString;

        public int? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string AbstractClassString
        {
            get => _AbstractClassString;
            set { _AbstractClassString = value; Changed(nameof(AbstractClassString)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.AbstractClass entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(AbstractClassString))) entity.AbstractClassString = AbstractClassString;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.AbstractClass MapToNew(IMappingContext context)
        {
            // Unacceptable constructors:
            // Type has no public constructors.
            throw new NotSupportedException("Type AbstractClass does not have a constructor suitable for use by Coalesce for new object instantiation. Fortunately, this type appears to never be used in an input position in a Coalesce-generated API.");
        }

        public Coalesce.Domain.AbstractClass MapToModelOrNew(Coalesce.Domain.AbstractClass obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    [JsonDerivedType(typeof(AbstractClassImplResponse), typeDiscriminator: "AbstractClassImpl")]
    public partial class AbstractClassResponse : IGeneratedResponseDto<Coalesce.Domain.AbstractClass>
    {
        public AbstractClassResponse() { }

        public int? Id { get; set; }
        public string AbstractClassString { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.AbstractClassPersonResponse> AbstractModelPeople { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.AbstractClass obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.AbstractClassString = obj.AbstractClassString;
            var propValAbstractModelPeople = obj.AbstractModelPeople;
            if (propValAbstractModelPeople != null && (tree == null || tree[nameof(this.AbstractModelPeople)] != null))
            {
                this.AbstractModelPeople = propValAbstractModelPeople
                    .OrderBy(f => f.Id)
                    .Select(f => f.MapToDto<Coalesce.Domain.AbstractClassPerson, AbstractClassPersonResponse>(context, tree?[nameof(this.AbstractModelPeople)])).ToList();
            }
            else if (propValAbstractModelPeople == null && tree?[nameof(this.AbstractModelPeople)] != null)
            {
                this.AbstractModelPeople = new AbstractClassPersonResponse[0];
            }

        }
    }
}
