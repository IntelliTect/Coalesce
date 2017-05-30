@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Getting Started
---------------

What do I do?
~~~~~~~~~~~~~

You are responsible for the interesting parts of your application:

-  Data Model
-  Business Logic
-  External Integrations
-  Page Content
-  Site Design
-  Custom Scripting

What is done for me?
~~~~~~~~~~~~~~~~~~~~

Coalesce builds the part of your application that are mundane and
monotonous to build:

-  Client side Knockout view models that mirror your data models. Both
   for collections and individual objects.
-  APIs to expose your models with list, get, save, etc methods.
-  Loads data into your view models singly or as a list.
-  Saves changes to your view models automatically as they are made.
-  Provides out-of-the-box bindings for common controls like strings,
   dates, objects via drop downs, enums, etc.
-  Items like list boxes support searching and paging automatically.
-  Grids can be built with two lines of code.
-  A complete admin site is built allowing you to view and edit your
   data.

How do I get started?
~~~~~~~~~~~~~~~~~~~~~

Coalesce is designed around productivity, so having an amazing
development story is paramount. Here are the basic steps.

#. Create an ASP.NET Core web application
#. Create a data model using EF Core
#. Install the Coalesce NuGet package
#. Run the scaffolding command
#. Run your site and check out the admin pages
#. Start customizing
#. Rerun the scaffolder when your model changes

Technologies
~~~~~~~~~~~~

Here is a short list of the technologies we are using behind the scenes.
We have intentionally kept this as light as possible without rewriting
existing functionality. The tooling used is all directly from Visual
Studio. All the 3rd party tools require no royalties.

-  ASP.NET Core
-  MVC Core
-  Entity Framework Core
-  C# 6
-  TypeScript: all generated client-side code
-  Bower: web packages
-  Gulp: for build steps
-  NPM: Gulp packages
-  NuGet: .net packages
-  Knockout: databinding
-  Moment.js: time manipulation
-  Bootstrap: core css for admin pages
-  jQuery
-  jQuery Validation

Source Control
~~~~~~~~~~~~~~

We have found it works best if the following folders are not checked in
to source control.

-  wwwroot
-  bower\_components
-  node\_modules
