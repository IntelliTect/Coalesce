DtoIncludes & DtoExcludes
=========================

Allows for easily controlling when data gets set to the client. When
requesting data from the generated client-side list view models you can
specify an :ts:`includes` property on the View Model. This property controls two areas:

-  If your underlying class implements :csharp:`IIncludable`, the :csharp:`Include`
   method will be called before querying the database, and you can
   choose which navigation properties to include based on the passed view. See ControllingLoading_.
-  When the database entries are returned to the client they will be
   trimmed based on the requested view and the values in :csharp:`DtoExcludes` and
   :csharp:`DtoIncludes`.

    .. caution::
   
        These attributes are **not security attributes** - consumers of your application's API can set this to any value when making a request.

        Do not use them to keep certain data private - use the SecurityAttribute_ family of attributes for that.
   


Example Usage
-------------

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

    In TypeScript:

    .. code-block:: typescript 

        var personList = new ListViewModels.PersonList();
        personList.includes = "Editor";
        personList.load(() => {
            // objects in personList.items will not contain CreatedBy nor Department objects.
        });


Properties
----------

    :csharp:`public string ContentViews { get; set; }` :ctor:`1`
        A comma-delimited list of possible values of :ts:`includes` on which to operate.

        For :csharp:`DtoIncludes`, this will be the possible values of :ts:`includes` for which this property will be allowed to be serialized and sent to the client.

        .. important::
        
            :csharp:`DtoIncludes` does not ensure that specific data will be loaded from the database. Only data loaded into current EF DbContext can possibly be returned from the API. See ControllingLoading_.

        For :csharp:`DtoExcludes`, this will be the possible values of :ts:`includes` for which this property will **never** be serialized and sent to the client.

Preset Values
.............

    There are a few values of :ts:`includes` that will be set by default by the scripts that drive Coalesce's auto-generated views:

    :code:`Editor`
        Used when loading an object in the generated CreateEdit views.
    :code:`<ModelName>ListGen`
        Used when loading a list of objects in the generated Table and Cards views.
        For example, :code:`PersonListGen`