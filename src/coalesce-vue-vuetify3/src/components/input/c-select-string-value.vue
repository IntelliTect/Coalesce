<template>
  <v-combobox
    class="c-select-string-value"
    :modelValue="internalValue ?? undefined"
    @update:modelValue="onInput"
    :loading="loading"
    :items="items"
    :hide-no-data="!listWhenEmpty"
    v-model:search="search"
    v-model:menu="menu"
    v-bind="inputBindAttrs"
  >
  </v-combobox>
</template>

<script lang="ts" setup generic="TModel extends Model">
import { computed, ref, watch, onMounted } from "vue";
import {
  ModelApiClient,
  ItemResultPromise,
  Model,
  StringValue,
  CollectionValue,
  ItemMethod,
  ModelType,
} from "coalesce-vue";
import {
  ForSpec,
  MethodForSpec,
  useMetadataProps,
} from "../c-metadata-component";

const MODEL_REQUIRED_MESSAGE =
  "c-select-string-value requires a model to be provided via the `model` prop, or a type to be provided via the `for` prop.";

defineOptions({
  name: "c-select-string-value",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

type StringsStaticMethod = ItemMethod & {
  isStatic: true;
  return: CollectionValue & { itemType: StringValue };
};

const props = defineProps<{
  /** An object owning the value to be edited that is specified by the `for` prop. */
  model?: TModel | null;

  /** A metadata specifier for the value being bound. One of:
   * * A string with the name of the value belonging to `model`. E.g. `"jobTitle"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.props.jobTitle`.
   */
  for: ForSpec<TModel, StringValue>;

  method: MethodForSpec<TModel, StringsStaticMethod>;
  params?: any;
  listWhenEmpty?: boolean;
}>();

const modelValue = defineModel<string | null>();
const menu = ref(false);

const { inputBindAttrs, valueMeta, modelMeta, valueOwner } =
  useMetadataProps(props);

const methodOwner = computed((): ModelType => {
  if (
    modelMeta.value &&
    typeof props.method == "string" &&
    modelMeta.value.type == "model"
  ) {
    // <CSSV model={vm} for=propName >
    return modelMeta.value;
  }

  if (valueMeta.value?.type == "model") {
    // <CSSV for=TypeName method=methodName >
    return valueMeta.value.typeDef;
  }

  throw Error(MODEL_REQUIRED_MESSAGE);
});

const methodMeta = computed(() => {
  let method;
  if (typeof props.method == "object") {
    method = props.method;
  } else {
    method = methodOwner.value.methods[props.method];
  }

  if (
    !method.isStatic ||
    method.transportType != "item" ||
    method.return.type != "collection" ||
    method.return.itemType.type != "string"
  ) {
    throw Error(
      "c-select-string-value requires a static model method that returns an array of strings."
    );
  }

  return method;
});

const caller = new ModelApiClient(methodOwner.value)
  .$useSimultaneousRequestCaching()
  .$makeCaller("item", (c, page?: number, search?: string) => {
    return c.$invoke(methodMeta.value, {
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
    caller(1, newVal).then(() => menu.value = true);
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

function onInput(value: string | null | undefined) {
  if (valueOwner.value && valueMeta.value) {
    (valueOwner.value as any)[valueMeta.value.name] = value;
  }

  modelValue.value = value || null;
}

onMounted(() => caller());
</script>
