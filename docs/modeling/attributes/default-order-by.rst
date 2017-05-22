
Default Order By
================

Allows setting of the default manner in which the data returned to the client will be sorted. Multiple fields can be used to sort an object by specifying an index.

Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            
            public int DepartmentId { get; set; }

            [DefaultOrderBy(FieldOrder = 0, FieldName = nameof(Department.Order))]
            public Department Department { get; set; }
            
            [DefaultOrderBy(FieldOrder = 1)]
            public string LastName { get; set; }
        }
        
    .. code-block:: c#

        public class LoginHistory
        {
            public int LoginHistoryId {get; set;}
            
            [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending)]
            public DateTime Date {get; set;}
        }


Properties
----------

    :csharp:`public int FieldOrder { get; set; }` :ctor:`1`
        Specify the index of this field when sorting by multiple fields.

        Lower-valued properties will be used first; higher-valued properties will be used as a tiebreaker (i.e. :csharp:`.ThenBy(...)`).

    :csharp:`public OrderByDirections OrderByDirection { get; set; }` :ctor:`2`
        Specify the direction of the ordering for the property.

        Enum values are:
            - :csharp:`DefaultOrderByAttribute.OrderByDirections.Ascending`
            - :csharp:`DefaultOrderByAttribute.OrderByDirections.Descending`

    :csharp:`public string FieldName { get; set; }`
        When using the :csharp:`DefaultOrderByAttribute` on an object property, specifies the field on the object to use for sorting. See the first example above.
