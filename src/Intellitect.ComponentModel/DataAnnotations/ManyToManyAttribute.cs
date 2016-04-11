using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Identifies a property as a many to many relationship and creates a secondary property on the view model for 
    /// viewing the collection of target items directly.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class ManyToManyAttribute : System.Attribute
    {
        public string CollectionName { get; }

        public ManyToManyAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}
