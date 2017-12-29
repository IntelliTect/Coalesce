
[SelectFilter]
==============

Specify a property to restrict dropdown menus by. Values presented will
be only those where the value of the foreign property matches the value
of the local property.

The local property name defaults to the same value of the foreign
property.

Additionally, in place of a :csharp:`LocalPropertyName` to check against, you
may instead specify a static value using :csharp:`StaticPropertyValue` to
filter by a constant.

.. important::
    This attribute only affects generated HTML - it does not enforce any relational rules in your data.

Example Usage
-------------

    In this example, a dropdown for :csharp:`EmployeeRank` created using ``@Knockout.SelectForObject`` in cshtml files will only present possible values of :csharp:`EmployeeRank` which are valid for the :csharp:`EmployeeType` of the :csharp:`Employee`.

    .. code-block:: c#

        public class Employee
        {
            public int EmployeeId { get; set; }
            public int EmployeeTypeId { get; set; }
            public EmployeeType EmployeeType { get; set; }
            public int EmployeeRankId { get; set; }
        
            [SelectFilter(ForeignPropertyName = nameof(EmployeeRank.EmployeeTypeId), LocalPropertyName = nameof(Employee.EmployeeTypeId))]
            public EmployeeRank EmployeeRank { get; set; }
        }
        
        public class EmployeeRank
        {
            public int EmployeeRankId { get; set; }
            public int EmployeeTypeId { get; set; }
            public EmployeeType EmployeeType { get; set; }
        }

    .. code-block:: html

        <div>
            @(Knockout.SelectForObject<Models.Employee>(e => e.EmployeeRank))
        </div>

Properties
----------

    :csharp:`public string ForeignPropertyName { get; set; }`
        The name of the property on the foreign object to filter against.

    :csharp:`public string LocalPropertyName { get; set; }`
        The name of another property belonging to the class in which this attribute is used.
        The results of select lists will be filtered to match this value.
        
        Defaults to the value of :csharp:`ForeignPropertyName` if not set.

    :csharp:`public string LocalPropertyObjectName { get; set; }`
        If specified, the :csharp:`LocalPropertyName` will be resolved from the property by this name that resides on the local object.
        
        This allows for querying against properties that are one level away from the current object.

    :csharp:`public ValueType StaticPropertyValue { get; set; }`
        A constant value that the foreign property will be filtered against.
        If this is set, :csharp:`LocalPropertyName` will be ignored.