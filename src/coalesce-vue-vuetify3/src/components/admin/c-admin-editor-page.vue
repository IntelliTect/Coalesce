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

<script lang="ts">
import {
  ViewModel,
  ModelType,
  ListParameters,
  mapQueryToParams,
  mapValueToModel,
  bindKeyToRouteOnCreate,
  modelDisplay,
  HiddenAreas,
} from "coalesce-vue";

import { PropType, computed, defineComponent, watch } from "vue";
import { useRoute } from "vue-router";
import { isPropReadOnly } from "../../util";
import { copyParamsToNewViewModel } from "./util";

export default defineComponent({
  name: "c-admin-editor-page",

  props: {
    type: { required: true, type: String },
    id: { required: false, type: [String, Number] },
    color: { required: false, type: String, default: null },
    autoSave: {
      required: false,
      type: [String, Boolean] as PropType<"auto" | boolean>,
      default: "auto",
    },
  },

  setup(props) {
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

    return {
      viewModel,
      HiddenAreas,
      effectiveAutoSave,
    };
  },

  computed: {
    /** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
    pageTitle() {
      if (this.isCreate) {
        return "Create - " + this.metadata.displayName;
      }

      let display = this.viewModel ? modelDisplay(this.viewModel) : null;
      if (!display) {
        return this.metadata.displayName;
      }

      const maxLen = 30;
      if (display.length <= maxLen) {
        return display + " - " + this.metadata.displayName;
      }
      return display.slice(0, maxLen) + "... - " + this.metadata.displayName;
    },

    metadata(): ModelType {
      if (this.viewModel) {
        return this.viewModel.$metadata;
      }
      throw `No metadata available.`;
    },

    isCreate() {
      return !this.viewModel!.$primaryKey;
    },

    hasInstanceMethods() {
      return (
        !this.isCreate &&
        this.metadata &&
        Object.values(this.metadata.methods).some((m) => !m.isStatic)
      );
    },
  },

  methods: {
    onDeleted() {
      this.$router.back();
    },
  },

  async created() {
    const params = mapQueryToParams(
      useRoute().query,
      ListParameters,
      this.metadata,
    );
    this.viewModel.$dataSource = params.dataSource;

    if (this.id) {
      await this.viewModel.$load(this.id);
    } else {
      copyParamsToNewViewModel(this.viewModel, params);

      bindKeyToRouteOnCreate(
        this,
        this.viewModel,
        "id",
        /* keep the querystring in case it has data source parameters */ true,
      );
    }

    watch(
      () => this.effectiveAutoSave,
      (autoSave) => {
        if (autoSave) {
          this.viewModel.$startAutoSave(this, { wait: 500 });
        } else {
          this.viewModel.$stopAutoSave();
        }
      },
      { immediate: true },
    );
  },
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
