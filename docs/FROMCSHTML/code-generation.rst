@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Code Generation
---------------

Overview
~~~~~~~~

Coalesce uses a combination of code generation, base classes, and
runtime reflection to provide its features. At the core is the Roslyn
compiler used for code generation.

Setting up your project
~~~~~~~~~~~~~~~~~~~~~~~

Follow these steps to set your project up for code generation. The
assumption is that you have a domain project with your POCO objects and
your DbContext and a web project that contains your web site.

-  From the web project reference the domain project.
-  In the domain project, add a reference to IntelliTect.Coalesce.
-  In the web project, add a reference to
   IntelliTect.Extensions.CodeGeneration.Mvc. This automatically
   references IntelliTect.Coalesce
-  From a command prompt in the folder for the web project run the
   following command.

   ``             dnx gen scripts -dc [nameOfDbContextClass]         ``

This will generate the view models and copy the salient files to your
solution. If you don't have Gulp or other tooling files, this will give
you templates as well as run an initial restore of the 3rd party tooling
that is needed (e.g. node modules that are referenced in package.json
and bower components that are listed in bower.json). If you don't change
these templates, they will continue to update. If you change them, new
copies can be found in the IntelliTect folder in your solution.

Web Configuration
~~~~~~~~~~~~~~~~~

Several configurations are necessary for the admin pages to work
correctly out of the box.

-  To startup.cs add the following line to the ConfigureServices method.
   This seeds the ReflectionRepository with the objects from your data
   context. This can then be used to access server side view models that
   represent your data model at runtime. This allows you to easily do
   dynamic view generation.

   ``ReflectionRepository.AddContext<DbContext>();``

-  Make sure entity framework is configured in the ConfigureServices
   method with something like the following.

   ::

               services.AddEntityFramework()
               .AddSqlServer()
               .AddDbContext<DbContext>
                   (options => options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));
               

-  Make sure JSON serialization is configured to ignore looping in the
   ConfigureServices method..

   ::

               services.AddMvc().AddJsonOptions(options =>
               {
                   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               });
               

\_layout.cshtml for admin pages.
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

By default, the admin pages use the \_layout.cshtml file in the
View\\Shared folder. If you are starting from a blank project, an
\_layout.cshtml file will be copied into your View\\Shared folder. If an
\_layout.cshtml file already exists, you will need to either copy the
\_layout.cshtml from the IntelliTect folder or just copy and paste the
scripts and styles sections.
