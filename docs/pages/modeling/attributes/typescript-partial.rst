
TypeScript Partial
==================

If defined on a model, a typescript file will be generated in
./Scripts/Partials if one does not already exist. 

This behavior allows you to extend the behavior of the generated TypeScript view models.

This 'Partial' TypeScript file contains a class which inherits from the generated TypeScript ViewModel. This class has the same name as the generated ViewModel would normally have, and the generated ViewModel is renamed to ``"<ClassName>Partial"``. The name of the generated ViewModel can be customized by setting the :csharp:`BaseClassName` property on the attribute.

Example Usage
-------------

    .. code-block:: c#

        [TypeScriptPartial]
        public class Employee
        {
            public int EmployeeId { get; set; }

            ...
        }

Properties
----------

    :csharp:`public string BaseClassName { get; set; }`
        If set, overrides the name of the generated ViewModel which becomes the base class for the generated 'Partial' TypeScript file.