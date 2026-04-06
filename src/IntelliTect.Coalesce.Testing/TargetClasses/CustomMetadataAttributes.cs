using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

[assembly: CoalesceMetadata<IntelliTect.Coalesce.Testing.TargetClasses.CustomMetadataTargetAttribute>]
[assembly: CoalesceMetadata<IntelliTect.Coalesce.Testing.TargetClasses.CustomMetadataMarkerAttribute>]

namespace IntelliTect.Coalesce.Testing.TargetClasses;

[AttributeUsage(AttributeTargets.All)]
public class CustomMetadataTargetAttribute : Attribute
{
    public string Name { get; }
    public int Value { get; set; }

    public CustomMetadataTargetAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.All)]
public class CustomMetadataMarkerAttribute : Attribute { }
