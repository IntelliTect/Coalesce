<template>
  <v-container
    fluid
    class="c-admin-table-page"
    :class="'type-' + metadata.name"
  >
    <c-admin-table class="c-admin-table-page--table" :list="listVM" query-bind>
    </c-admin-table>

    <c-admin-methods
      class="c-admin-table-page--methods"
      :model="listVM"
      :area="HiddenAreas.List"
      auto-reload-model
    ></c-admin-methods>
  </v-container>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { HiddenAreas, ListViewModel, ModelType } from "coalesce-vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

export default defineComponent({
  name: "c-admin-table-page",

  props: {
    ...makeMetadataProps(),
    type: { required: false, type: String, default: null },
    list: { required: false, type: Object as PropType<ListViewModel> },
  },

  setup(props) {
    return { ...useMetadataProps(props), HiddenAreas };
  },

  data() {
    let listVM;
    if (this.list) {
      listVM = this.list;
    } else {
      if (!this.type) {
        throw Error(
          "c-admin-table-page: If prop `list` is not provided, `type` is required."
        );
      } else if (!ListViewModel.typeLookup![this.type]) {
        // TODO: Bake a `getOrThrow` into `typeLookup`.
        throw Error(
          `No model named ${this.type} is registered to ListViewModel.typeLookup`
        );
      }
      listVM = new ListViewModel.typeLookup![this.type]();
      listVM.$includes = "admin-list";
    }

    return {
      listVM,
    };
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
