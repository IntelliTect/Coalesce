---
lang: en-US
title: Coalesce Documentation
description: Documentation home page for IntelliTect.Coalesce
---

.. _IntelliTect: https://intellitect.com

Coalesce
========

Designed to help you quickly build amazing web applications, Coalesce is a rapid-development code generation framework, created by IntelliTect_ and built on top of:

- ASP.NET Core
- EF Core
- TypeScript
- [Vue](https://vuejs.org/) or [Knockout](http://knockoutjs.com/)


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

- Client side [TypeScript ViewModels](/stacks/disambiguation/view-model.md) for either [Vue](https://vuejs.org/) or [Knockout](http://knockoutjs.com/) that mirror your data model for both [lists](/stacks/disambiguation/list-view-model.md) and [individual objects](/stacks/disambiguation/view-model.md). Utilize these to rapidly build out your applications various pages.
- APIs to interact with your models via endpoints like List, Get, Save, and more.
- Out-of-the-box [Vue Components](/stacks/vue/coalesce-vue-vuetify/overview.md) or [Knockout bindings](/stacks/ko/client/bindings.md) for common controls like dates, selecting objects via drop downs, enums, etc. Dropdowns support searching and paging automatically.
-  A complete set of admin pages are provided, allowing you to read, create, edit, and delete data straight away without writing any additional code.


Getting Started
===============

To get started with Coalesce, you first must choose which front-end stack you wish to use - [Vue](https://vuejs.org/), or [Knockout](http://knockoutjs.com/). While the [Knockout](http://knockoutjs.com/) stack is still fully supported, the [Vue](https://vuejs.org/) stack is the newer, more modern stack and [Vue](https://vuejs.org/) itself has a much bigger worldwide community and ecosystem of libraries, components, plugins, support, and more. The [Vue](https://vuejs.org/)-based stack is the one that will be receiving the bulk of development effort in Coalesce going forward. 

If you still need help choosing, check out the overviews for each stack:

- [Vue Overview](/stacks/vue/overview.md)
- [Knockout Overview](/stacks/ko/overview.md)

Once you've made your decision, check out one of the two links below:

- [Getting Started with Vue](/stacks/vue/getting-started.md)
- [Getting Started with Knockout](/stacks/ko/getting-started.md)

