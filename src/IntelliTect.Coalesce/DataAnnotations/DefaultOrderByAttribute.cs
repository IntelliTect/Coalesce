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
        public string FieldName { get; set; }

        public DefaultOrderByAttribute(int fieldOrder = 0, OrderByDirections orderByDirection = OrderByDirections.Ascending)
        {
            this.OrderByDirection = orderByDirection;
            this.FieldOrder = fieldOrder;
        }

    }

    public class OrderByInformation
    {
        public string FieldName { get; set; }
        public string FieldChildName { get; set; }
        public DefaultOrderByAttribute.OrderByDirections OrderByDirection { get; set; }
        public int FieldOrder { get; set; }
        public string ObjectDefaultValue { get; internal set; }

        public string OrderExpression(string prependText = "")
        {
            if (FieldChildName != null)
            {
                return $"({prependText}{FieldName} == null ? {ObjectDefaultValue}: {prependText}{FieldName}.{FieldChildName})";
            }

            return prependText + FieldName;
        }

    }
}
