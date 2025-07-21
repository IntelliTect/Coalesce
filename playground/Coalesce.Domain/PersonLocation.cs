using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain;

[Table("PersonLocation")]
public class PersonLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
