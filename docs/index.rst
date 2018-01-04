.. Coalesce documentation master file, created by
   sphinx-quickstart on Mon May 22 10:25:25 2017.
   You can adapt this file completely to your liking, but it should at least
   contain the root `toctree` directive.

.. _IntelliTect:
    https://intellitect.com


Coalesce
========

Designed to help you quickly build amazing web applications, Coalesce is a rapid-development code generation framework, created by IntelliTect_ and built on top of:

    - ASP.NET Core
    - EF Core
    - TypeScript
    - Knockout


What do I do?
-------------

You are responsible for the interesting parts of your application:

-  Data Model
-  Business Logic
-  External Integrations
-  Page Content
-  Site Design
-  Custom Scripting

What is done for me?
--------------------

Coalesce builds the part of your application that are mundane and
monotonous to build:

-  Client side Knockout :ref:`TypeScriptViewModel` that mirror your data model for both :ref:`lists <TypeScriptListViewModel>` and :ref:`individual objects <TypeScriptViewModel>`. Utilize these to rapidly build out your applications various pages.
-  APIs to interact with your models via endpoints like List, Get, Save, and more.
-  :ref:`Out-of-the-box bindings <KnockoutBindings>` for common controls like dates, selecting objects via drop downs, enums, etc. Dropdowns support searching and paging automatically.
-  A complete set of admin pages are built, allowing you to read, create, edit, and delete data straight away without writing any additional code.


Getting Started
===============

Creating a Project
------------------

The quickest and easiest way to create a new Coalesce application is to check out the starter project: https://github.com/IntelliTect/Coalesce.Starter. This project already contains all the needed projects, dependencies, and configuration. In PowerShell:

    ::

        mkdir MyProject
        cd MyProject 
        git clone --depth 1 https://github.com/IntelliTect/Coalesce.Starter.git .
        ./RenameProject.ps1

At this point, you can open up the solution in Visual Studio and run your application. However, your application won't do much without a data model, so you will probably want to do the following before running:

    - Create an initial :ref:`Data Model <EntityModels>` by adding EF entity classes to the data project and the corresponding :csharp:`DbSet<>` properties to :csharp:`AppDbContext`. You will notice that the starter project includes a single model, :csharp:`ApplicationUser`, to start with. Feel free to change this model or remove it entirely. Read :ref:`EntityModels` for more information about creating a data model. 
    
    - Run ``dotnet ef migrations add Init`` (Init can be any name) in the data project to create an initial database migration.

    - Run Coalesce's code generation by either:

        - Running ``dotnet coalesce`` in the web project's root directory
        - Running the ``coalesce`` gulp task in the Task Runner Explorer

You're now at a point where you can start creating your own controllers (or utilizing the generated :csharp:`partial` controllers) to serve your own pages with your own scripts.


Building Pages & Features
-------------------------

Lets say we've created a :ref:`model <EntityModels>` called :csharp:`Person` like so, and we've ran code generation with ``dotnet coalesce``:

    .. code-block:: c#

        namespace MyApplication.Data.Models 
        {
            public class Person
            {
                public int PersonId { get; set; }
                public string Name { get; set; }
                public DateTimeOffset? BirthDate { get; set; }
            }
        }

We can create a details page for a Person by creating:

    A controller in ``src/MyApplication.Web/Controllers/PersonController.cs``:

    .. code-block:: c#

        namespace MyApplication.Web.Controllers
        {
            public partial class PersonController
            {
                public IActionResult Details() => View();
            }
        }

    A view in ``src/MyApplication.Web/Views/Person/Details.cshtml``:

    .. code-block:: html
                
        <h1>Person Details</h1>

        <div data-bind="with: person">
            <dl class="dl-horizontal">
                <dt>Name </dt>
                <dd data-bind="text: name"></dd>

                <dt>Date of Birth </dt>
                <dd data-bind="moment: birthBirth, format: 'MM/DD/YYYY hh:mm a'"></dd>
            </dl>
        </div>

        @section Scripts
        {
        <script src="~/js/person.details.js"></script>
        <script>
            $(function () {
                var vm = new MyApplication.PersonDetails();
                ko.applyBindings(vm);
                vm.load();
            });
        </script>
        }
    
    And a script in ``src/MyApplication.Web/Scripts/person.details.ts``:

    .. code-block:: typescript

        /// <reference path="viewmodels.generated.d.ts" />

        module MyApplication {
            export class PersonDetails {
                public person = new ViewModels.Person();

                load() {
                    var id = Coalesce.Utilities.GetUrlParameter("id");
                    if (id != null && id != '') {
                        this.person.load(id);
                    }
                }
            }
        }

With these pieces in place, we now have a functioning page that will display details about a person. We can start up the application and navigate to ``/Person/Details?id=1`` (assuming a person with ID 1 exists - if not, navigate to ``/Person/Table`` and create one).

From this point, one can start adding more fields, more features, and more flair to the page. Check out all the other documentation in the sidebar to see what else Coalesce has to offer.


.. toctree::
    :hidden:
    :maxdepth: 1
    :titlesonly:
    :glob:
    :caption: Coalesce

.. toctree::
    :hidden:
    :maxdepth: 2
    :titlesonly:
    :caption: Model Types

    pages/modeling/model-types/entities
    pages/modeling/model-types/external-types
    pages/modeling/model-types/dtos

.. toctree::
    :hidden:
    :maxdepth: 3
    :titlesonly:
    :caption: Model Components

    pages/modeling/model-components/properties
    pages/modeling/model-components/attributes
    pages/modeling/model-components/methods
    pages/modeling/model-components/data-sources
    pages/modeling/model-components/behaviors

.. toctree::
    :hidden:
    :maxdepth: 3
    :titlesonly:
    :caption: Generated Code - Server

    pages/topics/generation
    pages/generated/server/dtos

.. toctree::
    :hidden:
    :maxdepth: 3
    :titlesonly:
    :caption: Generated Code - Client

    pages/generated/client/view-model
    pages/generated/client/list-view-model
    pages/generated/client/external-view-model
    pages/generated/client/model-config
    pages/generated/client/bindings

.. toctree::
    :hidden:
    :maxdepth: 3
    :caption: Concepts

    pages/concepts/include-tree
    pages/concepts/includes

.. toctree::
    :hidden:
    :maxdepth: 2
    :glob:
    :caption: Configuration

    pages/topics/startup
    pages/topics/coalesce-json

