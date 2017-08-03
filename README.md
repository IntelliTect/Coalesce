# Coalesce
Designed to help you quickly build amazing sites, Coalesce is a rapid-development code generation framework for 
  * ASP.NET Core
  * MVC Core
  * EF Core
  * TypeScript
  * Visual Studio 2017

Branch 2.0: [![Build status](https://ci.appveyor.com/api/projects/status/mev829igrspj4x5s/branch/2.0?svg=true)](https://ci.appveyor.com/project/IntelliTect/coalesce/branch/2.0)

Branch master: [![Build status](https://ci.appveyor.com/api/projects/status/mev829igrspj4x5s/branch/master?svg=true)](https://ci.appveyor.com/project/IntelliTect/coalesce/branch/master)

## Bottom Line
Projects that used to take us a three weeks now take one week. 

## Learn More
You can find a sample app with documentation here: http://coalesce.intellitect.com/

A video walkthrough will be available soon.

## Get Started Fast
We recommend using our starter project at at: https://github.com/IntelliTect/Coalesce.Starter

This project gives you everything you need to start building an amazing web site quickly. 


## What does Coalesce do for Me?
### Coalesce lets you do the fun stuff like 
* Designing data models
* Building business logic
* Crafting awesome web front ends

### Coalesce automates laborious laborious tasks
* Generating a robust API layer with searching, sorting, filtering, paging, etc. 
* Generating easy to use View Models for easy data binding on the client side using Knockout
* Pulling all (or most of) the data for your page in a single AJAX call. This includes the ability to gather the correct children.
* Server and client side validation
* Server side API trimming and secrurity
* Nice admin pages that you wouldn't be embarrased showing to a customer for internal use
* Exposing C# properties and methods in the client using TypeScript with simple data binding


## Why Coalesce
Coalesce was created when developers at IntelliTect, after writing many web sites, got bored. We got bored of writing really awesome API and View Model code because making this code awesome takes lots of work. However, it is boring because, while not all the same, it ended up being very repetitive. Supporting paging, sorting, searching, data binding, security, etc. for every type of object is laborious. We also really disliked the auto generated MVC CRUD pages and thought users had the right to a better admin experience. 

Specifically, we had a large line-of-business applicaiton to write which had a SPA page with over 300 fields on it. The thought of writing this by hand was just too daunting especially when the page needed to be saved as the edit were made. 

With Coalesce, every time we found a great thing to add to our APIs, view models or admin pages, we could just add it once and it was immediately available in all our classes. 

Just as a note, we have tried to make Coalesce really easy to pick up and use. So you will find many conventions that can be overridden by configuration. For example, if you have a property named Name, it will automatically be searchable. 


## Design Methodology
Being early adopters of ASP.NET 5/Core, we took cues from the Microsoft team to limit the cognative load of using Coalesce. Here are some of the guiding tenants.
* Writing code should be really fun
* I want to spend my time doing things that require thought and creativity not turning the crank
* Keep Coalesce focused on solving the task at hand and resist adding the ability to send email
* Favor using convention over configuration so things just work, but always allow for configuration overrides
* Use things like annotations and use the ones from products like EF seamlessly. For example for client side validation.
* Make sure that the developer experience, especially with IntelliSense, is awesome


### Knockout
Knockout is the default data binding mechanism for Coalesce. After using Knockout for the past few years, we really love it. It is a robust and well supported framework. Check out the amazing docs and tutorials here: http://knockoutjs.com/  Thank you to Steve Sanderson.

At the core, all Coalesce really needs is a data binding mechanism. While Knockout is not the newest and shiniest thing on the block it is a very robust data-binding framework which is exactly what we need. 

We have had great success with what we term hybrid-SPA applications. This is an application where which has several main pages which load via MVC, but once on the page all the interaction is done via AJAX. 

With this in mind, we have tried to build Coalesce so that another framework could be easily added at a later date. 

Currently, we are pondering support for the latest version of Angular, however some of us have feeling toward Vue.js and Aurelia. Input and of course pull requests are welcome. 
