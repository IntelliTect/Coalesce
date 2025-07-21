using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain;

[NotMapped]
public class PersonStats
{
#nullable disable
    //public int PersonStatsId { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public string Name { get; set; }

    [NotMapped]
    public ICollection<DateTimeOffset?> NullableValueTypeCollection { get; set; }

    public ICollection<DateTimeOffset> ValueTypeCollection { get; set; }

     public PersonLocation PersonLocation { get; set; }
}
