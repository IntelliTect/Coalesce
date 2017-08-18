
.. _CustomDataSources:

Custom Data Sources
-------------------

Static methods on your model that return an IQueryable of the same type as the class are treated specially. These methods, called custom data sources, are used to form queries that will fetch the data needed for the ViewModels and ListViewModels.

The structure of the :csharp:`IQueryable` returned by a custom data source is used to shape and trim the structure of the DTO as it is serialized and sent out to the client. See :ref:`IncludeTree` for more information on how this works.

Defining Data Sources
.....................

These methods must receive two parameters: :csharp:`db` as the :csharp:`DbContext` type, and a :csharp:`ClaimsPrincipal user`. `Additional parameters`_ are also outlined below_.

.. code-block:: c#

    public class Person
    {
        public static IQueryable<Person> IncludeFamily(AppDbContext db, ClaimsPrincipal user)
        {
            return db.People
                .Include(f => f.Parents).ThenInclude(s => s.Parents)
                .Include(f => f.Cousins).ThenInclude(s => s.Parents);
        }

        public static IQueryable<Person> NamesStartingWithA(AppDbContext db, ClaimsPrincipal user)
        {
            return db.People
                .Include(f => f.Siblings)
                .Where(f => f.FirstName.StartsWith("A"));
        }
    }


Consuming Data Sources
......................

The TypeScript viewmodels have a property called :ts:`dataSource`, and the list viewmodels have a property called :ts:`listDataSource`. These properties accept an enumeration with the available list methods:

.. code-block:: typescript

    var viewModel = new ViewModels.Person();
    viewModel.dataSource = viewModel.dataSources.IncludeFamily;
    viewModel.load(1);

    var list = new ListViewModels.PersonList();
    list.listDataSource = list.dataSources.NamesStartingWithA;
    list.load();

When a custom data source is used, the :ref:`DefaultLoadingBehavior` is suppressed. To get this behavior easily inside your data source, call the :csharp:`IQueryable<T>.IncludeChildren()` extension method (:csharp:`using IntelliTect.Coalesce.Data;`).

Additionally, when a custom data source is used on a model that implements :csharp:`IIncludable` (see :ref:`Includes`), the :csharp:`IIncludable.Include` method will not be called. 



.. _below:

Additional Parameters
.....................

In addition to these two required parameters, you can also add additional optional parameters based on the properties of your models:

    When using a custom data source on a :ts:`ViewModel`, only one additional parameter, :ts:`id`, will be populated. When your data source is called to load a single object, the :ts:`id` parameter will contain the value of the primary key of the object being loaded. The type must match the type of your model's primary key.

        A contrived example:

        .. code-block:: c#

            public class Person
            {
                public static IQueryable<Person> IncludeFamily(AppDbContext db, ClaimsPrincipal user, int id)
                {
                    var person = db.People.FirstOrDefault(p => p.PersonId == id);

                    return db.People
                        .Include(f => f.Parents)
                        .Include(f => f.Cousins).ThenInclude(s => s.Parents);
                }
            }

    When using a custom data source on a :ts:`ListViewModel`, one parameter will populated for each property on your model where a filter was set via :ts:`ListViewModel.query["propertyName"]`.

        A contrived example:

        .. code-block:: c#

            public static IQueryable<Person> NamesStartingWithA(AppDbContext db, ClaimsPrincipal user, string lastName)
            {
                return db.People
                    .Include(f => f.Siblings)
                    .Where(f => f.FirstName.StartsWith("A") && (lastName == null || lastName == f.LastName));
            }

        .. code-block:: typescript

            var list = new ListViewModels.PersonList();
            list.query = {lastName: "Erickson"};
            list.listDataSource = list.dataSources.NamesStartingWithA;
            list.load();

    If no value is available for these additional parameters, they will contain their default value.
