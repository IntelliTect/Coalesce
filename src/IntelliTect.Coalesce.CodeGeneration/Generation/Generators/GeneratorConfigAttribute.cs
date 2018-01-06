using System;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    /// <summary>
    /// Indicates that the targeted property on a generator inheriting from IntelliTect.Coalesce.CodeGeneration.Generation
    /// should have its value retrieved from the "generatorConfig" section in the coalesce.json file governing the current code gen execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class GeneratorConfigAttribute : Attribute
    {

    }
}