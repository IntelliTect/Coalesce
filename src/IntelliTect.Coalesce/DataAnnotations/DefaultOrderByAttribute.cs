using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows specifying the default sort order for returns lists of this object type.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class DefaultOrderByAttribute : System.Attribute
    {
        public enum OrderByDirections
        {
            Ascending = 0,
            Descending = 1
        }

        public OrderByDirections OrderByDirection { get; set; }
        public int FieldOrder { get; set; }

        /// <summary>
        /// When using the DefaultOrderByAttribute on an object property, specifies the field on the object to use for sorting.
        /// </summary>
        public string? FieldName { get; set; }

        public DefaultOrderByAttribute(int fieldOrder = 0, OrderByDirections orderByDirection = OrderByDirections.Ascending)
        {
            this.OrderByDirection = orderByDirection;
            this.FieldOrder = fieldOrder;
        }
    }

    public class OrderByInformation
    {
        /// <summary>
        /// The property to order by. If this contains multiple properties,
        /// it represents chained ancestor properties of the desired property.
        /// </summary>
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();

        public DefaultOrderByAttribute.OrderByDirections OrderByDirection { get; set; }
        public int FieldOrder { get; set; }

        public string OrderExpression(string prependText = "")
        {
            string text = Properties.Count > 1 ? "(" : "";
            string propAccessor = prependText;
            propAccessor += (propAccessor == "" ? "" : ".")  + Properties[0].Name;
            foreach (var prop in Properties.Skip(1))
            {
                text += $"{propAccessor} == null ? {prop.Type.CsDefaultValue} : ";
                propAccessor += (propAccessor == "" ? "" : ".") + prop.Name;
            }

            text += propAccessor + (Properties.Count > 1 ? ")" : "");

            return text;
        }

    }
}
