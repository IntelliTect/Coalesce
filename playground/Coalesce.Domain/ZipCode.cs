using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain;

public class ZipCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [ListText]
    public string Zip { get; set; } = null!;

    public string State { get; set; } = null!;
}
