using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
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
    }
}
