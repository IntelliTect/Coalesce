using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

/// <summary>
/// Entity with DateTime primary key
/// </summary>
public class DateTimePk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DateTime DateTimePkId { get; set; }

    public string Name { get; set; } = null!;
}

/// <summary>
/// Entity with DateTimeOffset primary key
/// </summary>
public class DateTimeOffsetPk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DateTimeOffset DateTimeOffsetPkId { get; set; }

    public string Name { get; set; } = null!;
}

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

/// <summary>
/// Entity with TimeOnly primary key
/// </summary>
public class TimeOnlyPk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public TimeOnly TimeOnlyPkId { get; set; }

    public string Name { get; set; } = null!;
}