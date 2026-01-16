using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class UserPasskeyInfoParameter : SparseDto, IGeneratedParameterDto<Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo>
    {
        public UserPasskeyInfoParameter() { }

        private byte[] _CredentialId;
        private string _Name;
        private System.DateTimeOffset? _CreatedOn;

        public byte[] CredentialId
        {
            get => _CredentialId;
            set { _CredentialId = value; Changed(nameof(CredentialId)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public System.DateTimeOffset? CreatedOn
        {
            get => _CreatedOn;
            set { _CreatedOn = value; Changed(nameof(CreatedOn)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(CredentialId))) entity.CredentialId = CredentialId;
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(CreatedOn))) entity.CreatedOn = (CreatedOn ?? entity.CreatedOn);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo MapToNew(IMappingContext context)
        {
            var includes = context.Includes;

            var entity = new Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo()
            {
                CredentialId = CredentialId,
            };
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(CreatedOn))) entity.CreatedOn = (CreatedOn ?? entity.CreatedOn);

            return entity;
        }

        public Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo MapToModelOrNew(Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class UserPasskeyInfoResponse : IGeneratedResponseDto<Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo>
    {
        public UserPasskeyInfoResponse() { }

        public byte[] CredentialId { get; set; }
        public string Name { get; set; }
        public System.DateTimeOffset? CreatedOn { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.CredentialId = obj.CredentialId;
            this.Name = obj.Name;
            this.CreatedOn = obj.CreatedOn;
        }
    }
}
