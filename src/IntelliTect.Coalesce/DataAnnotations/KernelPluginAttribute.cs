using System;

namespace IntelliTect.Coalesce
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class KernelPluginAttribute(string description) : Attribute
    {
        public string Description { get; set; } = description;
    }
}
