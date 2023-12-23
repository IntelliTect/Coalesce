
# Getting Started

## Creating a Project

The quickest and easiest way to create a new Coalesce Vue application is to use the ``dotnet new`` template. In your favorite shell:
    
``` sh
dotnet new install IntelliTect.Coalesce.Vue.Template
dotnet new coalescevue -o MyCompany.MyProject
cd *.Web
npm ci
```

<div style="display:flex">
<a href="https://www.nuget.org/packages/IntelliTect.Coalesce.Vue.Template/" target="_blank" rel="noreferrer"><img src="https://img.shields.io/nuget/v/IntelliTect.Coalesce.Vue.Template?logo=nuget&color=0176b5" alt=""></a>
&nbsp;
<a href="https://github.com/IntelliTect/Coalesce.Vue.Template" target="_blank" rel="noreferrer"><img src="https://img.shields.io/badge/Github-Coalesce.Vue.Template-0176b5?logo=github" alt="Static Badge"></a> 
</div>

## Project Structure

::: tip Important
The Vue template is based on [Vite](https://vitejs.dev/). You are strongly encouraged to read through at least the first few pages of the [Vite Documentation](https://vitejs.dev/guide/) before getting started on any development.
:::

The structure of the Web project follows the conventions of both ASP.NET Core and Vite. The Vue-specific folders are as follows:

- ``/src`` - Files that should be compiled into your application. CSS/SCSS, TypeScript, Vue SFCs, and so on.
- ``/public`` - Static assets that should be served as files. Includes index.html, the root document of the application.
- ``/wwwroot`` - Target for compiled output.

During development, no special tooling is required to build your frontend code. Coalesce's ``UseViteDevelopmentServer`` in ASP.NET Core will take care of that automatically when the application starts. Just make sure NPM packages have been installed (`npm ci`).

@[import-md "after":"MARKER:data-modeling", "before":"MARKER:data-modeling-end"](../agnostic/getting-started-modeling.md)


## Building Pages & Features

Lets say we've created a [model](/modeling/model-types/entities.md) called `Person` as follows, and we've ran code generation with ``dotnet coalesce``:

``` c#
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

We can create a details page for a Person by creating a [Single File Component](https://vuejs.org/guide/scaling-up/sfc.html) in ``MyApplication.Web/src/views/person-details.vue``:

``` vue
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


We then need to add route to this new view. In ``MyApplication.Web/src/router.ts``, add a new item to the `routes` array:

``` ts
// In the `routes` array, add the following item:
{
  path: '/person/:id',
  name: 'person-details',
  component: () => import('@/views/person-details.vue'),
  props: route => ({ id: +route.params.id }),
},
```

With these pieces in place, we now have a functioning page that will display details about a person. We can start up the application (or, if it was already running, refresh the page) and navigate to ``/person/1`` (assuming a person with ID 1 exists - if not, navigate to ``/admin/Person`` and create one).

From this point, you can start adding more fields, more features, and more flair to the page. Check out all the other documentation in the sidebar to see what else Coalesce has to offer, including the [Vue Overview](/stacks/vue/overview.md).
