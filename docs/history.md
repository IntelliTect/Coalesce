
# Welcome to Coalesce's documentation!

Coalesce is a framework based on ASP.NET Core that makes rapidly building awesome web applications much easier. A project that would take 3 months to complete now takes 1 month. We built this because we got tired of writing all the boiler plate code that is necessary to make amazing applications. 

It does this by allowing developers to focus on the creative aspects of the solution. The more mundane parts are generated automatically. This means that you get to focus on data modeling, business logic and front-end development. Coalesce does the plumbing.

Here is a typical workflow
   1. Build an EF Core data model with business logic
   2. Coalesce generates controllers, TypeScript view models, API and view model documentation, and admin pages/examples
   3. Build an interactive and intuitive user experience
   4. Rinse and repeat


## Core Features
   * Built on the latest Microsoft ASP.NET Core
   * Easy to learn
   * TypeScript from the ground up
   * Flexibility to use MVC patterns as required
   * Admin pages for all your models are build automatically and include features like searching, sorting, and paging
   * Robust documentation for the framework
   * Automatically generated documentation for the API layer and TypeScript view models
   * Feature rich TypeScript view models that can be easily extended
   * Many extension points for customizations
   * Abstraction that doesn't require you know how everything works
   * Security and data trimming by role is built in
   * Flexibility about which data to return to the client
   * Open source


## Is Coalesce for Everyone and Every Project?

Coalesce was designed to create line-of-business applications. It provides a more customizable and maintainable alternative to off the shelf customizable products like SharePoint and Sales Force. 

You should consider using Coalesce if your project:
   * Is small to medium size (1-200 classes)
   * Requires an interactive user experience
   * Has data entry requirements, especially forms, tables, etc.
   * Needs to get started quickly with functional prototypes that can become production software


## Design Decisions and Limitations

Coalesce is specifically designed to meet the needs of web developers. However, there are lots of ways to do this. We have made a set of decisions which we believe makes for a great development experience

   * ASP.NET Core: there is no intent to back port this to an earlier version
   * EF Core for the object relational mapper
   * Currently uses the full framework because .NET Core doesn't supported the required functionality, yet
   * Knockout for client-side data binding 
   * Business logic most easily lives in the model classes
   * Coalesce is designed for relational databases. This might change in the future, but not until we have a compelling use case.


## How Does it Work?

After you create your classes and the EF data context, Coalesce uses this information to generate code. When the Coalesce CLI (command line interface) is run, the following things happen:

   1. The model is validated to ensure that all the Coalesce specific requirements are met. This includes things like ensuring that all classes have a primary key assigned, validating that linked child objects have a key to their parent, etc. If issues are found, generation stops and the errors are displayed with advice to fix the issues.
   2. The core files needed for Coalesce are copied to the target project. This includes TypeScript base classes, customizable templates, and other files for extension points. Each file is copied twice, once as a file that can be modified in the project and once as an original file. This ensures that if any changes are made by the user these files Coalesce will not overwrite your changes.
   3. The API controllers are generated. One is generated for each object. This includes methods that get a list of items, get a specific item, save an item, etc. 
   4. The TypeScript view models are created. There are two view models for each object. One is a list view model which allows for getting and displaying lists of a type of object. This includes full functionality to sort, filter, search, page, etc. Additionally, a view model that represents the individual object is also created. This has all the properties and methods of the server side object. This is basically a client-side proxy object for representing and manipulating the object on the server side. These objects seamlessly use the API controllers to interact with the server. 
   5. Next, the View controllers are created. One is created for each model class and provides a tabular view, a card view (for mobile), an editor, and documentation.
   6. Finally, the CSHTML views for the controller are created. These are the actual CSHTML for the above controllers. These not only provide administrative view and editing features, but also serve as an example of how to use the framework


## General Guidance

Here are a few things we have found helpful when using Coalesce

   * Learn and embrace the Coalesce paradigm and work with it rather than trying to do things another way.
   * Following what we refer to as the 'well worn path' is very helpful. Try to stick to standard ways to do things rather than trying to use esoteric features.
   * Keep your models as consistent and straightforward as possible. Use relational modeling best practices.
   * Remember that public methods on your class models are added to the client side view models and this makes calling business logic from the client really easy.
   * Don't be afraid to fall back to building parts of your site using traditional methods. Coalesce isn't right for everything. But, honestly, we have only done this a few times, like 3. 


# The Story

## Why Coalesce

In 2014 several developers from IntelliTect got together to talk about our craft. There were lots of different backgrounds, but recently we had all been writing web code in C#. We discussed things we enjoyed and things we dreaded. There was an underlying commitment to providing customers with great sites at a reasonable cost. However, those things often seemed at odds because of the complexity of web development.

## The Problem

For example, writing AJAX drop down lists with type ahead takes quite a bit of plumbing. Layer onto this the need for view models that allow for validation and saving as the user moves from field to field. We absolutely want to deliver visually pleasing sites with complex UI paradigms. However, all this excellence adds up: complex view models, complex APIs, data binding, ugh. 

Then there is that sinking feeling when you have to add another class to the project knowing that you are going to need to create all this yet again and you consider taking short cuts. Will there really be more than about 20 items in this table, maybe we don't need paging. Inevitably, the customer asks for admin screens. We consider giving them SQL Server Management Console and then consider using the built in ASP.NET list and editor pages. Better sense wins out and we end up spending two weeks building slick admin pages with paging, searching, sorting, etc. 


## The Path to the Solution

That evening we starting talking about the things we loved to do:
  * Data modeling
  * Figuring out and writing business logic
  * Working with customers
  * Making cool user interfaces
  * Creating something new and awesome

We also lists things that we didn't enjoy
  * Writing the same controller again
  * Creating a view model for a class that is similar but different from another one in the project
  * Putting sorting and paging on every admin page
  * Basically doing anything that feels repetitive or boilerplate

Over the next few months we talked about this issue, but couldn't find the right abstraction. We talked about other solutions that solve parts of the problem and considered putting together something from several pieces. Nothing felt unified and we ended up with leaky abstractions. We needed some way to divide the problem so that we could build the fun stuff and have something generate the boring stuff. This solution needed to be robust enough to satisfy our customer's needs and also be of use to developers without their needing to know the inner workings of the system.


## Our Solution

What if we could build the models and business logic and have a tool build everything except the UI? There are great tools like Entity Framework for modeling and good tooling for minimizing duplicate code in user interfaces. And so Coalesce was born, a tool that would bring together the back-end and front-end development.

Coalesce takes Entity Framework Core models and builds controllers, TypeScript view models, and admin pages automatically. These are built in a general way so that they can be applied to many different scenarios. There will always be pages that need to be written by hand and we intentionally don't support many edge cases in order to keep things simple. There is nothing wrong with building something by hand. 


## How has it Worked?

We have been using Coalesce for many of our web projects with great success. Typically, a project is taking about 1/3 the time it was taking before once developers ramp up. The ramp up on Coalesce has typically been a couple of days. We realized that in order for Coalesce to be useful it need to be intuitive to use and easy to understand. We have intentionally used simple paradigms to minimize the learning curve. There are complex bits, but hopefully, those are well hidden and documented as needed.

