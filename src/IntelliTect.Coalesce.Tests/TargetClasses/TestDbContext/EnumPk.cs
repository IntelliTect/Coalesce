using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

public enum EnumPkId
{
    Value0 = 0,
    Value1 = 1,
    Value10 = 10,
}

public class EnumPk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public EnumPkId EnumPkId { get; set; }

    public string Name { get; set; }
}
