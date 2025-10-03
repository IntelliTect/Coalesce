# Getting Started

## Environment Setup

Before you begin, ensure that you have all the required tools installed:

- Recent version of the [.NET SDK](https://dotnet.microsoft.com/en-us/download). If you have Visual Studio, you already have this.
- A recent version of [Node.js](https://nodejs.org/) (an LTS version is recommended).
- A compatible IDE
  - Recommended:
    - Visual Studio for backend (C#) development
    - VS Code for frontend (Vue, TypeScript) development (with [Vue - Official](https://marketplace.visualstudio.com/items?itemName=Vue.volar))
  - Alternatively, you could use any of these:
    - VS Code for full stack development
    - JetBrains Rider

## Creating a Project

The quickest and easiest way to create a new Coalesce Vue application is to use the `dotnet new` template.

First, select the features that you would like included in your project, and choose the root .NET namespace of your project:

<script setup>
import TemplateBuilder from './TemplateBuilder.vue'
import { ref, computed} from 'vue'
const templateParams = ref("")
const namespace = ref("")
const effectiveNamespace = computed(() => namespace.value?.replace(/\.+$/, '') || 'MyCompany.MyProject')
const effectiveFolder = computed(() => effectiveNamespace.value.split('.').at(-1))
function copyCode() {
  document.querySelector(".template-code .copy").click()
}
</script>

<TemplateBuilder v-model:options="templateParams" v-model:namespace="namespace" />

Next, click the button or manually copy the commands below into your favorite terminal, and execute them! This will create a root folder named <code>{{effectiveFolder}}</code> - execute the script in your `sources`/`repos`/etc folder.

<button @click="copyCode()" style="
color: var(--vp-button-brand-text);
background-color: var(--vp-button-brand-bg);
border-radius: 20px;
padding: 0 20px;
line-height: 38px;
font-size: 14px;
display: inline-block;
margin: auto;
display: block;
font-weight: 600;
">Copy CLI Commands</button>

<style>
  .template-code .copy { opacity: 1 !important }
</style>
<div class="template-code">

```sh-vue
dotnet new install IntelliTect.Coalesce.Vue.Template
dotnet new coalescevue -n {{effectiveNamespace}} -o {{effectiveFolder}} {{templateParams}}
cd {{effectiveFolder}}/*.Web
npm i
npm run lint:fix
dotnet restore
dotnet coalesce

```

</div>

You now have a new Coalesce project! For the recommended development experience, open the `.Web` project in VS Code and open the root `.slnx` file in Visual Studio.

If any of the options you chose above require external integrations, you'll need to configure those - follow the instructions for each section that have been placed into `appsettings.json`.

## Project Structure

### AppHost Project

The AppHost project is a [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) dev-time orchestration project. It is recommended you set this as your startup project when developing, although this is not strictly required if your project only has SQL Server as its sole dependency. If you're not familiar with .NET Aspire, you're strongly encouraged to read through its [overview documentation](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) and learn about the features of the [Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/explore).

### Data Project

The data project contains all your [entity models](/modeling/model-types/entities.md), [services](/modeling/model-types/services.md), and most other custom backend code that you'll write while building your application. The code within it acts as the inputs to Coalesce's code generation, which outputs generated files into the Web project.

### Web Project

The Web project is an ASP.NET Core application where the generated outputs from Coalesce are placed. It's also where you'll build your rich front-end pages that users will use to interact with your application.

The structure of the Web project follows the conventions of both ASP.NET Core and Vite. Some specific files and folders are as follows:

- `/src` - Files that should be compiled into your frontend application. CSS/SCSS, TypeScript, Vue SFCs, and so on.
- `/public` - Static assets that should be served directly as files.
- `/wwwroot` - Target for Vite's compiled output. This directory is excluded from git.
- `/Program.cs` - Entry point for your ASP.NET Core web application.
- `/ProgramServiceDefaults.cs` - [.NET Aspire Service Defaults configuration](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults), including Open Telemetry configuration, health checks, and resiliency configuration. Since Coalesce projects typically only have one .NET service (the web application), this file is not in a separate project. If your solution grows beyond that, you're encouraged to split this out of the Web project.
- `/Controllers/HomeController.cs` - Controller that serves the root page of your Vue SPA, both in development and production. Some customizations can be added here.

::: tip Important
The frontend build system uses [Vite](https://vitejs.dev/). You are strongly encouraged to read through at least the first few pages of the [Vite Documentation](https://vitejs.dev/guide/) before getting started on any development.
:::

During development, no special effort is required to build your frontend code. Coalesce's `UseViteDevelopmentServer` in ASP.NET Core will take care of that automatically when the application starts. Just make sure NPM packages have been installed (`npm ci`). For more details on how the Vite integration works, see [Vite Integration](/topics/vite-integration.md).

@[import-md "after":"MARKER:data-modeling", "before":"MARKER:data-modeling-end"](../agnostic/getting-started-modeling.md)

## Building Pages & Features

Let's say we've created a [model](/modeling/model-types/entities.md) called `Person` as follows, and we've ran code generation with `dotnet coalesce`:

```c#
namespace MyApplication.Data.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
    }
}
```

We can create a details page for a Person by creating a [Single File Component](https://vuejs.org/guide/scaling-up/sfc.html) in `MyApplication.Web/src/views/person-details.vue`:

```vue
<template>
  <dl>
    <dt>Name</dt>
    <dd>
      <c-display :model="person" for="name" />
    </dd>

    <dt>Date of Birth</dt>
    <dd>
      <c-display :model="person" for="birthDate" format="M/d/yyyy" />
    </dd>
  </dl>
</template>

<script setup lang="ts">
import { PersonViewModel } from "@/viewmodels.g";

const props = defineProps<{ id: number }>();
const person = new PersonViewModel();

person.$load(props.id);
</script>
```

::: tip Note
In the code above, [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) is a component that comes from the [Vuetify Components](/stacks/vue/coalesce-vue-vuetify/overview.md) for Coalesce.

For simple property types like `string` and `number` you can always use simple template interpolation syntax, but for more complex properties like dates, [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) is handy to use because it includes features like built-in date formatting.
:::

We then need to add a route to this new view. In `MyApplication.Web/src/router.ts`, add a new item to the `routes` array:

```ts
// In the `routes` array, add the following item:
{
  path: '/person/:id',
  name: 'person-details',
  component: () => import('@/views/person-details.vue'),
  props: route => ({ id: +route.params.id }),
},
```

With these pieces in place, we now have a functioning page that will display details about a person. We can start up the application (or, if it was already running, refresh the page) and navigate to `/person/1` (assuming a person with ID 1 exists - if not, navigate to `/admin/Person` and create one).

From this point, you can start adding more fields, more features, and more flair to the page. Check out all the other documentation in the sidebar to see what else Coalesce has to offer, including the [Vue Overview](/stacks/vue/overview.md).
