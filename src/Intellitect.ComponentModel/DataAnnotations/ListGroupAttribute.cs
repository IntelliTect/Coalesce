using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Used to specify when two fields contain data that should be combined for string select lists.
    /// Field entries with the same group name will be combined into a select drop down list.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ListGroupAttribute : System.Attribute
    {
        public string Group { get; set; }

        public ListGroupAttribute(string group)
        {
            this.Group = group;
        }
    }
}
