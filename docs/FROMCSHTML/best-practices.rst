@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

ASP.NET Core 'Best Practices'
-----------------------------

Overview
~~~~~~~~

ASP.NET Core is a new playing field for .net web developers (and
presumably for other .net developers in the future). This is an exciting
time to be developing with the Microsoft platform. Many of the best
practices from previous generations of ASP.NET still apply, but there
are some new and different aspects to require new approaches.

I hesitate to proclaim 'best' practices for a framework that is so
young. Maybe this is more about providing some helpful hints that we
have discovered after hundreds of hours using ASP.NET during alpha,
beta, and after RTM.

A great place to find guidance is to look at the ASP.NET Core GitHub
repos. They have good examples of how the team is working internally.
However, there are differences with project structure and naming between
projects like ASP.NET and EF.

Solution Structure
~~~~~~~~~~~~~~~~~~

The solution structure in ASP.NET Core is still project based with a
solution that serves as a container for them.

src Folder
^^^^^^^^^^

The key difference is that it is now recommended to place your projects
in a solution folder called src. Because this is a solution folder, it
does not automatically create an actual folder. Creating this folder in
the solution folder makes the actual folder structure match the
structure in Visual Studio and minimizes confusion.

test Folder
^^^^^^^^^^^

Microsoft has adopted putting test projects in this folder.

Solution Items
^^^^^^^^^^^^^^

The solution level puts files in the solution folder in the automatic
solution folder Solution Items. This is where the global.json files goes
and is not created by default. Here is a sample global.json file

::

    {
      "sdk": {
        "version": "1.0.0-preview2-003131"
      },
      "Projects": [ "src", "test" ]
    }

Web Project Structure
~~~~~~~~~~~~~~~~~~~~~

The most significant changes are in web projects. Web projects now use a
wwwroot folder for all web assets rather than having the root of the
project be included with special folder properties controlling
deployment.

wwwroot
^^^^^^^

The wwwroot folder contains all the files served by the web site. It may
be tempting to place all your JavaScript and CSS into this folder.
Resist this urge. By placing them in this folder directly, it makes it
more difficult to handle tasks like compilation and bundling. Instead,
treat all files that you modify like source files and use a tool like
Gulp to compile them and move them into the correct location in wwwroot.

One exception to this rule is for truly static assets like images and
the like. However, there is an advantage to having those in another
folder because it allows for creating things like thumbnails. This way
that process can be automated.

Scripts
^^^^^^^

Use this folder for all your custom script source. This is code that you
write, and not 3rd party libraries. If you use TypeScript and
JavaScript, this is the location to use. Use Gulp to compile and copy
these files to the wwwroot/js (or the like) folder.

Styles
^^^^^^

Using SASS has become commonplace in web development, and ASP.NET 5
embraces this paradigm. This works the same way as the Scripts folder.
Gulp compiles and copies the files into a wwwroot/css folder.

Bower for 3rd Party Components
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In ASP.NET 4 most people used NuGet to acquire all their components both
for .net and for the web (css, js, etc.). While those of us using .net
used NuGet for all components (which was easy), the rest of the
development community migrated to using Bower for all web components.
This caused the complexity of creating custom NuGet packages for web
components made available on Bower. This meant version lag and
inconsistency in deployment strategy. ASP.NET 5 uses Bower to get 3rd
part components.

Bower is a tool based on Node.js. It is integrated into Visual Studio
and a manager akin to NuGet's is provided. The Bower components are
downloaded into a local folder and then compiled and/or copied to the
wwwroot folder. There is some variation in the folder names, but here is
our team's approach.

All Bower components are placed into the bower\_components folder. This
location is specified in the .bowerrc JSON file at the project level.
Gulp then compiles and/or copies these files into the wwwroot/lib folder
with a sub folder for each component. For example a reference to
bootstrap would be /lib/bootstrap/css/boostrap.css. The down side of
this approach is that every Bower component added to the project also
needs to have a change to the Gulp script to ensure it gets copied to
the wwwroot folder. If not, the files aren't accessible via the web
server.

TypeScript
^^^^^^^^^^

One of the key benefits of TypeScript is compile-time type checking and
the resulting IntelliSense. This is done for 3rd party libraries via a
TypeScript Type Definition Fies. These have an extension of d.ts. They
generally found at the DefinitelyTyped repository on GitHub
(https://github.com/DefinitelyTyped/DefinitelyTyped).

For our projects, we create a typings folder that contains folders for
each component containing the .d.ts file or files. The list of
DefinitelyTyped files needed is included in the tsd.json file. There
currently does not appears to be a really slick way to get this files
automatically.

Source Control
~~~~~~~~~~~~~~

Because there are so many cases where files are being compiled and
copied, the source control strategy needs to be adjusted. Again, these
are just the practices we have had the best experience with. There are
reasons to check in everything in a solution. However, we found that it
leads to lots of extra churn.

Excluded Folders
^^^^^^^^^^^^^^^^

In wwwroot, we don't check the compiled or copied files. We also don't
check in the bower\_components folder since that is downloaded. We also
don't check in the node\_modules folder which primarily contains Node
packages used for Gulp and other Node processes.

Currently, the .tfignore file is not functioning in Visual Studio with
ASP.NET Core projects. As a result, these files need to be excluded
manually from source control when checking in.
