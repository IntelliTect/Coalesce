
.. _ExternalTypes:


External Types
--------------

In Coalesce, any type which is connected to your data model but is not directly part of it is considered to be an "external type".

The collection of external types for a data model looks like this:
    
    #. Take all of the database-mapped types in your data model by examining the :csharp:`DbSet<>` properties of your :csharp:`DbContext`.
    #. Take all of the property types and method return types of your database-mapped types.
    #. Any of these types which are not primitives and not database-mapped types are external types.
    #. For any external type, any of the property types which qualify under the above rules are also external types.


.. warning::

    Be careful when using types that you do not own for properties and method returns in your data model. When Coalesce generates external type ViewModels and DTOs, it will not stop until it has exhausted all paths that can be reached by following public property types and method returns.

    In general, you should only expose types that you have created so that you will always have full control over them. Mark any properties you don't wish to expose with :ref:`InternalUse`, or make those members non-public.


Example Data Model
==================


For example, in the following scenario, the following classes have external type TypeScript ViewModels:

    * :csharp:`PluginMetadata`, exposed through a getter-only property on :csharp:`ApplicationPlugin`.
    * :csharp:`PluginResult`, exposed through a method return on :csharp:`ApplicationPlugin`. 

:csharp:`PluginHandler` is not because it not exposed by the model, neither directly nor through any of the other external types.


.. code-block:: c#

    public class AppDbContext : DbContext {
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationPlugin> ApplicationPlugins { get; set; }
    }

    public class Application {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public ICollection<ApplicationPlugin> Plugins { get; set; }
    }

    public class ApplicationPlugin {
        public int ApplicationPluginId { get; set; }
        public int ApplicationId { get; set; }
        public Application Application { get; set; }

        public string TypeName { get; set; }

        private PluginHandler GetInstance() => 
            ((PluginHandler)Activator.CreateInstance(Type.GetType(TypeName)));

        public PluginMetadata Metadata => GetInstance().GetMetadata();

        public PluginResult Invoke(string action, string data) => GetInstance().Invoke(Application, action, data);
    }

    public abstract class PluginHandler { 
        public abstract PluginMetadata GetMetadata();
        public abstract PluginResult Invoke(Application app, string action, string data);
    }

    public abstract class PluginMetadata { 
        public bool Name { get; set; }
        public string Version { get; set; }
        public ICollection<string> Actions { get; set; }
    }

    public abstract class PluginResult { 
        public bool Success { get; set; }
        public string Message { get; set; }
    }


Generated TypeScript
====================

    The TypeScript ViewModels for external types do not have a common base class, and do not have any of the behaviors or convenience properties that the regular :ref:`TypeScriptViewModel` for database-mapped classes have.

    Data Properties
        For each exposed property on the underlying EF POCO, a :ts:`KnockoutObservable<T>` property will exist on the TypeScript model. For POCO properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections, these properties will be :ts:`KnockoutObservableArray<T>` objects.

        .. code-block:: typescript

            public personId: KnockoutObservable<number> = ko.observable(null);
            public fullName: KnockoutObservable<string> = ko.observable(null);
            public gender: KnockoutObservable<number> = ko.observable(null);
            public companyId: KnockoutObservable<number> = ko.observable(null);
            public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);
            public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
            public birthDate: KnockoutObservable<moment.Moment> = ko.observable(moment());

    Computed Text Properties
        For each Enum property on your POCO, a :ts:`KnockoutComputed<string>` property will be created that will provide the text to display for that property.

        .. code-block:: typescript

            public genderText: () => string;


            
Loading & Serialization
=======================

External types have slightly different behavior when undergoing serialization to be sent to the client. Unlike database-mapped types which are subject to the rules of :ref:`IncludeTree`, external types ignore the Include Tree when being mapped to DTOs for serialization. Read :ref:`IncludeTree`/:ref:`ExternalTypeIncludeTreeCaveats` for a more detailed explanation of this exception.