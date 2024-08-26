# Immutability

Immutability of data is an essential consideration of almost any system - it is usually important there is confidence in the correctness of historical data. 

For example, a record of an invoice must not change when the prices for the line items are updated as the future progresses, and if a purchased item is later refunded, none of the data about the original purchase should be changed. Only new information describing the refund should be added to the database, including details about the refund and some indication of "refunded" on the original item (like a foreign key referencing the refund transaction)

It is ultimately up to each individual use case when deciding when data needs to be immutable, but at the very least, any data involving financial transactions and any data where auditing is a major concern should be immutable to at least some extent.

This page explores some techniques to achieve immutability in a Coalesce application.

## Configuration vs Transactional data

### Transactional Data

In an application, transaction data is any data that represents an event or action that occurred at a particular time. A purchase, an invoice or account statement, a message or email, an audit or error log, a calendar event - these are all examples of transactional data.

### Configuration Data

Configuration data is the data in a system that informs how transactions occur. The current price of an item, any kind of template, and even singleton configuration like a site's theme and colors.

If you have configuration data that is linked to transactional data and affects how that transactional data is interpreted, that configuration data becomes a strong candidate for immutability.


## General Techniques

The following immutability techniques are applicable to both transaction and configuration data.

### Disable edits

One of the simplest ways to enforce immutability of data is to simply prevent it from being edited (and deleted, if applicable) at all. 

