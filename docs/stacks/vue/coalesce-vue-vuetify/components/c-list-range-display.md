# c-list-range-display

<!-- MARKER:summary -->
    
Displays pagination information about the current `$items` of a [ListViewModel](/stacks/vue/layers/viewmodels.md) in the format ``<start index> - <end index> of <total count>``.

<!-- MARKER:summary-end -->

Uses the pagination information returned from the last successful `$load` call, not the current `$params` of the [ListViewModel](/stacks/vue/layers/viewmodels.md).

## Examples

``` vue-html
<c-list-range-display :list="list" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) to display pagination information for.




