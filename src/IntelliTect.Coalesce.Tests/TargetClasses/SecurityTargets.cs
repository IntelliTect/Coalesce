using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    public class DbSetIsInternalUse
    {
        public int Id { get; set; }
    }

    [InternalUse]
    public class TypeWithInternalUse
    {
        public int Id { get; set; }
    }

    [Edit(Roles = " RoleA , RoleB,RoleC")]
    public class EditWithSpaces { }

    public class PropSec
    {
        [Read]
        public string ReadOnlyViaRead { get; set; }

        [ReadOnly(true)]
        public string ReadOnlyViaReadOnly { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        [ReadOnlyApi]
#pragma warning restore CS0618 // Type or member is obsolete
        public string ReadOnlyViaReadOnlyApi { get; set; }

        public string ReadOnlyViaNonPublicSetter { get; internal set; }

        [Read("ReadRole"), Edit("EditRole")]
        public string ReadWriteDifferentRoles { get; set; }

        [Restrict<AuthenticatedRestriction>]
        public string RestrictFilter { get; set; }

        [Restrict<AuthenticatedRestriction>]
        [Restrict<Restrict2>]
        public string RestrictMultiple { get; set; }

        public DbSetIsInternalUse ExternalTypeUsageOfEntityWithInternalDbSet { get; set; }
    }

    public class AuthenticatedRestriction : IPropertyRestriction
    {
        public bool UserCanFilter(IMappingContext context, string propertyName)
            => context.User.Identity?.IsAuthenticated == true;

        public bool UserCanRead(IMappingContext context, string propertyName, object model)
            => context.User.Identity?.IsAuthenticated == true;

        public bool UserCanWrite(IMappingContext context, string propertyName, object model, object incomingValue)
            => context.User.Identity?.IsAuthenticated == true;
    }

    public class Restrict2 : IPropertyRestriction
    {
        public bool UserCanRead(IMappingContext context, string propertyName, object model) => true;

        public bool UserCanWrite(IMappingContext context, string propertyName, object model, object incomingValue) => true;
    }
}
