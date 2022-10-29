
# Getting Started with Vue

## Creating a Project

The quickest and easiest way to create a new Coalesce Vue application is to use the ``dotnet new`` template. In your favorite shell:
    
``` sh
mkdir MyCompany.MyProject
cd MyCompany.MyProject
dotnet new --install IntelliTect.Coalesce.Vue.Template
dotnet new coalescevue
cd *.Web
npm ci
```

[![](https://img.shields.io/nuget/v/IntelliTect.Coalesce.Vue.Template)](https://www.nuget.org/packages/IntelliTect.Coalesce.Vue.Template/) • [View on GitHub](https://github.com/IntelliTect/Coalesce.Vue.Template) 

## Project Structure

::: tip Important
The Vue template is based on [Vite](https://vitejs.dev/). You are strongly encouraged to read through at least the first few pages of the [Vite Documentation](https://vitejs.dev/guide/) before getting started on any development.
:::

The structure of the Web project follows the conventions of both ASP.NET Core and Vite. The Vue-specific folders are as follows:

- ``/src`` - Files that should be compiled into your application. CSS/SCSS, TypeScript, Vue SFCs, and so on.
- ``/public`` - Static assets that should be served as files. Includes index.html, the root document of the application.
- ``/tests`` - Jest unit tests.
- ``/wwwroot`` - Target for compiled output.

During development, no special tooling is required to build your frontend code. Coalesce's ``UseViteDevelopmentServer`` in ASP.NET Core will take care of that automatically when the application starts. Just make sure NPM packages have been installed (`npm ci`).

::: tip
If developing with Visual Studio, you are strongly encouraged to disable Visual Studio's built-in automatic NPM package restore functionality (``Options > Projects and Solutions > Web Package Management > Package Restore``). 

This feature of Visual Studio fails to respect your ``package.lock.json`` file, and the version of NPM that Visual Studio comes with tends to be quite old and will behave differently from the ``npm`` on your system's $PATH.

You should manually restore your packages with ``npm ci`` (when you haven't tried to change any versions) or ``npm i`` (when installing new packages or upgrading versions).
:::

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

We can create a details page for a Person by creating a [Single File Component](https://vuejs.org/v2/guide/single-file-components.html) in ``MyApplication.Web/src/views/person-details.vue``:

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

<script lang="ts"> 
import { Vue, Component, Watch, Prop } from "vue-property-decorator";
import { PersonViewModel } from "@/viewmodels.g";

@Component({})
export default class extends Vue {
  @Prop({ required: true, type: Number })
  id!: number;

  person = new PersonViewModel();

  created() {
    this.person.$load(this.id);
  }
}
</script>
```

::: tip Note
In the code above, [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) is a component that comes from the [Vuetify Components](/stacks/vue/coalesce-vue-vuetify/overview.md) for Coalesce.

For simple property types like `string` and `number` you can always use simple template interpolation syntax, but for more complex properties like dates, [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) is handy to use because it includes features like built-in date formatting.
:::


::: tip
The code above uses [vue-class-component](https://class-component.vuejs.org/) and [vue-property-decorator](https://github.com/kaorun343/vue-property-decorator) to define the component.

These libraries provide an alternative to the default component declaration syntax in [Vue](https://vuejs.org/). However, you must be aware of the [Caveats](https://class-component.vuejs.org/guide/caveats.html) if you want to use these tools to build your own class-style components.
:::

We then need to add route to this new view. In ``MyApplication.Web/src/router.ts``, add a new item to the `routes` array:

``` ts
// At the top of the file, import the component:
import PersonDetails from '@/views/person-details.vue';
```

``` ts
// In the `routes` array, add the following item:
{
  path: '/person/:id',
  name: 'person-details',
  component: PersonDetails,
  props: route => ({ id: +route.params.id }),
},
```

With these pieces in place, we now have a functioning page that will display details about a person. We can start up the application (or, if it was already running, refresh the page) and navigate to ``/person/1`` (assuming a person with ID 1 exists - if not, navigate to ``/admin/Person`` and create one).

From this point, you can start adding more fields, more features, and more flair to the page. Check out all the other documentation in the sidebar to see what else Coalesce has to offer, including the [Vue Overview](/stacks/vue/overview.md).