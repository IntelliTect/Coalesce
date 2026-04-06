using System;

namespace Coalesce.Domain;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method)]
public class FeatureFlagAttribute : Attribute
{
    public string Flag { get; }

    public FeatureFlagAttribute(string flag)
    {
        Flag = flag;
    }
}
