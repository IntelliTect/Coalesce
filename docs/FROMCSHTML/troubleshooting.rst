@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Troubleshooting ASP.NET Core, DotNet, Bower, NPM, Gulp, and related tools.
--------------------------------------------------------------------------

Overview
~~~~~~~~

ASP.NET Core RTM was released summer of 2016. While this version if an
offical release, the tooling for it in Visual Studio is not as
streamlined as other parts of .net. As a result there are many rough
edges. There are times when restarting Visual Studio will fix your
problems.

Because DotNet Core is cross platform, all the tooling is designed to
work from the command line. While this may seem like steps backwards,
the command line tools allow much easier application life-cycle
management in areas like deployment and automated builds. Microsoft has
heard the cries of the community that the current process is too painful
and they are working to address the situation. Each set of tooling has
gotten better. However, the 'best practices' have been shifting
significantly. There is also a potential gap between best practice and
what is scaffolded by the Visual Studio new project tooling.

What is DotNet Core
^^^^^^^^^^^^^^^^^^^

DotNet.exe is the cross platform .Net execution environment. It is akin
to the Node or Java runtime. DotNet commands are typically structured
like
``DotNet [module] [Command] [SubCommand] -[option] [option value]``. For
example: ``dotnet ef migrations add MyMigration``

The paths given below are for a standard configuration Windows 10
system. Please modify them as needed for your configuration.

Installing
~~~~~~~~~~

Install the latest version of Visual Studio. DotNet is typically
installed by the .net Core Visual Studio tooling. It can be found at
https://www.microsoft.com/net/core

Once installed, the command ``dotnet`` should run from any command line
prompt.

This troubleshooting guide covers the typical install case. Assuming a
64-bit Windows OS.

NuGet Package Errors
~~~~~~~~~~~~~~~~~~~~

Project.json contains references to .net NuGet packages that are used in
the solution. This is much less frequent with DotNet Core. When packages
don't load try these steps:

-  Right click on your project's references folder and choose Restore
   Packages.
-  Close Visual Studio and reopen your project. On project open, VS will
   restore your packages again.
-  Via the command line, go into your project directory and run the
   command ``dotnet restore``
-  A restore can also be done by changing the project.json file and
   saving it. The project.json.lock file can also be deleted. It will be
   automatically rebuilt.

Packages are downloaded to a package cache in
c:/users/[username]/.nuget/packages. All packages found in your
project.json files are downloaded here as a cache. In some rare cases,
this cache seems to be corrupted and needs to be removed and reloaded
from NuGet sources via a package restore.

If you are getting messages about not being able to restore packages, a
likely cause is that there is not a NuGet feed in Visual Studio that
contains the packages you want to load. This happens if you are using
packages not found on nuget.org and haven't added the additional NuGet
feed. Changing development machines and forgetting to update the NuGet
package sources is a common culprit, especially if you are using the
nightly builds. NuGet feeds can be added to your solution via the
NuGet.config file.

Another common messages is something like 'attempted to get version 5
but ended up with version 2.' This is typically caused by referencing
two different versions of the same NuGet package in the same solution
based on dependency trees. The best way to track this down is to start
at your lowest level project (no dependencies on other projects) and
check the References node in the solution explorer and compare this with
the packages and versions in the project.json file. An additional
feature is that all the dependent packages and their versions are listed
under the References node as well. A package may reference an older or
newer version of a package than what is explicitly specified in your
project.json. Unloading all the projects and reloading them from the
bottom one by one getting them to each compile without dependent
projects.

Bower Component Package Errors
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Bower is a source for web-client libraries, however more users are
moving to npm for all their JavaScript and client libraries. The
packages to be loaded are listed in the bower.json file in your project.
For IntelliTect projects, these Bower packages get stored in the
bower\_compontents folder in your solution. Bower typically contains
source files as well as compiled versions. For example scss and css
files. These source files are often compiled via a Gulp task. The
compiled files are then copied into the wwwroot area of the solution for
deployment again by a Gulp task.

The most common reason for Bower failures is a misconfigured npm.

When Bower packages don't load try these steps:

-  Right click on your project's references folder and choose Restore
   Packages.
-  Close Visual Studio and reopen your project. On project open, VS will
   restore your packages again.
-  Make sure that the .bowerrc file contains
   ``{ "directory": "bower_components" }``. Note that this is not the
   Visual Studio default for new web projects.
-  Clear the Bower cache at ./bower\_components by deleting all the
   folders. Do a package restore per above.
-  A restore can also be done by changing the bower.json file and saving
   it.

Node Packages and Gulp
~~~~~~~~~~~~~~~~~~~~~~

Node is a powerful JavaScript runtime engine that is used in many
projects. Specifically Microsoft has adopted it as a means of running
tools within the development environment via a system called Gulp (Grunt
was the tool of choice a year ago.). Gulp runs on Node and uses external
components via the Node Package Manager (npm). Node gets installed with
Visual Studio and it is not recommended to install it stand alone. Note
that if you have Node installed stand alone, Visual Studio will not use
that version by default, but you can run the new version via the command
line.

Node packages are specified in the package.json file. These packages are
used with Gulp. Note that by default this file may be hidden in your
solution. Use show all files to see it.

If you are having issues with Node, npm or Gulp try these options.

-  Make sure you are using npm version 3 or greater. This installs with
   Visual Studio 2015 update 3. Older versions of npm had very long path
   names which caused lots of issues with Windows because of path length
   restrictions.

   If Gulp tasks in the Task Runner Explorer are not showing correctly,
   the issue is likely that npm didn't restore packages correctly for
   Gulp. If Visual Studio crashes after adding a package try these
   steps. Note that you may need to use rimraf to delete long file
   names.

   -  Close Visual Studio
   -  Delete all folders under the node\_modules in your project folder.
   -  Clear the node cache at C:\\Users\\[user
      name]\\AppData\\Roaming\\npm-cache by deleting all folders.
   -  From the project folder run the following command. If you have
      Node in your path the full name isn't necessary.

      ::

          C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\npm install

-  If you want to run node and npm from any folder via command line you
   must either

   -  Add to your path the location of node and npm:
      ``C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External``
      or
      ``C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External``
      or more recent location.
   -  Download and install node from https://nodejs.org.

-  If you are having trouble getting the Task Runner Explorer to load
   the Gulp file successfully, try the following:

   #. Exit Visual Studio
   #. Download and Install Node
   #. Remove any node modules folders from your application
   #. Restart Visual Studio and reload your project
