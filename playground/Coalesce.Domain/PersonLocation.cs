using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Coalesce.Domain;

/// <summary>
/// Separate-key one-to-one
/// </summary>
[Index(nameof(PersonId), IsUnique = true)]
public class PersonLocation
{
    public int PersonLocationId { get; set; }

    public int? PersonId { get; set; }
    public Person? Person { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
