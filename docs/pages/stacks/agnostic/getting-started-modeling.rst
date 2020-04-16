
.. MARKER:creating-project

Creating a Project
------------------

The quickest and easiest way to create a new Coalesce application is to use the ``dotnet new`` templates that have been created. In your favorite shell:

.. |vuebadge| image:: https://img.shields.io/nuget/v/IntelliTect.Coalesce.Vue.Template   
    :alt: Nuget
    :target: https://www.nuget.org/packages/IntelliTect.Coalesce.Vue.Template/

.. |kobadge| image:: https://img.shields.io/nuget/v/IntelliTect.Coalesce.KnockoutJS.Template   
    :alt: Nuget
    :target: https://www.nuget.org/packages/IntelliTect.Coalesce.KnockoutJS.Template/

`Vue Template <https://github.com/IntelliTect/Coalesce.Vue.Template>`_: |vuebadge|

    .. code-block:: sh

        dotnet new --install IntelliTect.Coalesce.Vue.Template
        dotnet new coalescevue

`Knockout Template <https://github.com/IntelliTect/Coalesce.KnockoutJS.Template>`_: |kobadge|
    
    .. code-block:: sh
    
        dotnet new --install IntelliTect.Coalesce.KnockoutJS.Template
        dotnet new coalesceko

.. MARKER:creating-project-end


.. MARKER:data-modeling

Data Modeling
-------------

At this point, you can open up the newly-created solution in Visual Studio and run your application. However, your application won't do much without a data model, so you will probably want to do the following before running:

- Create an initial :ref:`Data Model <EntityModels>` by adding EF entity classes to the data project and the corresponding :csharp:`DbSet<>` properties to :csharp:`AppDbContext`. You will notice that the starter project includes a single model, :csharp:`ApplicationUser`, to start with. Feel free to change this model or remove it entirely. Read :ref:`EntityModels` for more information about creating a data model. 

- Run ``dotnet ef migrations add Init`` (Init can be any name) in the data project to create an initial database migration.

- Run Coalesce's code generation by either:

    - Running ``dotnet coalesce`` in the web project's root directory
    - Running the ``coalesce`` npm script (Vue) or gulp task (Knockout) in the Task Runner Explorer

You're now at a point where you can start creating your own pages!


.. MARKER:data-modeling-end