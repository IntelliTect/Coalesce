# c-admin-method

<!-- MARKER:summary -->
    
Provides an interface for invoking a [method](/modeling/model-components/methods.md) and rendering its result, designed to be use in an admin page.

<!-- MARKER:summary-end -->

For each parameter of a method, a [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) will be rendered to accept the input of that parameter. A button is provided to trigger an invocation of the method, progress and errors are rendered with a [c-loader-status](/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.md), and results are rendered with [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md).

## Examples

``` vue-html
<c-admin-method :model="person" for="setTitle" auto-reload-model />
```

## Props

<Prop def="for: string | Method" lang="ts" />

A metadata specifier for the method. One of:
    
- A string with the name of the method belonging to `model`. 
- A direct reference to a method's metadata object.
- A string in dot-notation that starts with a type name and ending with a method name.

<Prop def="model: ViewModel | ListViewModel" lang="ts" />

An [ViewModel](/stacks/vue/layers/viewmodels.md) or [ListViewModel](/stacks/vue/layers/viewmodels.md) owning the method and [API Caller](/stacks/vue/layers/api-clients.md#api-callers) that was specified by the `for` prop.

<Prop def="autoReloadModel?: boolean = false" lang="ts" />

True if the `model` should have its `$load` invoked after a successful invocation of the method.


