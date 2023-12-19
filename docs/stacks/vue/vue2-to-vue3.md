---
pageClass: wide-page
---

# Vue 2 to Vue 3

If you're already experienced with Vue 2 but are new to Vue 3, or if you're migrating an existing Vue 2 app to Vue 3, you should first read through the [official migration guide](https://v3-migration.vuejs.org/).

Vuetify also offers a [migration guide](https://vuetifyjs.com/en/getting-started/upgrade-guide/) to upgrade from Vuetify 2 to Vuetify 3.

If you're new to Vue entirely, check out the rest of Vue docs and [pick your learning path](https://vuejs.org/guide/introduction.html#pick-your-learning-path).


## Coalesce Upgrade Steps

The changes specific to Coalesce when migrating from Vue2 to Vue3 are pretty minimal. Most of your work will be in following the [Vue 3 Migration Guide](https://v3-migration.vuejs.org/) and the [Vuetify 3 Migration Guide](https://vuetifyjs.com/en/getting-started/upgrade-guide/).

The table below contains the Coalesce-specific changes when migrating to Vue 3. However, the easiest migration path may be to disregard the table below and instead, [instantiate the Coalesce Vue template](/stacks/vue/getting-started.md#creating-a-project) or [look at it on GitHub](https://github.com/IntelliTect/Coalesce.Vue.Template/tree/master/content/Coalesce.Starter.Vue.Web) and compare individual files between your project and the template side by side and ingest the changes that you observe.


<table>
<thead><tr><th width="150px">Location</th><th>Old (Vue 2)</th><th>New (Vue 3)</th></tr></thead>
<tr><td>

package.json

</td>
<td style="vertical-align: top">

```json:no-line-numbers
{
  "dependencies": {
    "coalesce-vue-vuetify2": "x"
  }
}
```

</td>
<td style="vertical-align: top">

```json:no-line-numbers
{
  "dependencies": {
    "coalesce-vue-vuetify3": "x"
  }
}
```

</td></tr>
<tr><td>

vite.config.ts

</td>
<td style="vertical-align: top">

```ts:no-line-numbers
import { CoalesceVuetifyResolver } from "coalesce-vue-vuetify2/lib/build"
```

</td>
<td style="vertical-align: top">

```ts:no-line-numbers
import { CoalesceVuetifyResolver } from "coalesce-vue-vuetify3/build"

// Custom SASS options and `optimizeDeps` configuration can be removed
// since Vuetify3 no longer uses deprecated sass features,
// and pre-bundling styles no longer has appreciable benefit.
```

</td></tr>
<tr><td>

main.ts

</td>
<td style="vertical-align: top">

```ts:no-line-numbers
import "coalesce-vue-vuetify2/dist/coalesce-vue-vuetify.css"

// Either of these:
import CoalesceVuetify from 'coalesce-vue-vuetify2/lib'
import CoalesceVuetify from 'coalesce-vue-vuetify2'

Vue.use(CoalesceVuetify, {
  metadata: $metadata,
});

```

</td>
<td style="vertical-align: top">

```ts:no-line-numbers
import "coalesce-vue-vuetify3/styles.css"


import { createCoalesceVuetify } from "coalesce-vue-vuetify3";


const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
});
app.use(coalesceVuetify);
```

</td></tr>
<tr><td>

router.ts

</td>
<td style="vertical-align: top">

```ts:no-line-numbers
// Either of these:
import { CAdminTablePage, CAdminEditorPage } from 'coalesce-vue-vuetify2/lib';
import { CAdminTablePage, CAdminEditorPage } from 'coalesce-vue-vuetify2';
```

</td>
<td style="vertical-align: top">

```ts:no-line-numbers

import { CAdminEditorPage, CAdminTablePage } from "coalesce-vue-vuetify3";
```

</td></tr>
<tr><td>

Vitest/Jest tests

</td>
<td style="vertical-align: top">

If you had a global test setup file performing Vue configuration, you can likely remove it entirely, or at least remove the parts that configure Vue. Vue3 does not operate on global configuration like Vue2 did.

</td>
<td style="vertical-align: top">

See [test-utils.ts](https://github.com/IntelliTect/Coalesce.Vue.Template/blob/master/content/Coalesce.Starter.Vue.Web/src/test-utils.ts) and [HelloWorld.spec.ts](https://github.com/IntelliTect/Coalesce.Vue.Template/blob/master/content/Coalesce.Starter.Vue.Web/src/components/HelloWorld.spec.ts) in the template for examples of Vue3 component testing.

</td></tr>
</table>


## From Class Components to `<script setup>`

The components in the Coalesce template for Vue 3 have switched from `vue-class-component` to Vue Composition API with `<script setup>`, the [official recommendation](https://vuejs.org/guide/introduction.html#which-to-choose) for building full Vue 3 applications.

If you're used to writing components in Vue 2 with `vue-class-component` and `vue-property-decorator`, you can use this table of comparisons as a quick reference of what the equivalent features are using [`<script setup>`](https://vuejs.org/api/sfc-script-setup.html) and [Vue Composition API](https://vuejs.org/guide/extras/composition-api-faq.html). That said, this is not a replacement for learning and understanding the composition API. You should read the [Composition API FAQ](https://vuejs.org/guide/extras/composition-api-faq.html) as well as the [Reactivity Fundamentals](https://vuejs.org/guide/essentials/reactivity-fundamentals.html) documentation (make sure to set the API preference in the top left to Composition!).

If you'd like to continue using class components with Vue 3 (e.g. upgrading an existing project where rewriting all components is not feasible), you can try switching to [`vue-facing-decorator`](https://www.npmjs.com/package/vue-facing-decorator).


::: tip Note
The examples below assume that `unplugin-auto-import` is being used (included in the Coalesce Vue3 template), eliminating the need to manually import common Vue Composition API functions.
:::

<table>
<thead><tr><th width="150px">Feature</th><th>Class Component</th><th>Script Setup</th></tr></thead>
<tr><td>

Coalesce `ViewModel` and `ListViewModel` usage

</td>
<td style="vertical-align: top">

```vue
<script lang="ts">
import { Vue, Component } from "vue-property-decorator";
import { PersonViewModel, PersonListViewModel } from "@/viewmodels.g";

@Component({})
export default class MyComponent extends Vue {
  public person = new PersonViewModel();
  public list = new PersonListViewModel();

  async created() {
    await person.$load();
    await list.$load();

    person.$startAutoSave(this);
    list.$startAutoLoad(this);
  }
}
</script>
```

</td>
<td style="vertical-align: top">

```vue
<script lang="ts" setup>
import { PersonViewModel, PersonListViewModel } from "@/viewmodels.g";

const person = new PersonViewModel();
const list = new PersonListViewModel();

person.$useAutoSave();
list.$useAutoLoad();

// If you need to await an async operation during component creation, 
// use an IIFE so that the component mount is not delayed.
(async function created() {
  await person.$load();
  await list.$load();
})();
</script>
```

</td></tr>

<tr><td>

@Prop, @Watch

</td>
<td style="vertical-align: top">

```vue
<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({})
export default class MyComponent extends Vue {
  @Prop({ default: "Student" })
  label!: string;

  @Prop({ required: true })
  student!: ApplicationUserViewModel;

  @Watch("label")
  labelChanged(newVal, oldVal) {
    console.log(`label changed. new:${newVal}, old:${oldVal}`)
  }
}
</script>
```

</td>
<td style="vertical-align: top">

```vue
<script lang="ts" setup>
const props = withDefaults(defineProps<{
  label?: string,
  student?: ApplicationUserViewModel
}>(), { label: 'Student' })

watch(
  () => props.label,
  (newVal, oldVal) => {
    console.log(`label changed. new:${newVal}, old:${oldVal}`);
  }
);
</script>
```

</td></tr>

<tr><td>

Reactive data

</td>
<td style="vertical-align: top">

```vue
<script lang="ts">
import { Vue, Component } from "vue-property-decorator";
import { PersonViewModel } from "@/viewmodels.g";

@Component({})
export default class MyComponent extends Vue {
  public person = new PersonViewModel();

  public checked = false;

  public items = [
    { name: "Foo", checked: false, },
    { name: "Bar", checked: true, }
  ]
}
</script>
```

</td>
<td style="vertical-align: top">

```vue
<script lang="ts" setup>
import { PersonViewModel } from "@/viewmodels.g";

// Properties on coalesce-generated ViewModels have built in reactivity 
// and don't need to be wrapped ref/reactive unless you're going to replace 
// the entire top level object with a different instance.
const person = new PersonViewModel();

const checked = ref(false);

const items = reactive([
  { name: "Foo", checked: false, },
  { name: "Bar", checked: true, }
])
</script>
```

</td></tr>


<tr><td>

Computed values

</td>
<td style="vertical-align: top">

```vue
<script lang="ts">
import { Vue, Component } from "vue-property-decorator";
import { PersonViewModel } from "@/viewmodels.g";

@Component({})
export default class MyComponent extends Vue {
  public person = new PersonViewModel()

  get fullName() {
    return `${person.firstName} ${person.lastName}`
  }
}
</script>
```

</td>
<td style="vertical-align: top">

```vue
<script lang="ts" setup>
import { PersonViewModel } from "@/viewmodels.g";

const person = new PersonViewModel();

const fullName = computed(() => `${person.firstName} ${person.lastName}`)
</script>
```

</td></tr>

<tr><td>

$emit, methods

</td>
<td style="vertical-align: top">

```vue
<template>
  <input
    :value="value"
    @input="inputChanged($event.target.value)"
  />
</template>

<script lang="ts">
import { Vue, Component } from "vue-property-decorator";

@Component({})
export default class MyComponent extends Vue {
  @Prop()
  value!: string;

  inputChanged(v: string) {
    this.$emit('update:input', v)
  }
}
</script>
```

</td>
<td style="vertical-align: top">

```vue
<template>
  <input
    :value="modelValue"
    @input="inputChanged(($event.target as HTMLInputElement).value)"
  />
</template>

<script lang="ts" setup>
defineProps<{ modelValue: string | null }>();

// This may seem tedious, but it enables full Typescript intellisense!
const emit = defineEmits<{
  (e: "update:modelValue", value: string | null): void;
}>();

function inputChanged(v: string) {
  emit('update:modelValue', v)
}
</script>
```

</td></tr>


</table>
