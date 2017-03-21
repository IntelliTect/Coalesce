using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{

    /// <summary>
    /// This class is designed to provide a wrapper for information about the Context inself
    /// that will work with Reflection or maybe (now) in the future Symbols. 
    /// </summary>
    public class ContextInfo
    {
        public ContextInfo(Type type, string nameSpace)
        {
            Namespace = nameSpace;
            Name = type.Name;
        }

        public string Namespace { get; }

        public string Name { get; }
    }
}
