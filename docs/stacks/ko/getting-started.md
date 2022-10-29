# Getting Started with Knockout

## Creating a Project

::: warning
The Coalesce Knockout.js stack is deprecated and will be receiving only critical bug fixes going forward. You are strongly encouraged to start all new Coalesce projects with [Vue](../vue/getting-started.md).
:::

The quickest and easiest way to create a new Coalesce Knockout application is to use the ``dotnet new`` template. In your favorite shell:

``` sh
dotnet new --install IntelliTect.Coalesce.KnockoutJS.Template
dotnet new coalesceko
```

[![](https://img.shields.io/nuget/v/IntelliTect.Coalesce.KnockoutJS.Template)](https://www.nuget.org/packages/IntelliTect.Coalesce.KnockoutJS.Template/) • [View on GitHub](https://github.com/IntelliTect/Coalesce.KnockoutJS.Template) 

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

We can create a details page for a Person by creating:

- A controller in ``src/MyApplication.Web/Controllers/PersonController.cs``:

    ``` c#
    namespace MyApplication.Web.Controllers
    {
        public partial class PersonController
        {
            public IActionResult Details() => View();
        }
    }
    ```

- A view in ``src/MyApplication.Web/Views/Person/Details.cshtml``:

    ``` razor
    <h1>Person Details</h1>

    <div data-bind="with: person">
        <dl class="dl-horizontal">
            <dt>Name </dt>
            <dd data-bind="text: name"></dd>

            <dt>Date of Birth </dt>
            <dd data-bind="moment: birthDate, format: 'MM/DD/YYYY hh:mm a'"></dd>
        </dl>
    </div>

    @section Scripts
    {
    <script src="~/js/person.details.js"></script>
    <script>
        $(function () {
            var vm = new MyApplication.PersonDetails();
            ko.applyBindings(vm);
            vm.load();
        });
    </script>
    }
    ```

- And a script in ``src/MyApplication.Web/Scripts/person.details.ts``:

    ``` ts
    /// <reference path="viewmodels.generated.d.ts" />

    module MyApplication {
        export class PersonDetails {
            public person = new ViewModels.Person();

            load() {
                var id = Coalesce.Utilities.GetUrlParameter("id");
                if (id != null && id != '') {
                    this.person.load(id);
                }
            }
        }
    }
    ```

With these pieces in place, we now have a functioning page that will display details about a person. We can start up the application and navigate to ``/Person/Details?id=1`` (assuming a person with ID 1 exists - if not, navigate to ``/Person/Table`` and create one).

From this point, one can start adding more fields, more features, and more flair to the page. Check out all the other documentation in the sidebar to see what else Coalesce has to offer, including the [Knockout Overview](/stacks/ko/overview.md).