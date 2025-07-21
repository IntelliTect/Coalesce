using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

[Table("CaseProduct")]
public class CaseProduct
{
    public int CaseProductId { get; set; }
    public int CaseId { get; set; }

    [Search]
    [DefaultOrderBy(FieldOrder = 2)]
    public Case Case { get; set; }

    public int ProductId { get; set; }

    [Search]
    [DefaultOrderBy(FieldOrder = 1)]
    public Product Product { get; set; }
}
