using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

public class ZipCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [ListText]
    public string Zip { get; set; } = null!;

    public string State { get; set; } = null!;
}
