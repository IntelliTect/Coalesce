
.. _EntityModels:

Entity Models
================

Models are the core business objects of your application - they serve as the fundamental representation of data in your application. The design of your models is very important. In `Entity Framework Core`_ (EF), data models are just Plain Old CLR Objects (POCOs).

.. contents:: Contents
    :local:

.. _Entity Framework Core:
.. _EF Core:
.. _EF:
    https://docs.microsoft.com/en-us/ef/core/


Building a Data Model
---------------------

To start building your data model that Coalesce will generate code for, follow the best practices for `EF Core`_.

Guidance on this topic is available in abundance in the `Entity Framework Core`_ documentation.

Don't worry about querying or saving data when you're just getting started - Coalesce will provide a lot of that functionality for you, and it is very easy to customize what Coalesce offers later. To get started, just build your POCOs and `DbContext` classes. Annotate your `DbContext` class with `[Coalesce]` so that Coalesce will discover it and generate code based off of your context for you.

Before you start building, you are highly encouraged to read the sections below. The linked pages explain in greater detail what Coalesce will build for you for each part of your data model.

Properties
~~~~~~~~~~

Read :ref:`ModelProperties` for an outline of the different types of properties that you may place on your models and the code that Coalesce will generate for each of them.


Attributes
~~~~~~~~~~

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from `System.ComponentModel.DataAnnotations`.

Read :ref:`ModelAttributes` to learn more.


Methods
~~~~~~~

You can place both static and interface methods on your model classes. Any public methods annotated with :ref:`[Coalesce] <CoalesceAttribute>` will have a generated API endpoint and corresponding generated TypeScript members for calling this API endpoint. Read :ref:`ModelMethods` to learn more.


Customizing CRUD Operations
---------------------------

Once you've got a solid data model in place, its time to start customizing the way that Coalesce will *read* your data, as well as the way that it will handle your data when processing *creates*, *updates*, and *deletes*.

Data Sources
~~~~~~~~~~~~

The method by which you can control what data the users of your application can access through Coalesce's generated APIs is by creating custom data sources. These are classes that allow complete control over the way that data is retrieved from your database and provided to clients. Read :ref:`DataSources` to learn more.

Behaviors
~~~~~~~~~~~~

Behaviors in Coalesce are to mutating data as data sources are to reading data. Defining a behaviors class for a model allows complete control over the way that Coalesce will create, update, and delete your application's data in response to requests made through its generated API. Read :ref:`Behaviors` to learn more.


Standalone (non-EF) Entities
----------------------------

In Coalesce, Standalone Entities are entity types that are not based on Entity Framework. These types are discovered by Coalesce by annotating them with `[Coalesce, StandaloneEntity]`.

For these types, you must define at least one custom :ref:`Data Source <DataSources>`, and optionally a :ref:`Behaviors` class as well. If no behaviors are defined, the type is implicitly read-only, equivalent to turning off create/edit/delete via the :ref:`SecurityAttributes`.

To define data sources and behaviors for Standalone Entities, it is recommended you inherit from :cs:`StandardDataSource<T>` and :cs:`StandardBehaviors<T>`, respectively. For example:

.. code-block:: c#

    [Coalesce, StandaloneEntity]
    public class StandaloneExample
    {
        public int Id { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
        public string Name { get; set; } = "";

        [DefaultOrderBy]
        public DateTimeOffset Date { get; set; }

        private static int nextId = 0;
        private static ConcurrentDictionary<int, StandaloneExample> backingStore = new ConcurrentDictionary<int, StandaloneExample>();

        public class DefaultSource : StandardDataSource<StandaloneExample>
        {
            public DefaultSource(CrudContext context) : base(context) { }

            public override Task<IQueryable<StandaloneExample>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(backingStore.Values.AsQueryable());
        }

        public class Behaviors : StandardBehaviors<StandaloneExample>
        {
            public Behaviors(CrudContext context) : base(context) { }

            public override Task ExecuteDeleteAsync(StandaloneExample item)
            {
                backingStore.TryRemove(item.Id, out _);
                return Task.CompletedTask;
            }

            public override Task ExecuteSaveAsync(SaveKind kind, StandaloneExample? oldItem, StandaloneExample item)
            {
                if (kind == SaveKind.Create)
                {
                    item.Id = Interlocked.Increment(ref nextId);
                    backingStore.TryAdd(item.Id, item);
                }
                else
                {
                    backingStore.TryRemove(item.Id, out _);
                    backingStore.TryAdd(item.Id, item);
                }
                return Task.CompletedTask;
            }
        }
    }

The above example is admittedly contrived, as it is unlikely that you would be using an in-memory collection as a data persistence mechanism. A more likely real-world scenario would be to inject an interface to some other data store. Data Source and Behavior classes are instantiated using your application's service provider, so any registered service can be injected.