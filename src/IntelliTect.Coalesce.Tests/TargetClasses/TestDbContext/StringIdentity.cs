using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class StringIdentity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string StringIdentityId { get; set; } = null!;
    }


#nullable disable

    public partial class StringIdentityDtoGen : GeneratedDto<StringIdentity>
    {
        public StringIdentityDtoGen() { }

        private string _StringIdentityId;

        public string StringIdentityId
        {
            get => _StringIdentityId;
            set { _StringIdentityId = value; Changed(nameof(StringIdentityId)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(StringIdentity obj, IMappingContext context, IncludeTree tree = null)
            => throw new NotImplementedException(
                "This 'generated dto' is actually hand-written for these tests. Mapping methods are unused.");

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(StringIdentity entity, IMappingContext context)
            => throw new NotImplementedException(
                "This 'generated dto' is actually hand-written for these tests. Mapping methods are unused.");

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override StringIdentity MapToNew(IMappingContext context)
        {
            var entity = new StringIdentity();
            MapTo(entity, context);
            return entity;
        }
    }
}
