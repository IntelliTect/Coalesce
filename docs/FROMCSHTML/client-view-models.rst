@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Client View Models
------------------

Overview
~~~~~~~~

One of the key features of Coalesce is the ability to generate
client-side Knockout view models. View models are simply a
representation of your data in JavaScript in the browser. Additionally,
the properties are wrapped as observables so that when the underlying
values change the web page can be automatically updated.

TypeScript
^^^^^^^^^^

All Coalesce ViewModels are created using TypeScript. If you are
unfamiliar with TypeScript, check out their site at
http://www.typescriptlang.org/. TypeScript lets you write strongly typed
JavaScript. In layman's terms this means that you get IntelliSense and
compile-time type checking. TypeScript compiles to JavaScript and is
very compatible with ECMA Script 2015 and 2016. In the end you can
choose whether you use TypeScript or JavaScript for your client side
code. All the TypeScript in the project compiles to an app.js file (that
by default is EcmaScript 5 compatible) and .map files that allow for
debugging the TypeScript on the client side.

Knockout
^^^^^^^^

Coalesce uses Knockout because it is a simple, single-purpose,
data-binding framework. In the future Coalesce may support other data
binding frameworks like Angular or Aurelia. If you are new to Knockout,
they have some excellent tutorials and documentation on their site at
http://knockoutjs.com.

View Models
~~~~~~~~~~~

Coalesce create two sets of client view models.

-  Class View Models are in the Scripts folder and are named
   ko.[classname].ts. They represent an instance of your model classes.
-  List View Models are in the Scripts folder and are named
   ko.[classname]List.ts. They represent a list of Class View Models.

Class View Models
^^^^^^^^^^^^^^^^^

Class view models represent your class on the client side. They have
observable properties for each of your properties and functions for the
methods. They have load and save functionality and by default save
change to properties automatically. This feature can be disabled and the
save method bound to a button to enable an on demand save.

Coalesce includes an easy way to extend these models with your own
custom typescript. Simply add the ``[TypeScriptPartial]`` attibute to
your C# class. Upon running code generation, a typescript file will then
be generated for you in ./Scripts/Partials that you are free to modify
as desired.

List View Models
^^^^^^^^^^^^^^^^

List view models are designed to retrieve a list of a certain type of
object. The load function get this list and puts it into an array in the
collection. The items in the array are of the type of the Class view
model. This means that if one of the items in the collection is
modified, the change can be automatically saved. This makes creating
editable tables simple.

View Model Documentation
^^^^^^^^^^^^^^^^^^^^^^^^

As a part of the scaffolding, a documentation page is build for each of
your model classes. These contain complete documentation for both of
these view models. It also contains a full description of the generated
Web API.

**Example docs for a Person.**

Usage
~~~~~

The Coalesce view models were designed to be easy to use.

Here is an example of how to get a list of People and bind it to the
current HTML document via Knockout. This will load the first 10 cases
into the view model. Properties can be set on the view model to control
behavior. These can all be bound to graphical elements.

::

        var viewModel = new CaseList();
        ko.applyBindings(viewModel);
        viewModel.load();

Below is an example of how to load a specific item and bind it to an
HTML page. This loads the case with an ID of 1. If you wanted to create
a new one, just leave the load statement out. The object will save
automatically once all the required fields are set.

::

        var viewModel = new Case();
        ko.applyBindings(viewModel);
        viewModel.load(1); // Loads case with ID of 1.

Other Examples
~~~~~~~~~~~~~~

The admin pages provide excellent examples of how to code for various
cases. The demo site has good examples of how to get things like counts
and use custom queries.
