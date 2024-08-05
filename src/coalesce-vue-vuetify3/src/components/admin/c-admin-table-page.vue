<template>
  <v-container
    fluid
    class="c-admin-table-page"
    :class="'type-' + metadata.name"
  >
    <c-admin-table
      class="c-admin-table-page--table"
      :list="listVM"
      :color
      :autoSave
      :page-sizes="[10, 25, 100, 500]"
      query-bind
    >
    </c-admin-table>

    <c-admin-methods
      class="c-admin-table-page--methods"
      :model="listVM"
      :area="HiddenAreas.List"
      :color
      auto-reload-model
    ></c-admin-methods>
  </v-container>
</template>

<script lang="ts" setup>
import { computed, PropType, Ref, ref, watch } from "vue";
import { HiddenAreas, ListViewModel, ModelType } from "coalesce-vue";

const props = defineProps({
  type: { required: false, type: String, default: null },
  color: { required: false, type: String, default: null },
  list: { required: false, type: Object as PropType<ListViewModel> },
  autoSave: {
    required: false,
    type: [String, Boolean] as PropType<"auto" | boolean>,
    default: "auto",
  },
});

// Casted to any because this will never hold `undefined` after the following watcher runs:
const listVM: Ref<ListViewModel> = ref<ListViewModel>() as any;
watch(
  () => props.list,
  (list) => {
    if (list) {
      listVM.value = list;
      return;
    }

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

    listVM.value = new ListViewModel.typeLookup![props.type]();
    listVM.value.$includes = "admin-list";
  },
  { immediate: true }
);

const metadata = computed((): ModelType => {
  if (listVM.value) {
    return listVM.value.$metadata;
  }

  throw `No metadata available - no list provided, and couldn't create one.`;
});

/** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
const pageTitle = computed(() => {
  return metadata.value.displayName + " List";
});

defineExpose({ pageTitle });
</script>

<style lang="scss">
.c-admin-table-page {
  .c-admin-table-page--methods {
    margin-top: 30px;
  }
}
</style>
