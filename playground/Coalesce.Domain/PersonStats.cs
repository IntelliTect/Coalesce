using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain;

/// <summary>
/// Shared-key one-to-one
/// </summary>
public class PersonStats
{
    [Key, ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public double Height { get; set; }
    public double Weight { get; set; }
}
