---
lang: en-US
title: Introduction
description: Documentation home page for IntelliTect.Coalesce
---

<style>
.ext-logo {
  min-width: 80px;
  max-width: 80px;
  border-radius: 8px;
}
.hero-logo-horiz {
  font-size: calc(20px + max(35px, min(7vw, 70px))) !important; 
  white-space: nowrap; 
  text-align: center; 
  margin-bottom: .4em;
  color: var(--logo-text-color);
}
.hero-logo-horiz img {
  height: 3ex; 
  display: inline-block; 
  vertical-align: middle; 
  padding-bottom: 0.35ex; 
  margin-right: -0.3ex;
}
</style>

<h1 class="hero-logo-horiz">
  <img src=/coalesce-icon-color.svg> Coalesce
</h1>

Designed to help you quickly build amazing web applications, Coalesce is a rapid-development, code generation-based web application framework created by [IntelliTect](https://intellitect.com) and built on top of:

<table>
<tr>
  <td>
    <a href="https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core"><img class=ext-logo src=/net-logo.svg></a>
  </td>
  <td>C#, .NET, and ASP.NET Core are the backend foundation of all Coalesce applications.</td>
</tr>
<tr>
  <td><a href="https://learn.microsoft.com/en-us/ef/core/"><img class=ext-logo src=/ef-logo.svg></a></td>
  <td>Entity Framework provides the interface between your database and C# code. Coalesce will use your EF data model to generate an extensible, customizable CRUD API that will drive both your custom pages and the out-of-the-box admin pages.</td>
</tr>
<tr>
  <td><a href="https://www.typescriptlang.org/"><img class=ext-logo src=/ts-logo-512.svg></a></td>
  <td>TypeScript enables discovery of Coalesce features through Intellisense and provides confidence that your frontend code won't break as your application grows.</td>
</tr>
<tr>
  <td><a href="https://vuejs.org/"><img class=ext-logo src=/vue-logo.svg></a></td>
  <td>Vue.js provides data binding and interactivity for your application as it runs in a browser. Coalesce will generate <a href="./stacks/vue/layers/viewmodels.html">TypeScript ViewModels</a> to facilitate rapid development of custom pages. </td>
</tr>
<tr>
  <td><a href="https://vitejs.dev/"><img class=ext-logo src=/vite-logo.svg></a></td>
  <td>Vite is the development and build tooling for your frontend Vue code, enabling lightning-fast single-page application development. Coalesce integrates ASP.NET Core and Vite together, streamlining local development to require nothing more than a <code>dotnet run</code> or a single-click launch in your IDE.</td>
</tr>
</table>

## What do I do?

You are responsible for the interesting parts of your application:

- Data Model
- Business Logic
- External Integrations
- Page Content
- Site Design
- Custom Scripting

## What is done for me?

Coalesce builds the part of your application that are mundane and
monotonous to build:

- Client side [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md) that mirror your data model for both lists and individual objects. Utilize these to rapidly build out your application's various pages.
- APIs to interact with your models via endpoints like List, Get, Save, and more.
- Out-of-the-box [Vue Components](/stacks/vue/coalesce-vue-vuetify/overview.md) for common controls like dates, selecting objects via drop downs, enums, etc. Dropdowns support searching and paging automatically.
- A complete set of admin pages are provided, allowing you to read, create, edit, and delete data straight away without writing any additional code.

## Getting Started

To get started with Coalesce, check out [Getting Started with Vue](/stacks/vue/getting-started.md).

The legacy Knockout.js stack for Coalesce is no longer supported, and will be removed in a future release.
