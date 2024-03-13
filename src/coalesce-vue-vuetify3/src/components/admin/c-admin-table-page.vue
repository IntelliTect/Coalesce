<template>
  <v-container
    fluid
    class="c-admin-table-page"
    :class="'type-' + metadata.name"
  >
    <c-admin-table
      class="c-admin-table-page--table"
      :list="listVM"
      :color="color"
      :page-sizes="[10, 25, 100, 500]"
      query-bind
    >
    </c-admin-table>

    <c-admin-methods
      class="c-admin-table-page--methods"
      :model="listVM"
      :area="HiddenAreas.List"
      :color="color"
      auto-reload-model
    ></c-admin-methods>
  </v-container>
</template>

<script lang="ts">
import { defineComponent, PropType, Ref, ref, toRefs } from "vue";
import { HiddenAreas, ListViewModel, ModelType } from "coalesce-vue";

export default defineComponent({
  name: "c-admin-table-page",

  props: {
    type: { required: false, type: String, default: null },
    color: { required: false, type: String, default: null },
    list: { required: false, type: Object as PropType<ListViewModel> },
  },

  setup(props) {
    let listVM: Ref<ListViewModel>;
    if (props.list) {
      listVM = toRefs(props).list as any;
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
      listVM = ref(new ListViewModel.typeLookup![props.type]() as any);
      listVM.value.$includes = "admin-list";
    }

    return { listVM, HiddenAreas };
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
