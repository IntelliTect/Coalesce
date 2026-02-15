using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Testing.TargetClasses.Github31;

// https://github.com/IntelliTect/Coalesce/issues/31
public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }

    public DateTimeOffset? BirthDate { get; set; }

    [Coalesce]
    public static ICollection<Person> GetMyPeeps()
    {
        return null;
    }
}
