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
- Vue_ or Knockout_


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

- Client side :ref:`TypeScriptViewModels` for either Vue_ or Knockout_ that mirror your data model for both :ref:`lists <TypeScriptListViewModels>` and :ref:`individual objects <TypeScriptViewModels>`. Utilize these to rapidly build out your applications various pages.
- APIs to interact with your models via endpoints like List, Get, Save, and more.
- Out-of-the-box :ref:`Vue Components <VuetifyOverview>` or :ref:`Knockout bindings <KnockoutBindings>` for common controls like dates, selecting objects via drop downs, enums, etc. Dropdowns support searching and paging automatically.
-  A complete set of admin pages are provided, allowing you to read, create, edit, and delete data straight away without writing any additional code.


Getting Started
===============

To get started with Coalesce, you first must choose which front-end stack you wish to use - Vue_, or Knockout_. While the Knockout_ stack is still fully supported, the Vue_ stack is the newer, more modern stack and Vue_ itself has a much bigger worldwide community and ecosystem of libraries, components, plugins, support, and more. The Vue_-based stack is the one that will be receiving the bulk of development effort in Coalesce going forward. 

If you still need help choosing, check out the overviews for each stack:

- [Vue Overview](./stacks/vue/overview.md)
- :ref:`KoOverview`

Once you've made your decision, check out one of the two links below:

- :ref:`VueGettingStarted`
- :ref:`KoGettingStarted`


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
    pages/modeling/model-types/services

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
    :caption: Server-side Generated Code

    pages/stacks/agnostic/generation
    pages/stacks/agnostic/dtos

.. toctree::
    :hidden:
    :maxdepth: 1
    :titlesonly:
    :caption: Client - Vue

    pages/stacks/vue/overview
    pages/stacks/vue/getting-started
    pages/stacks/vue/layers/metadata
    pages/stacks/vue/layers/models
    pages/stacks/vue/layers/api-clients
    pages/stacks/vue/layers/viewmodels
    pages/stacks/vue/coalesce-vue-vuetify/overview

.. toctree::
    :hidden:
    :maxdepth: 3
    :titlesonly:
    :caption: Client - Knockout

    pages/stacks/ko/overview
    pages/stacks/ko/getting-started
    pages/stacks/ko/client/view-model
    pages/stacks/ko/client/list-view-model
    pages/stacks/ko/client/external-view-model
    pages/stacks/ko/client/methods
    pages/stacks/ko/client/model-config
    pages/stacks/ko/client/bindings

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

