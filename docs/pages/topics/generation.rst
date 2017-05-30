Code Generation
===============

Coalesce generates a number of bits and pieces of code based on your models.

.. contents:: Contents
    :local:



Generated Code
--------------

Below you will find a brief overview of each of the different pieces of code that Coalesce will generate for you.


TypeScript
..........

Coalesce generates a number of different types of TypeScript classes to support your data through the generated API.

ViewModels
    One view model class is generated for each of your EF Database-mapped POCO classes. These models contain fields for your model :ref:`ModelProperties`, and functions and other members for your model :ref:`ModelMethods`. They also contain a number of standard fields & functions inherited from :ts:`BaseViewModel` which offer basic loading & saving functionality, as well as other handy utility members for use with Knockout.

    See :ref:`TypeScriptViewModel` for more details.

ListViewModels
    One ListViewModel is generated for each of your EF Database-mapped POCO classes. These classes contain functionality for loading sets of objects from the server. They provide searching, paging, sorting, and filtering functionality.

    See :ref:`TypeScriptListViewModel` for more details.

ExternalType
    Any types which are accessible through your Database-mapped POCO classes, either through one of its getter-only :ref:`ModelProperties` or return value from one of its :ref:`ModelMethods`, will have a corresponding TypeScript ViewModel generated for it. These ViewModels only provide a :ts:`KnockoutObservable` field for each property on the C# class.


C# DTOs
.......

For each of your EF Database-mapped POCO classes, a C# DTO class is created. These classes are used to hold the data that will be serialized and sent to the client, as well as data that has been received from the client before it has been mapped back to your EF POCO class.

See :ref:`ControllingLoading` and :ref:`DTOs` for more information.


API Controllers
...............

For each of your EF Database-mapped POCO classes, an API controller is created in the ``/Api/Generated`` directory of your web project. These controllers provide a number of endpoints for interacting with your data.

See :ref:`Api` for more information.


View Controllers
................

For each of your EF Database-mapped POCO classes, a controller is created in the ``/Controllers/Generated`` directory of your web project. These controllers provide routes for the generated admin views.

As you add your own pages to your application, you should add additional partial classes in the ``/Controllers`` that extend these generated partial classes to expose those pages.


Admin Views
...........

For each of your EF Database-mapped POCO classes, a number of views are generated to provide administrative-level access to your data.

Table
    Provides a basic table view with sorting, searching, and paging of your data.

TableEdit
    Provides the table view, but with inline editing in the table.

Cards
    Provides a card-based view of your data with searching and paging.

CreateEdit
    Provides an editor view which can be used to create new entities or edit existing ones.

EditorHtml
    Provides a minimal amount of HTML to display an editor for the object type. This is used by the :ts:`showEditor` method on the generated TypeScript ViewModels.


Template Customization
----------------------

For all of the code that Coalesce generates, you are free to edit and customize the templates to your liking. The templates are put into ``/Coalesce/Templates`` the first time that you run the code generation. These are the copies of the templates that will be used from that point onwards whenever you re-run Coalesce to generate your code again.

Another copy of these templates is also put into ``/Coalesce/Originals``. These copies of the templates always represent the pristine versions of the templates that are bundled with Coalesce. They should not be modified (they'll be flagged as read-only in the filesystem), and they are always overwritten when code generation is performed. Their purpose is twofold:

    - First, they are used to detect whether or not you have modified the templates in ``/Coalesce/Templates``. If the template file and the original file are the same, Coalesce overwrites both when code generation is ran. Otherwise, the template file is left alone in order to preserve your modifications.
    - Second, they offer an easily accessible version of the pristine templates. If you have issues with your modified templates, or if you update Coalesce and want any changes that have been made to the pristine templates, you can obtain the diff between the old and new revisions of the pristine templates and apply that diff to your modified template.