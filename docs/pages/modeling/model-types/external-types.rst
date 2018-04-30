
.. _ExternalTypes:

External Types
--------------

In Coalesce, any type which is connected to your data model but is not directly part of it is considered to be an "external type".

The collection of external types for a data model looks like this:
    
    #. Take all of the api-served types in your data model. This includes :ref:`EntityModels` and :ref:`CustomDTOs`.
    #. Take all of the property types, method parameters, and method return types of these types.
    #. Any of these types which are not primitives and not database-mapped types are external types.
    #. For any external type, any of the property types which qualify under the above rules are also external types.


.. warning::

    Be careful when using types that you do not own for properties and method returns in your data model. When Coalesce generates external type ViewModels and DTOs, it will not stop until it has exhausted all paths that can be reached by following public property types and method returns.

    In general, you should only expose types that you have created so that you will always have full control over them. Mark any properties you don't wish to expose with :ref:`InternalUse`, or make those members non-public.


Generated Code
==============

For each external type found in your application's model, Coalesce will generate:

    * A :ref:`Generated DTO <GenDTOs>`
    * A :ref:`TypeScript Model <TypeScriptExternalViewModel>`


Example Data Model
==================

For example, in the following scenario, the following classes are considered as external types:

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

            
Loading & Serialization
=======================

External types have slightly different behavior when undergoing serialization to be sent to the client. Unlike database-mapped types which are subject to the rules of :ref:`IncludeTree`, external types ignore the Include Tree when being mapped to DTOs for serialization. Read :ref:`IncludeTree`/:ref:`ExternalTypeIncludeTreeCaveats` for a more detailed explanation of this exception.