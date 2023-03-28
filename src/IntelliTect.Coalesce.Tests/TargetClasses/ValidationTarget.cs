using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    public class ValidationTarget
    {
        public int Id { get; set; }

        [RegularExpression(@"^product:\d+:\d+$")]
        public string ProductId { get; set; }

        [EmailAddress(ErrorMessage = "Custom email error message")]
        [StringLength(150, MinimumLength = 3)]
        public string Email { get; set; }

        [Display(Name = "Fancy Number")]
        [Required]
        [Range(5, 10)]
        public int? Number { get; set; }

        public ValidationTargetChild OptionalChild { get; set; }

        [Required]
        public ValidationTargetChild RequiredChild { get; set; } = null!;

        // This class (ValidationTarget) is not mapped,
        // but the code under test here (parameter validation) doesn't really care.
        [Coalesce]
        public ItemResult MethodWithMixedParameters([Required,EmailAddress] string email, [Required] ValidationTarget target)
        {
            return true;
        }

//#if NET7_0_OR_GREATER
//        public required ValidationTargetChild RequiredKeywordChild { get; set; }
//#endif
    }

    public class ValidationTargetChild
    {
        [Url]
        public string String { get; set; }

        [Required]
        public string RequiredVal { get; set; }
    }

    public partial class ValidationTargetDtoGen : GeneratedDto<ValidationTarget>
    {
        public ValidationTargetDtoGen() { }

        private int? _Id;
        private string _ProductId;
        private string _Email;
        private int? _Number;
        private ValidationTargetChildDtoGen _OptionalChild;
        private ValidationTargetChildDtoGen _RequiredChild;

        public int? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string ProductId
        {
            get => _ProductId;
            set { _ProductId = value; Changed(nameof(ProductId)); }
        }
        public string Email
        {
            get => _Email;
            set { _Email = value; Changed(nameof(Email)); }
        }
        public int? Number
        {
            get => _Number;
            set { _Number = value; Changed(nameof(Number)); }
        }
        public ValidationTargetChildDtoGen OptionalChild
        {
            get => _OptionalChild;
            set { _OptionalChild = value; Changed(nameof(OptionalChild)); }
        }
        public ValidationTargetChildDtoGen RequiredChild
        {
            get => _RequiredChild;
            set { _RequiredChild = value; Changed(nameof(RequiredChild)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(ValidationTarget obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.ProductId = obj.ProductId;
            this.Email = obj.Email;
            this.Number = obj.Number;

            this.OptionalChild = obj.OptionalChild.MapToDto<ValidationTargetChild, ValidationTargetChildDtoGen>(context, tree?[nameof(this.OptionalChild)]);


            this.RequiredChild = obj.RequiredChild.MapToDto<ValidationTargetChild, ValidationTargetChildDtoGen>(context, tree?[nameof(this.RequiredChild)]);

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(ValidationTarget entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(ProductId))) entity.ProductId = ProductId;
            if (ShouldMapTo(nameof(Email))) entity.Email = Email;
            if (ShouldMapTo(nameof(Number))) entity.Number = Number;
            if (ShouldMapTo(nameof(OptionalChild))) entity.OptionalChild = OptionalChild?.MapToModelOrNew(entity.OptionalChild, context);
            if (ShouldMapTo(nameof(RequiredChild))) entity.RequiredChild = RequiredChild?.MapToModelOrNew(entity.RequiredChild, context);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override ValidationTarget MapToNew(IMappingContext context)
        {
            var entity = new ValidationTarget();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class ValidationTargetChildDtoGen : GeneratedDto<ValidationTargetChild>
    {
        public ValidationTargetChildDtoGen() { }

        private string _String;
        private string _RequiredVal;

        public string String
        {
            get => _String;
            set { _String = value; Changed(nameof(String)); }
        }
        public string RequiredVal
        {
            get => _RequiredVal;
            set { _RequiredVal = value; Changed(nameof(RequiredVal)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(ValidationTargetChild obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.RequiredVal = obj.RequiredVal;
            this.String = obj.String;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(ValidationTargetChild entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(RequiredVal))) entity.RequiredVal = (RequiredVal ?? entity.RequiredVal);
            if (ShouldMapTo(nameof(String))) entity.String = String;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override ValidationTargetChild MapToNew(IMappingContext context)
        {
            var entity = new ValidationTargetChild();
            MapTo(entity, context);
            return entity;
        }
    }
}
