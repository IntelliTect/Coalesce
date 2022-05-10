.. _c-admin-methods:

c-admin-methods
===============

.. MARKER:summary
    
Renders in a [Vuetify](https://vuetifyjs.com/) [v-expansion-panels](https://vuetifyjs.com/en/components/expansion-panels/) a [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md) for each method on a [ViewModel](/stacks/vue/layers/viewmodels.md) or [ListViewModel](/stacks/vue/layers/viewmodels.md).

.. MARKER:summary-end

[[toc]]

Examples
--------

``` vue-html
<c-admin-methods :model="person" class="x" auto-reload-model />
```

``` vue-html

    <c-admin-methods :model="person" auto-reload-model />
    

```

``` vue-html

    <c-admin-methods :model="personList" auto-reload-model />


```

Props
-----

`model: ViewModel | ListViewModel`
    An [ViewModel](/stacks/vue/layers/viewmodels.md) or [ListViewModel](/stacks/vue/layers/viewmodels.md) whose methods should each render as a [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md).

`autoReloadModel?: boolean = false`
    True if the `model` should have its `$load` invoked after a successful invocation of any method.




