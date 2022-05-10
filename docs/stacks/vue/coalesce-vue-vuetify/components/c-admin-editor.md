.. _c-admin-editor:

c-admin-editor
==============

.. MARKER:summary
    
An editor for a single [ViewModel](/stacks/vue/layers/viewmodels.md) instance. Provides a [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) for each property of the model.

.. MARKER:summary-end

Does not automatically enable [auto-save](/stacks/vue/layers/viewmodels.md) - if desired, this must be enabled by the implementor of this component.

[[toc]]

Examples
--------

``` vue-html

    <c-admin-editor :model="person" />


```

Props
-----

`model: ViewModel | ListViewModel`
    The [ViewModel](/stacks/vue/layers/viewmodels.md) to render an editor for.



