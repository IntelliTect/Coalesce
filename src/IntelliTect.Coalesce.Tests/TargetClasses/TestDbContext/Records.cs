using IntelliTect.Coalesce.DataAnnotations;
using System;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

public record PositionalRecord(string String, int Num)
{
    public DateTime Date { get; init; }
}

public record InitRecordWithDefaultCtor
{
    public string String { get; init; }

    [Read("ReadRole"), Edit("EditRole")]
    public int Num { get; init; }

    [DtoIncludes("asdf"), DtoExcludes("qwert")]
    public PositionalRecord NestedRecord { get; init; }
}

