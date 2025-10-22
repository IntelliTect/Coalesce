<template>
  <v-container class="c-admin-editor-page" :class="'type-' + metadata.name">
    <c-admin-editor
      class="c-admin-editor-page--editor"
      :model="viewModel"
      :color="color"
      @deleted="onDeleted"
    />

    <c-admin-methods
      class="c-admin-editor-page--methods"
      :model="viewModel"
      :area="HiddenAreas.Edit"
      auto-reload-model
      :color="color"
    />
  </v-container>
</template>

<script setup lang="ts">
import {
  ViewModel,
  ModelType,
  ListParameters,
  mapQueryToParams,
  bindKeyToRouteOnCreate,
  modelDisplay,
  HiddenAreas,
} from "coalesce-vue";

import { computed, watch, getCurrentInstance, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { isPropReadOnly } from "../../util";
import { copyParamsToNewViewModel } from "./util";

defineOptions({
  name: "c-admin-editor-page",
});

const props = withDefaults(
  defineProps<{
    type: string;
    id?: string | number;
    color?: string;
    autoSave?: "auto" | boolean;
  }>(),
  {
    color: "",
    autoSave: "auto",
  },
);

const route = useRoute();
const router = useRouter();
const instance = getCurrentInstance();

if (!ViewModel.typeLookup![props.type]) {
  // TODO: Bake a `getOrThrow` into `typeLookup`.
  throw Error(
    `No model named ${props.type} is registered to ViewModel.typeLookup`,
  );
}

const viewModel = new ViewModel.typeLookup![props.type]();
viewModel.$includes = "admin-editor";

const effectiveAutoSave = computed(() => {
  const value = props.autoSave;
  if (value == null || value == "auto") {
    const meta = viewModel.$metadata;
    for (const propName in meta.props) {
      const prop = meta.props[propName];
      if (prop.createOnly && !isPropReadOnly(prop, viewModel)) {
        // This is an editable, create-only prop (which means the model isn't yet saved).
        // Explicit saves must be used so the user can have a chance to fill out all create-only props.
        return false;
      }
    }
    return true;
  }
  return value;
});

/** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
const pageTitle = computed(() => {
  if (isCreate.value) {
    return "Create - " + metadata.value.displayName;
  }

  let display = viewModel ? modelDisplay(viewModel) : null;
  if (!display) {
    return metadata.value.displayName;
  }

  const maxLen = 30;
  if (display.length <= maxLen) {
    return display + " - " + metadata.value.displayName;
  }
  return display.slice(0, maxLen) + "... - " + metadata.value.displayName;
});

const metadata = computed((): ModelType => {
  if (viewModel) {
    return viewModel.$metadata;
  }
  throw `No metadata available.`;
});

const isCreate = computed(() => {
  return !viewModel!.$primaryKey;
});

const hasInstanceMethods = computed(() => {
  return (
    !isCreate.value &&
    metadata.value &&
    Object.values(metadata.value.methods).some((m) => !m.isStatic)
  );
});

function onDeleted() {
  router.back();
}

const params = mapQueryToParams(route.query, ListParameters, metadata.value);
viewModel.$dataSource = params.dataSource;

if (props.id) {
  // Clear the dirty flag before initial load,
  // which may be set if the type has any props with a defaultValue.
  // If we don't clear it, autosave will get confused and try to immediately save.
  viewModel.$isDirty = false;
  viewModel.$load(props.id);
} else {
  copyParamsToNewViewModel(viewModel, params);

  bindKeyToRouteOnCreate(
    instance!,
    viewModel,
    "id",
    /* keep the querystring in case it has data source parameters */ true,
  );
}

watch(
  () => effectiveAutoSave.value,
  (autoSave) => {
    if (autoSave) {
      viewModel.$startAutoSave(instance!);
    } else {
      viewModel.$stopAutoSave();
    }
  },
  { immediate: true },
);

// Expose computed values for external access
defineExpose({
  pageTitle,
  metadata,
  isCreate,
  hasInstanceMethods,
  viewModel,
});
</script>

<style lang="scss">
.c-admin-editor-page {
  max-width: 1300px;

  .c-admin-editor-page--methods {
    margin-top: 30px;
  }

  // Add overscroll to bottom of page so that opening/closing method expansion panels
  // doesn't cause scroll jank
  margin-bottom: 50vh;
}
</style>
