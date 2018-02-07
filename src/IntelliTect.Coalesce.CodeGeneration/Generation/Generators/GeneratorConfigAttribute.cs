using System;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    /// <summary>
    /// Indicates that the targeted property on a generator inheriting from <see cref="Generator"/>
    /// should have its value retrieved from the "generatorConfig" section in the coalesce.json file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class GeneratorConfigAttribute : Attribute
    {

    }
}