using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain;

/// <summary>
/// Entity with DateOnly primary key
/// </summary>
public class DateOnlyPk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DateOnly DateOnlyPkId { get; set; }

    public string Name { get; set; } = null!;
}
