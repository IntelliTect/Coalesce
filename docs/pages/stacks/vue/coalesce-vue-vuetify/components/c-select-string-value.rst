.. _c-select-string-value:

c-select-string-value
=====================

.. MARKER:summary
    
A dropdown component that will present a list of suggested string values from a custom API endpoint. Allows users to input values that aren't provided by the endpoint.

Effectively, this is a server-driven autocomplete list.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------


.. code-block:: sfc

    <c-select-string-value 
        :model="person" 
        for="jobTitle"
        method="getSuggestedJobTitles"
    />
    

.. code-block:: c#

    class Person 
    {
        public int PersonId { get; set; } 

        public string JobTitle { get; set; }

        [Coalesce]
        public static Task<ICollection<string>> GetSuggestedJobTitles(AppDbContext db, string search) 
        {
            return db.People
                .Select(p => p.JobTitle)
                .Distinct()
                .Where(t => t.StartsWith(search))
                .OrderBy(t => t)
                .Take(100)
                .ToListAsync()
        }
    }

Props
-----

:ts:`for: string | Property | Value`
    A metadata specifier for the value being bound. One of:
    
    - A string with the name of the value belonging to :ts:`model`.
    - A direct reference to a metadata object.
    - A string in dot-notation that starts with a type name.

:ts:`model: Model`
    An object owning the value that was specified by the :ts:`for` prop. If provided, the input will be bound to the corresponding property on the :ts:`model` object.

:ts:`method: string`
    The camel-cased name of the :ref:`Custom Method <ModelMethods>` to invoke to get the list of valid values. Will be passed a single string parameter :ts:`search`. Must be a static method on the type of the provided :ts:`model` object that returns a collection of strings.

:ts:`params?: DataSourceParameters`
    An optional set of :ref:`Data Source Standard Parameters <DataSourceStandardParameters>` to pass to API calls made to the server.

:ts:`listWhenEmpty?: boolean = false`
    True if the method should be invoked and the list displayed when the entered search term is blank.



