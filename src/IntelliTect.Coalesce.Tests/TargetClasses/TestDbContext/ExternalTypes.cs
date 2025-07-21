using System;
using System.Collections.Generic;

#nullable enable annotations
#nullable disable warnings

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

public class ExternalParent
{
    public int[] ValueArray { get; set; }
    public int?[] ValueNullableArray { get; set; }
    public int[]? ValueArrayNullable { get; set; }

    public ICollection<int> ValueICollection { get; set; }
    public ICollection<int?> ValueNullableICollection { get; set; }
    public ICollection<int>? ValueICollectionNullable { get; set; }
    public List<int> ValueList { get; set; }

    public ICollection<string> StringICollection { get; set; }
    public List<string> StringList { get; set; }

    public ExternalChild[] RefArray { get; set; }
    public ExternalChild?[] RefNullableArray { get; set; }
    public ExternalChild[]? RefArrayNullable { get; set; }

    public ICollection<ExternalChild> RefICollection { get; set; }
    public ICollection<ExternalChild?> RefNullableICollection { get; set; }
    public ICollection<ExternalChild>? RefICollectionNullable { get; set; }

    public List<ExternalChild> RefList { get; set; }
    public List<ExternalChild?> RefNullableList { get; set; }
    public List<ExternalChild>? RefListNullable { get; set; }
}

public class ExternalChild
{
    public string Value { get; set; }
}

public class ExternalParentAsInputOnly
{
    public ExternalChildAsInputOnly Child { get; set; }
}
public class ExternalChildAsInputOnly
{
    public string Value { get; set; }
    public ExternalParentAsInputOnly Recursive { get; set; }
}

public class ExternalParentAsOutputOnly
{
    public ExternalChildAsOutputOnly Child { get; set; }
}
public class ExternalChildAsOutputOnly 
{ 
    public string Value { get; set; }
    public ExternalParentAsOutputOnly Recursive { get; set; }
}

/// <summary>
/// This type can codegen its dto successfully because despite having no valid constructors that Coalesce can use, it also has zero input-mappable properties in the first place.
/// </summary>
public class OutputOnlyExternalTypeWithoutDefaultCtor
{
    public OutputOnlyExternalTypeWithoutDefaultCtor(Exception ex)
    {
        Bar = ex.Message;
        Baz = ex.Message;
    }

    public string Bar { get; }

    public string Baz { get; internal set; }
}

/// <summary>
/// This type can codegen its dto successfully because despite having no valid constructors that Coalesce can used, it is also never used in an input position so Coalesce doesn't require the ctor to be generated.
/// </summary>
public class OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties
{
    public OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties(Exception ex)
    {
        Message = ex.Message;
    }

    public string Message { get; set; }
}


public class OutputOnlyExternalTypeWithRequiredEntityProp
{
    public int Id { get; set; }

#if NET7_0_OR_GREATER
    // Entity props aren't accepted as inputs as children of external types,
    // so this creates quite the conundrum indeed.
    // Expected output here is that we generate a `throw` in MapToNew, since the scenario is just impossible.
    public required ComplexModel Entity { get; set; }
#endif
}


public class InputOutputOnlyExternalTypeWithRequiredNonscalarProp
{
    public int Id { get; set; }

#if NET7_0_OR_GREATER
    // Entity props aren't accepted as inputs as children of external types,
    // so this creates quite the conundrum indeed.
    // Expected output here is that we generate a `throw` in MapToNew, since the scenario is just impossible.
    public required ExternalChild ExternalChild { get; set; }
#endif
}
