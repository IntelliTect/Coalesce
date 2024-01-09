<template>
  <v-combobox
    class="c-select-string-value"
    :modelValue="internalValue ?? undefined"
    @update:modelValue="onInput"
    :loading="loading"
    :items="items"
    :hide-no-data="false"
    v-model:searchInput="search"
    v-bind="inputBindAttrs"
  >
  </v-combobox>
  <!-- TODO: This component has a lot of issues in vuetify3:
    - Dropdown menu hides until the second keystroke.
    - Current value disappears when the component receives focus if the value has not yet been edited since the component mounted.
  -->
</template>

<script lang="ts" setup generic="TModel extends Model | undefined">
import { computed, ref } from "vue";
import {
  ModelApiClient,
  ItemResultPromise,
  Model,
  StringValue,
} from "coalesce-vue";
import { ForSpec, useMetadataProps } from "../c-metadata-component";
import { watch } from "vue";
import { onMounted } from "vue";

const MODEL_REQUIRED_MESSAGE =
  "c-select-string-value requires a model to be provided via the `model` prop, or a type to be provided via the `for` prop.";

defineOptions({
  name: "c-select-string-value",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

const props = defineProps<{
  /** An object owning the value to be edited that is specified by the `for` prop. */
  model?: TModel;

  /** A metadata specifier for the value being bound. One of:
   * * A string with the name of the value belonging to `model`. E.g. `"autocompleteString"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.props.autocompleteString`.
   * * A string in dot-notation that starts with a type name. E.g. `"Person.autocompleteString"`.
   */
  for: ForSpec<TModel, StringValue>;

  method: string;
  params?: any;
  listWhenEmpty?: boolean;
}>();

const modelValue = defineModel<string | null>();

const {
  inputBindAttrs,
  valueMeta,
  modelMeta: { value: modelMeta },
  valueOwner,
} = useMetadataProps(props);

if (!modelMeta || !("type" in modelMeta) || modelMeta.type != "model") {
  throw Error(MODEL_REQUIRED_MESSAGE);
}

const methodMeta = modelMeta.methods[props.method];

if (!methodMeta) {
  throw Error(
    `No method named ${props.method} could be found on type ${modelMeta.name}. Note: method name is expected to be camelCase.`
  );
}

if (
  !methodMeta.isStatic ||
  methodMeta.transportType != "item" ||
  methodMeta.return.type != "collection" ||
  methodMeta.return.itemType.type != "string"
) {
  throw Error(
    "c-select-string-value requires a static model method that returns an array of strings."
  );
}

const caller = new ModelApiClient(modelMeta)
  .$withSimultaneousRequestCaching()
  .$makeCaller("item", (c, page?: number, search?: string) => {
    return c.$invoke(methodMeta, {
      page,
      search,
      ...props.params,
    }) as ItemResultPromise<string[]>;
  })
  .setConcurrency("debounce");

const search = ref<string>();
watch(search, (newVal, oldVal) => {
  if (!newVal && !props.listWhenEmpty) {
    return;
  }

  if (newVal != oldVal) {
    // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
    // since null == undefined, the transition from undefined to null will fail.
    caller(1, newVal);
  }
});

const loading = computed(() => {
  return caller.isLoading;
});

const items = computed(() => {
  if (!search.value && !props.listWhenEmpty) return [];
  return caller.result || [];
});

const internalValue = computed((): string | null => {
  if (valueOwner.value && valueMeta.value) {
    return (valueOwner.value as any)[valueMeta.value.name];
  }

  return modelValue.value ?? null;
});

// `unknown` because vuetify's types are a little weird right now (wont infer `string`)
function onInput(value: string | null | undefined) {
  if (valueOwner.value && valueMeta.value) {
    (valueOwner.value as any)[valueMeta.value.name] = value;
  }

  modelValue.value = value || null;
}

onMounted(() => caller());
</script>
