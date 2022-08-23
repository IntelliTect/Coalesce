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

<script lang="ts">
import { convertValueToModel } from "coalesce-vue";
import { defineComponent } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

export default defineComponent({
  name: "c-select-values",

  setup(props) {
    return { ...useMetadataProps(props) };
  },

  props: {
    ...makeMetadataProps(),
    value: { required: false, type: Array },
  },

  computed: {
    internalValue(): any[] {
      if (this.model && this.collectionMeta) {
        return (this.model as any)[this.collectionMeta.name] || [];
      }
      return this.value || [];
    },

    collectionMeta() {
      const valueMeta = this.valueMeta;
      if (
        valueMeta &&
        valueMeta.type == "collection" &&
        valueMeta.itemType.type != "model" &&
        valueMeta.itemType.type != "object"
      ) {
        return valueMeta;
      } else {
        throw Error(
          "c-select-values requires value metadata for a collection of non-object values. Specify it with the 'for' prop'"
        );
      }
    },
  },

  methods: {
    onInput(value: any[]) {
      const items: any[] = [];
      for (let i = 0; i < value.length; i++) {
        try {
          items.push(
            convertValueToModel(value[i], this.collectionMeta.itemType)
          );
        } catch {
          // Ignore items that have parse exceptions.
          // TODO: Throw a more specific ParseError from coalesce-vue, and catch that.
        }
      }

      if (this.model) {
        return ((this.model as any)[this.collectionMeta.name] = items);
      }

      this.$emit("input", items);
    },
  },

  mounted() {
    // Access this so it will throw an error if the meta props aren't in order.
    this.collectionMeta;
  },
});
</script>