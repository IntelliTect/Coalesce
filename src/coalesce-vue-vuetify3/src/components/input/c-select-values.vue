<template>
  <v-combobox
    class="c-select-values"
    :modelValue="internalValue"
    @update:modelValue="onInput"
    v-bind="inputBindAttrs"
    multiple
    chips
    deletable-chips
    small-chips
    no-filter
  >
  </v-combobox>
</template>

<script
  lang="ts"
  setup
  generic="TModel extends Model | AnyArgCaller | undefined"
>
import {
  AnyArgCaller,
  CollectionValue,
  convertValueToModel,
  Model,
  NumberValue,
  StringValue,
} from "coalesce-vue";
import { computed } from "vue";
import { ForSpec, useMetadataProps } from "../c-metadata-component";

defineOptions({
  name: "c-select-values",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

const props = defineProps<{
  /** An object owning the value to be edited that is specified by the `for` prop. */
  model?: TModel;

  /** A metadata specifier for the value being bound. One of:
   * * A string with the name of the value belonging to `model`. E.g. `"tags"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.props.tags`.
   * * A string in dot-notation that starts with a type name. E.g. `"Person.tags"`.
   */
  for: ForSpec<
    TModel,
    // Technically this supports anything that is parsable from a string,
    // but convertValueToModel is not well suited to parsing user date inputs,
    // booleans don't make sense as collection elements, and enums should use c-input.
    // So, we restrict to strings and numbers.
    CollectionValue & { itemType: StringValue | NumberValue }
  >;
}>();

const modelValue = defineModel<any[]>();

const { inputBindAttrs, valueMeta, valueOwner } = useMetadataProps(props);

const internalValue = computed((): any[] => {
  if (valueOwner.value && collectionMeta.value) {
    return valueOwner.value[collectionMeta.value.name] || [];
  }
  return modelValue.value || [];
});

const collectionMeta = computed(() => {
  const meta = valueMeta.value;
  if (
    meta &&
    meta.type == "collection" &&
    meta.itemType.type != "model" &&
    meta.itemType.type != "object"
  ) {
    return meta;
  } else {
    throw Error(
      "c-select-values requires value metadata for a collection of non-object values. Specify it with the 'for' prop'"
    );
  }
});

// Typed as unknown because Vuetify's types are a little weird
function onInput(value: unknown) {
  if (!Array.isArray(value)) {
    throw new Error("Got non-array from VCombobox's onUpdate:modelValue");
  }

  const items: any[] = [];
  for (let i = 0; i < value.length; i++) {
    try {
      items.push(convertValueToModel(value[i], collectionMeta.value.itemType));
    } catch {
      // Ignore items that have parse exceptions.
      // TODO: Throw a more specific ParseError from coalesce-vue, and catch that.
    }
  }

  if (valueOwner.value) {
    return (valueOwner.value[collectionMeta.value.name] = items);
  }

  modelValue.value = items;
}

// Access this so it will throw an error if the meta props aren't in order.
collectionMeta.value;
</script>
