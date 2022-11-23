<template>
  <v-container fluid class="c-admin-table-page">
    <c-admin-table
      class="c-admin-table-page--table"
      :list="listVM"
      :color="color"
      query-bind
    >
    </c-admin-table>

    <c-admin-methods
      class="c-admin-table-page--methods"
      :model="listVM"
      :color="color"
      auto-reload-model
    ></c-admin-methods>
  </v-container>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";
import { ListViewModel, ModelType } from "coalesce-vue";

export default defineComponent({
  name: "c-admin-table-page",

  props: {
    ...makeMetadataProps(),
    type: { required: false, type: String, default: null },
    color: { required: false, type: String, default: null },
    list: { required: false, type: Object as PropType<ListViewModel> },
  },

  setup(props) {
    let listVM;
    if (props.list) {
      listVM = props.list;
    } else {
      if (!props.type) {
        throw Error(
          "c-admin-table-page: If prop `list` is not provided, `type` is required."
        );
      } else if (!ListViewModel.typeLookup![props.type]) {
        // TODO: Bake a `getOrThrow` into `typeLookup`.
        throw Error(
          `No model named ${props.type} is registered to ListViewModel.typeLookup`
        );
      }
      listVM = new ListViewModel.typeLookup![props.type]();
    }

    return { listVM, ...useMetadataProps(props) };
  },

  computed: {
    /** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
    pageTitle() {
      return this.metadata.displayName + " List";
    },

    metadata(): ModelType {
      if (this.listVM) {
        return this.listVM.$metadata;
      }
      // TODO: this message is bad.
      throw `No metadata available - no list provided, and couldn't create one.`;
    },
  },
});
</script>

<style lang="scss">
.c-admin-table-page {
  .c-admin-table-page--methods {
    margin-top: 30px;
  }
}
</style>
