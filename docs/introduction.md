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

Coalesce is a full-stack meta-framework for ASP.NET Core and Vue.js, created by [IntelliTect](https://intellitect.com). You write your data models, business logic, and UI pages — Coalesce generates the API layer, TypeScript clients, ViewModels, and admin UI that connect them.

It is built on top of:

<table>
<tbody>
<tr>
  <td>
    <a href="https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core"><img class=ext-logo src=/net-logo.svg></a>
  </td>
  <td>C#, .NET, and ASP.NET Core are the backend foundation of all Coalesce applications.</td>
</tr>
<tr>
  <td><a href="https://learn.microsoft.com/en-us/ef/core/"><img class=ext-logo src=/ef-logo.svg></a></td>
  <td>Entity Framework provides the interface between your database and C# code. Coalesce uses your EF data model to generate an extensible, customizable CRUD API that drives both your custom pages and the out-of-the-box <a href="./stacks/vue/admin-pages.html">admin pages</a>.</td>
</tr>
<tr>
  <td><a href="https://www.typescriptlang.org/"><img class=ext-logo src=/ts-logo-512.svg></a></td>
  <td>TypeScript enables discovery of Coalesce features through Intellisense and provides confidence that your frontend code won't break as your application grows.</td>
</tr>
<tr>
  <td><a href="https://vuejs.org/"><img class=ext-logo src=/vue-logo.svg></a></td>
  <td>Vue.js provides data binding and interactivity for your application as it runs in a browser. Coalesce generates <a href="./stacks/vue/layers/viewmodels.html">TypeScript ViewModels</a> to facilitate rapid development of custom pages.</td>
</tr>
<tr>
  <td><a href="https://vitejs.dev/"><img class=ext-logo src=/vite-logo.svg></a></td>
  <td>Vite is the development and build tooling for your frontend Vue code, enabling lightning-fast single-page application development. <a href="./topics/vite-integration">Coalesce integrates Vite</a> with ASP.NET Core, streamlining local development to require nothing more than a <code>dotnet run</code> or a single-click launch in your IDE.</td>
</tr>
</tbody>
</table>

## What you build

You are responsible for the interesting parts of your application:

- **Data models** — EF Core entities and relationships
- **Business logic** — services, custom methods, integrations
- **UI pages** — Vue components, layouts, and custom experiences

## What Coalesce generates

Coalesce builds the parts of your application that are mundane and monotonous to build:

- [**API controllers & DTOs**](/modeling/model-types/entities.md) — Full CRUD endpoints for every entity and service, with request/response DTOs that enforce security trimming automatically.
- [**TypeScript ViewModels**](/stacks/vue/layers/viewmodels.md) — Reactive objects with auto-save, bulk saves, dirty tracking, loading states, and validation rules flowing from your C# attributes.
- [**Vue components**](/stacks/vue/coalesce-vue-vuetify/overview.md) — Data-driven inputs, dropdowns with search and paging, date pickers, tables, and more.
- [**Admin pages**](/stacks/vue/admin-pages.md) — Full CRUD UI out of the box, no code required.
- [**Security enforcement**](/topics/security.md) — Role-based, row-level, and property-level security declared in C# and enforced on every API response.
- <Beta/> [**Semantic Kernel plugins**](/modeling/model-components/attributes/semantic-kernel.md) — AI-powered interaction with your application through LLM-based tools.

## Getting Started

To get started with Coalesce, check out [Getting Started](/stacks/vue/getting-started.md).