Instead of using incremental saves (e.g. through [auto-saves](/stacks/vue/layers/viewmodels.md#auto-save)), only create completed records or sets of records with an explicit [save](/stacks/vue/layers/viewmodels.md#saving-and-deleting) or [bulk save](/stacks/vue/layers/viewmodels.md#bulk-saves) operation, or a [custom method](/modeling/model-components/methods.md). Disable edits entirely to the immutable entities with [security attributes](/modeling/model-components/attributes/security-attribute.md#edit).

Unfortunately, this approach is usually not feasible for anything but the simplest of data models. There are usually additional considerations to be had, including:

- Completely immutable hierarchical data models can be unreasonably difficult to work with, requiring a full clone of the hierarchy to make the smallest of change (although this *could* be a desirable characteristic, depending on the use case).
- There's no ability to save in-progress or "draft" state. All data must be created all at once.



### Editable until finalized

A more nuanced approach to immutability is to only disable editing once a record has reached a "finalized" state. For example, an order that has transitioned from a "shopping cart" to a submitted, paid-for order. Or, a set of configuration that has transitioned from a "draft" state to a "published" state.

Using [Behaviors](/modeling/model-components/behaviors.md) on all entities in a hierarchy, prohibit any undesirable edits by overriding the `BeforeSave` or `BeforeSaveAsync` method and checking the state of the record in the database to determine if edits are permissible.

This technique has the advantage of being infinitely customizable, allowing for scenarios like administrative overrides of records that would otherwise be uneditable by an unprivileged user.


### Soft deletes

While not a immutability strategy on its own, implementing immutability usually requires the prevention hard deletes of existing records. However, the ability to retire or archive old records using soft deletes is still valuable and doesn't violate the principals of immutability. Doing so is fairly straightforward in Coalesce:

- Add a property to the type to indicate soft delete status (usually a `DateTimeOffset? DeletedDate { get; set; }`)
- Choose how soft deletes will occur:
  - To soft-delete items using the built-in `/delete` endpoint and `$delete` API on `ViewModel` instances, override `ExecuteDeleteAsync` on the type's [Behaviors](/modeling/model-components/behaviors.md) to set the `DeletedDate` and call `db.SaveChangesAsync()`. Do not call the base ExecuteDeleteAsync method (which will perform a hard delete). This approach also makes the Delete button in [admin pages](/stacks/vue/coalesce-vue-vuetify/overview.md#admin-components) to perform a soft delete.
  - Otherwise, set the soft delete flag using regular saves, just as you would change any other property. Don't forget to implement security restrictions around who can delete and un-delete records if that's important to your application.
- Filter out soft-deleted values from selection in your custom UI pages. There are a few options here:
  - The simplest way is to use Coalesce's built-in [filtering](/modeling/model-components/data-sources.md#member-applylistpropertyfilter) to exclude soft-deleted items. This can be done from a [ListViewModel's $params.filter](/stacks/vue/layers/viewmodels.md#member-item-_params):

    ``` ts:no-line-numbers
    const list = new ItemTypeListViewModel();
    list.$params.filter.deletedDate = null;
    ```
    
    ...or be passed directly to a [`c-input` or `c-select`](/stacks/vue/coalesce-vue-vuetify/components/c-select.html#member-params):
    
    ``` vue-html
    <c-select 
      :model="item" 
      for="itemType" 
      :params="{ filter: { deletedDate: null } }" 
    />
    ```

  - You can also use a [custom data source](/modeling/model-components/data-sources.md) if your needs around excluding soft-deleted items are more complex. For example, if there are certain classes of users in your application who should not be allowed to read soft-deleted items, enforce that in the [default data source](/modeling/model-components/data-sources.md#defining-data-sources) for the type.



## Configuration Immutability

Configuration data is the data in a system that informs how transactions occur. The current price of an item, any kind of template, and even singleton configuration like a site's theme and colors.

If you have configuration data that is linked to transactional data and affects how that transactional data is interpreted, that configuration data becomes a strong candidate for immutability as well.

### Enforce nothing, document consequences

The simplest and riskiest approach is to enforce nothing in the application, but ensure that configuration administrators are aware of the consequences of changing configuration that could have unintended consequences.

For example, in a scenario with a transactional "Item" record and a configuration "ItemType" record, changing the name of the ItemType would affect the apparent type of all existing Item records that use that type. This can be OK if modifications are performed with this understanding as to not alter the meaning of existing data, but can have undesirable consequences if an existing ItemType is renamed to something completely unrelated.

### Disable edits

The next simplest approach is to make configuration records fully immutable by disabling edits and hard deletes using [security attributes](/modeling/model-components/attributes/security-attribute.md#edit). This is largely foolproof, but comes with the same drawbacks [as described above](#disable-edits).

For simple cases like a table having not much more than a `Name` column that provides values in a dropdown, the burden on configuration administrators is usually small. However, for more complex configuration - especially hierarchical configuration - the burden imparted by pure immutability is often unreasonably high. For these scenarios, continue reading the next sections.


### Editable until used

As an extension of the [Editable until finalized](#editable-until-finalized) technique described above, configuration data could be left editable as long as it has not yet been referenced by any transactional data.

This can be useful for scenarios where a formal "publish" state for the configuration is excessive. Pick-lists for selection in a dropdown, for example, can benefit from this approach by allowing values to be created and worked on as long as changes would not affect the meaning of any existing transactional data that references those values.

To enforce this, use [Behaviors](/modeling/model-components/behaviors.md) to block edits to in-use configuration by looking in the database for uses of the configuration record being edited.


### Copy onto transactional records

Another strategy for dealing with configuration changes is to leave configuration records mutable, but copy the important configuration values onto each transactional record as transactions occur. 

This can work great for financial records especially - when a purchase is finalized and paid for, copy the fields like price and description onto each line item in the purchase. This way, future updates to products do not affect past purchases of that item.

Ensure that the properties in your transactional records that will hold the snapshotted configuration [are immutable](/modeling/model-components/attributes/security-attribute.md#read), then populate these properties from configuration in your backend code (custom methods, services, or behaviors) when transactional records are created.

### Versioned configuration

A more advanced but more powerful system of configuration is to use versioned configuration.

In this approach, there are two tables: A primary configuration table that is freely mutable, and a second table that is versioned and immutable. The primary table keeps track of active version of configuration as well as any configuration that does not need to be versioned or kept immutable, while the records in the versioned table are what get associated to transaction data that relies on the configuration.

For example:

``` c#

[Delete(DenyAll)]
public class ProductConfiguration 
{
    public int Id { get; set; }

    public string Sku { get; set; }
    public string MarketingDescription { get; set; }
    public string InternalNotes { get; set; }

    public int? CurrentVersionId { get; set; }
    public ProductConfigurationVersion CurrentVersion { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }
}

[Edit(DenyAll)]
[Delete(DenyAll)]
public class ProductConfigurationVersion
{
    public int Id { get; set; }

    public int ConfigurationId { get; set; }
    public ProductConfiguration Configuration { get; set; }

    public string Name { get; set; }
    public decimal Price { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
}

```

In this example, other configuration records that needs to reference the product can reference the `ProductConfiguration` record and not need to worry about requiring updates to foreign keys every time a new version of the product configuration is created. 

Transactional records, on the other hand, should have foreign keys that reference the `ProductConfigurationVersion` record so that the exact active version at the time of purchase is known. The principal configuration record can be acquired through the `Configuration` navigation property.

