# EF Entity Models

Models are the core business objects of your application - they serve as the fundamental representation of data in your application. The design of your models is very important. In [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) (EF), data models are just Plain Old CLR Objects (POCOs).

## Building a Data Model

To start building your data model that Coalesce will generate code for, follow the best practices for [EF Core](https://docs.microsoft.com/en-us/ef/core/). Guidance on this topic is available in abundance in the [Entity Framework Core documentation](https://docs.microsoft.com/en-us/ef/core/).

Don't worry about querying or saving data when you're just getting started - Coalesce will provide much of that functionality for you, and it's very easy to customize what Coalesce offers later. To get started, just build your entity classes and `DbContext` class. Annotate your `DbContext` class with `[Coalesce]` so that Coalesce will discover it and generate code based on your context. Also ensure that each entity model has a `DbSet` property on the context - this is how Coalesce discovers your entity model types.

As you start building, you are highly encouraged to read the sections below. The linked pages explain in greater detail what Coalesce will build for you for each part of your data model.

## Relational Modeling

### Primary Keys

To work with Coalesce, your model must have a single property for a primary key. By convention, this property should be named the same as your model class with `Id` appended to that name, but you can also annotate a property with `[Key]` or name it exactly "Id" to denote it as the primary key.

### Foreign Keys & Reference Navigation Properties

While a foreign key may be defined in EF via `DbContext.OnModelCreating` or similar methods, Coalesce won't know that a property is a foreign key unless it is accompanied by a corresponding reference navigation property, and vice versa - Coalesce cannot examine your EF model metadata at generation time; it can only see the API surface of your C# code.

In cases where the foreign key is not named after the navigation property with `"Id"` appended, the `[ForeignKeyAttribute]` may be used on either the key or the navigation property to denote the other property of the pair, in accordance with the recommendations set forth by [EF Core's Modeling Guidelines](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/mapping-attributes#foreignkeyattribute).

### Collection Navigation Properties

On entities that are the "one" side of a one-to-many relationship, collection navigation properties can be defined and used in the typical manner. In the event where the inverse property on the other side of the relationship cannot be determined, `[InversePropertyAttribute]` will need to be used. [EF Core provides documentation](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/mapping-attributes#inversepropertyattribute) on how to use this attribute. Errors will be displayed at generation time if an inverse property cannot be determined without the attribute.

### One-to-one Relationships

One-to-one relationships can be represented in Coalesce, but require fairly specific configuration to satisfy both EF and Coalesce's needs. Specifically, the dependent/child side of the one-to-one (the entity whose PK is also a FK), must explicitly annotate its PK with `[ForeignKey]` pointing at the parent navigation property. For example:

```c#
public class OneToOneParent
{
    public int Id { get; set; }

    public OneToOneChild? Child { get; set; }
}

public class OneToOneChild
{
    [Key, ForeignKey("Parent")]
    public int ParentId { get; set; }

    public OneToOneParent? Parent { get; set; }
}
```

Alternatively, you could also implement a one-to-one like a normal one-to-many relationship, where both sides of the relationship have their own distinct PK, and just never add more than one child to any particular principal entity. A unique constraint on the foreign key column can enforce the singularity of this relationship.

### Inheritance (TPH and TPT)

Coalesce is compatible with TPH, TPT, and TPCT entity hierarchies. Define your models as you normally would per [the EF documentation](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance). Coalesce can work with your hierarchy model, with the following notes:

- Abstract base classes are supported, and will be available for read via `/get`, `/list`, and `/count` endpoints if they have a public `DbSet` property on the `DbContext` and are not disabled via [attributes](/topics/security.md#endpoint-security). Abstract base classes also support [Custom Methods](/modeling/model-components/methods.md) without issue.
- API responses that are declared to serve the abstract base type will serve instances of the concrete, derived types, respecting all the [property security](/topics/security.md#property-column-security) of those concrete types.
- The generated [TypeScript ViewModel](/stacks/vue/layers/viewmodels.md) for an abstract type is not like a regular ViewModel - it functions as a proxy whose primary purpose is to allow usage of `$load` to load a PK value whose concrete type is not known. After a successful load, it transforms itself into an instance of the concrete type.
- The generated [Model](/stacks/vue/layers/models.md) types **do** mirror your C# inheritance hierarchy using regular TypeScript class inheritance.
- If you want to expose the discriminator property to the client, map it to a .NET property on the base type [per EF documentation](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance#table-per-hierarchy-and-discriminator-configuration).

## Other Model Customization

### Properties

Read [Properties](/modeling/model-components/properties.md) for an outline of the different types of properties that you may place on your models and the code that Coalesce will generate for each of them. The above section also details the specific needs of properties that have a relational role in your model.

### Attributes

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from `System.ComponentModel.DataAnnotations`.

Read [Attributes](/modeling/model-components/attributes.md) to learn more.

### Methods

You can place both static and instance methods on your model classes. Any public methods annotated with [[Coalesce]](/modeling/model-components/attributes/coalesce.md) will have a generated API endpoint and corresponding generated TypeScript members for calling this API endpoint. Read [Methods](/modeling/model-components/methods.md) to learn more.

## Customizing CRUD Operations

Once you've got a solid data model in place, it's time to start customizing the way that Coalesce will _read_ your data, as well as the way that it will handle your data when processing _creates_, _updates_, and _deletes_.

### Data Sources

The method by which you can control what data the users of your application can access through Coalesce's generated APIs is by creating custom data sources. These are classes that allow complete control over the way that data is retrieved from your database and provided to clients. Read [Data Sources](/modeling/model-components/data-sources.md) to learn more.

### Behaviors

Behaviors in Coalesce are to mutating data as data sources are to reading data. Defining a behaviors class for a model allows complete control over the way that Coalesce will create, update, and delete your application's data in response to requests made through its generated API. Read [Behaviors](/modeling/model-components/behaviors.md) to learn more.
