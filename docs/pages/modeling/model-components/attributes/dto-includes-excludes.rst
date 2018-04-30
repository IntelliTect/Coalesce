
.. _DtoIncludesExcludesAttr:

[DtoIncludes] & [DtoExcludes]
=============================

Allows for easily controlling what data gets set to the client. When requesting data from the generated client-side list view models, you can specify an :ts:`includes` property on the ViewModel or ListViewModel. 

For more information about the includes string, see :ref:`Includes`.

When the database entries are returned to the client they will be trimmed based on the requested includes string and the values in :csharp:`DtoExcludes` and :csharp:`DtoIncludes`.

    .. caution::
   
        These attributes are **not security attributes** - consumers of your application's API can set the includes string to any value when making a request.

        Do not use them to keep certain data private - use the :ref:`SecurityAttribute` family of attributes for that.
   

It is important to note that the value of the includes string will match against these attributes on *any* of your models that appears in the object graph being mapped to DTOs - it is not limited only to the model type of the root object.

Example Usage
.............

    .. code-block:: c#

        public class Person
        {
            // Don't include CreatedBy when editing - will be included for all other views
            [DtoExcludes("Editor")]
            public AppUser CreatedBy { get; set; }

            // Only include the Person's Department when :ts:`includes == "details"` on the TypeScript ViewModel.
            [DtoIncludes("details")]
            public Department Department { get; set; }

            // LastName will be included in all views
            public string LastName { get; set; }
        }

        public class Department
        {
            [DtoIncludes("details")]
            public ICollection<Person> People { get; set; }
        }

    In TypeScript:

    .. code-block:: typescript 

        var personList = new ListViewModels.PersonList();
        personList.includes = "Editor";
        personList.load(() => {
            // objects in personList.items will not contain CreatedBy nor Department objects.
        });

    .. code-block:: typescript 

        var personList = new ListViewModels.PersonList();
        personList.includes = "details";
        personList.load(() => {
            // objects in personList.items will be allowed to contain both CreatedBy and Department objects. Department will be allowed to include its other Person objects.
        });


Properties
..........

    :csharp:`public string ContentViews { get; set; }` :ctor:`1`
        A comma-delimited list of values of :ts:`includes` on which to operate.

        For :csharp:`DtoIncludes`, this will be the values of :ts:`includes` for which this property will be allowed to be serialized and sent to the client.

        .. important::
        
            :csharp:`DtoIncludes` does not ensure that specific data will be loaded from the database. Only data loaded into current EF DbContext can possibly be returned from the API. See :ref:`Data Sources <CustomDataSources>` for more information.

        For :csharp:`DtoExcludes`, this will be the values of :ts:`includes` for which this property will **never** be serialized and sent to the client.
