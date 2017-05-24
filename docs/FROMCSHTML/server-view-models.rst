@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Server View Models
------------------

Overview
~~~~~~~~

Coalesce uses an internal view model to represent your data model. These
types are found in the IntelliTect.Coalesce.TypeDefinition namespace.
Internally, these type definition classes use both Reflection via Types
and Roslyn via Symbols.

They are used with Roslyn during code generation and with reflection
during runtime. They encapsulate much of the complexity of using the
underlying technologies and present a consistent interface to the
developer.

The type definition view models contain the following classes:

-  **ReflectionRepository:** Collection of all the ClassViewModels.
-  **ClassViewModel:** Representation of classes of the DbSets in your
   Context.
-  **PropertyViewModel:** Representation of properties on a
   ClassViewModel.
-  **MethodViewModel:** Representation of methods on a ClassViewModel.
-  **TypeViewModel:** Represents either a return value or parameter of a
   method or the type of a property.
-  **ParameterViewModel:** Representation of a parameter of a
   MethodViewModel.

These object are available using the following code.

::

    ReflectionRepository.AddContext<DbContext>();

Enumerating Your Data Model
~~~~~~~~~~~~~~~~~~~~~~~~~~~

It is easy to get meta data information about your view model. There are
many convenience methods and properties to make accessing child elements
easy. This is how the Display and Knockout helper extensions work behind
the scenes.

::


        var objectVm = ReflectionRepository.GetClassViewModel<Person>();
        var propertyVm = objectVm.PropertyBySelector<Person>(p=>p.FirstName);
        Console.WriteLine(propertyVm.DisplayName);

