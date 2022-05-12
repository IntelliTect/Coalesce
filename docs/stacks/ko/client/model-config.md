

# ViewModel Configuration

A crucial part of the generated TypeScript ViewModels that Coalesce creates for you is the hierarchical configuration system that allows coarse-grained or fine-grained control over their behaviors.

## Hierarchy

The configuration system has four levels where configuration can be performed, structured as follows:

### Root Configuration


<Prop def="Coalesce.GlobalConfiguration: ModelConfiguration<any>
Coalesce.GlobalConfiguration.app: AppConfiguration" lang="ts" /> 
    
The root configuration contains all configuration properties which apply to class category ([TypeScript ViewModels](/stacks/ko/client/view-model.md), [TypeScript ListViewModels](/stacks/ko/client/list-view-model.md), and [Services](/modeling/model-types/services.md)). The `app` property contains global app configuration that exists independent of any models. Then, for each class kind, the following are available:

### Root ViewModel/ListViewModel Configuration
<Prop def="Coalesce.GlobalConfiguration.viewModel: ViewModelConfiguration<BaseViewModel>
Coalesce.GlobalConfiguration.listViewModel: ListViewModelConfiguration<BaseListViewModel<BaseViewModel>, BaseViewModel>
Coalesce.GlobalConfiguration.serviceClient: ServiceClientConfiguration<ServiceClient>" lang="ts" /> 
    
Additional root configuration objects exist, one for each class kind. These configuration objects govern behavior that applies to only objects of these types. Root configuration *can* be overridden using these objects, although the practicality of doing so is dubious.

### Class Configuration
<Prop def="ViewModels.ClassName.coalesceConfig: ViewModelConfiguration<ViewModels.ClassName>
ListViewModels.ClassNameList.coalesceConfig: ListViewModelConfiguration<ListViewModels.ClassNameList, ViewModels.ClassName>
Services.ServiceNameClient.coalesceConfig: ServiceClientConfiguration<ServiceName>" lang="ts" /> 

Each class kind has a static property named `coalesceConfig` that controls behavior for all instances of that class.

### Instance Configuration
<Prop def="instance.coalesceConfig: ViewModelConfiguration<ViewModels.ClassName>
listInstance.coalesceConfig: ListViewModelConfiguration<ListViewModels.ClassNameList, ViewModels.ClassName>
serviceInstance.coalesceConfig: ServiceClientConfiguration<ServiceName>" lang="ts" /> 

Each instance of these classes also has a `coalesceConfig` property that controls behaviors for that instance only.



## Evaluation

All configuration properties are Knockout `ComputedObservable<T>` objects. These observables behave like any other observable - call them with no parameter to obtain the value, call with a parameter to set their value.

Whenever a configuration property is read from, it first checks its own configuration object for the value of that property. If the explicit value for that configuration object is null, the parent's configuration will be checked for a value. This continues until either a value is found or the root configuration object is reached.

When a configuration property is given a value, that value is established on that configuration object only. Any dependent configuration objects will not be modified, and if those dependent configuration objects already have a value for that property, their existing value will be used unless that value is later set to null.

To obtain the raw value for a specific configuration property, call the `raw()` method on the observable: `model.coalesceConfig.autoSaveEnabled.raw()`.


## Available Properties & Defaults

The following configuration properties are available. Their default values are also listed. Note that all configuration properties are observables, but for simplicity the documentation below lists the underlying type.

### Root Configuration

These properties on `Coalesce.GlobalConfiguration` are available to both ViewModelConfiguration, ListViewModelConfiguration, and ServiceClientConfiguration.

<Prop def="baseApiUrl: string = '/api'" lang="ts" />

The relative url where the API may be found. 


<Prop def="baseViewUrl: string = ''" lang="ts" />

The relative url where the admin views may be found.


<Prop def="showFailureAlerts: boolean = true" lang="ts" />

Whether or not the callback specified for `onFailure` will be called or not.


<Prop def="onFailure: (obj, message) => alert(message)" lang="ts" />

A callback to be called when a failure response is received from the server.


<Prop def="onStartBusy: obj => Coalesce.Utilities.showBusy()" lang="ts" />

A callback to be called when an AJAX request begins.


<Prop def="onFinishBusy: obj => Coalesce.Utilities.hideBusy()" lang="ts" />

A callback to be called when an AJAX request completes.


### App Configuration

These properties on `Coalesce.GlobalConfiguration.app` are not hierarchical - they govern the entire Coalesce application:

<Prop def="select2Theme: string | null = null" lang="ts" />

The theme parameter to select2's constructor when called by Coalesce's select2 [Knockout Bindings](/stacks/ko/client/bindings.md).


### ViewModelConfiguration


<Prop def="saveTimeoutMs: number = 500" lang="ts" />

Time to wait after a change is seen before auto-saving (if `autoSaveEnabled` is true). Acts as a debouncing timer for multiple simultaneous changes.

<Prop def="saveIncludedFields: string[] | null = null" lang="ts" />

An array of property names that, if set, will determine which fields will be sent to the server when saving. Only those values that are actually sent to the server will be mapped to the underlying entity.

This can improves the handling of concurrent changes being made by multiple users against different fields of the same entity. Specifically, if one page is designed to edit fields A and B, and another page is designed for editing fields C and D, you can configure this setting appropriately on each page to only save the corresponding fields.

Due to design limitations, this cannot be determined dynamically like it can with [Vue's $saveMode property](/stacks/vue/layers/viewmodels.md)

::: warning
@[import-md "after":"MARKER:surgical-saves-warning", "before":"MARKER:end-surgical-saves-warning"](../../../modeling/model-types/dtos.md)
:::

<Prop def="autoSaveEnabled: boolean = true" lang="ts" />

Determines whether changes to a model will be automatically saved after `saveTimeoutMs` milliseconds have elapsed.


<Prop def="autoSaveCollectionsEnabled: boolean = true" lang="ts" />

Determines whether or not changes to many-to-many collection properties will automatically trigger a save call to the server or not.


<Prop def="showBusyWhenSaving: boolean = false" lang="ts" />

Whether to invoke `onStartBusy` and `onFinishBusy` during saves.


<Prop def="loadResponseFromSaves: boolean = true" lang="ts" />

Whether or not to reload the ViewModel with the state of the object received from the server after a call to `.save()`.


<Prop def="validateOnLoadFromDto: boolean = true" lang="ts" />

Whether or not to validate the model after loading it from a DTO from the server. Disabling this can improve performance in some cases.


<Prop def="setupValidationAutomatically: boolean = true" lang="ts" />

Whether or not validation on a ViewModel should be setup in its constructor, or if validation must be set up manually by calling `viewModel.setupValidation()`. Turning this off can improve performance in read-only scenarios.


<Prop def="onLoadFromDto: null | ((object: T) => void) = null" lang="ts" />

An optional callback to be called when an object is loaded from a response from the server. Callback will be called after all properties on the ViewModel have been set from the server response.


<Prop def="initialDataSource: null | DataSource<T> | (new () => DataSource<T>) = null" lang="ts" />

The dataSource (either an instance or a type) that will be used as the initial dataSource when a new object of this type is created. Not valid for global configuration; recommended to be used on class-level configuration. E.g. `ViewModels.MyModel.coalesceConfig.initialDataSource(MyModel.dataSources.MyDataSource);`


### ListViewModelConfiguration

No special configuration is currently available for ListViewModels.

### ServiceClientConfiguration

No special configuration is currently available for ServiceClients.