<!-- MARKER:data-modeling -->

# Data Modeling

At this point, you can open up the newly-created solution in Visual Studio and run your application. However, your application won't do much without a data model, so you will probably want to do the following before running:

- Create an initial [Data Model](/modeling/model-types/entities.md) by adding EF entity classes to the data project and the corresponding `DbSet<>` properties to `AppDbContext`. You will notice that the starter project includes a single model, `Widget`, to start with. Feel free to change this model or remove it entirely. Read [Entity Models](/modeling/model-types/entities.md) for more information about creating a data model. 

- Run ``dotnet ef migrations add Init`` (Init can be any name) in the data project to create an initial database migration.

- Run Coalesce's code generation by either:

    - Running ``dotnet coalesce`` in the web project's root directory
    - Running the ``coalesce`` npm script (Vue) or gulp task (Knockout) in the Task Runner Explorer

You're now at a point where you can start creating your own pages!


<!-- MARKER:data-modeling-end -->