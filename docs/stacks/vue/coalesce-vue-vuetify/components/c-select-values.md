# c-select-values

<!-- MARKER:summary -->
    
A multi-select input component for collections of non-object values (primarily strings and numbers).

<!-- MARKER:summary-end -->

::: tip
It is unlikely that you'll ever need to use this component directly - it is highly recommended that you use [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) instead and let it delegate to [c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md) for you.
:::

[[toc]]

## Examples

``` vue-html
<c-select-values 
    :model="post.setTags.args" 
    for="Post.methods.setTags.params.tagNames" 
/>
```

## Props

<Prop def="for: string | CollectionProperty | CollectionValue" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

<Prop def="model?: Model" lang="ts" />

An object owning the value that was specified by the `for` prop.

<Prop def="value: any" lang="ts" />

If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.


