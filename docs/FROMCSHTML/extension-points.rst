@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Extension Points
----------------

Overview
~~~~~~~~

Coalesce is designed to strike a balance between doing things for you
and allowing you flexibility. This means giving the developer multiple
levels of freedom in controlling the final product.

Model Annotations
~~~~~~~~~~~~~~~~~

Models can be extended a number of ways.

-  Instance method can be added to provide TS/JS functions that can be
   called. The instance object can be returned to update the one on the
   client or some other type of data can be returned.
-  Static methods on the model will be placed on the List view model.
   They can return different types of data.
-  A special version of static methods return IQueryable. These will
   also be placed on the List view model, however the results they
   return will be directly added to the list. This allows for paging and
   the other features of the list. This is used via the listDataSource
   property on the list view model. It is an enumeration with the
   available back-end methods.

Controller Customizations
~~~~~~~~~~~~~~~~~~~~~~~~~

Controllers are generated as partial classes that can be extended with
your own code. The base class for the API controller also has several
overridable methods that get called during the various API calls. These
can be customized in your partial classes. They include functions like
BeforeSave and AfterSave.

Client View Model Extensions
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

On the client side view models are provided in TypeScript. These get
compiled to JavaScript. You can use either language to extend them.
There are two primary ways to do this.

-  Using JS prototyping you can add your own custom methods to the
   ViewModels. The down side is that these have no internal access to
   object variables. This is the most straightforward approach.
-  Each class has an ``init`` callback that gets called during the
   constructor. This allows you to add methods to the ViewModel that
   have full access to internal variables.

Custom Code
~~~~~~~~~~~

Of course, because this is an ASP.NET/MVC application you are always
free to write your own controllers and create your own views. You have
the option to use any part of the framework. Use the APIs with your own
view models. Use the view models with your APIs. Create a custom set of
APIs and view models. Use the APIs with Angular View Models.

At some point the infrastructure may be expanded to support plug ins for
generating customer view models.
