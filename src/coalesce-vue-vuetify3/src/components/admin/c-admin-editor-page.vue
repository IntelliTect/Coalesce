<template>
  <v-container class="c-admin-editor-page">
    <c-admin-editor class="c-admin-editor-page--editor" :model="viewModel" />

    <c-admin-methods
      class="c-admin-editor-page--methods"
      :model="viewModel"
      auto-reload-model
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
} from "coalesce-vue";

import { defineComponent } from "vue";
import { useRoute } from "vue-router";

export default defineComponent({
  name: "c-admin-editor-page",

  props: {
    type: { required: true, type: String },
    id: { required: false, type: [String, Number] },
  },

  data() {
    if (!ViewModel.typeLookup![this.type]) {
      // TODO: Bake a `getOrThrow` into `typeLookup`.
      throw Error(
        `No model named ${this.type} is registered to ViewModel.typeLookup`
      );
    }

    return {
      viewModel: new ViewModel.typeLookup![this.type](),
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

  async created() {
    if (this.id) {
      await this.viewModel.$load(this.id);
    } else {
      const params = mapQueryToParams(
        useRoute().query,
        ListParameters,
        this.metadata
      );
      if (params.filter) {
        for (const propName in this.metadata.props) {
          const prop = this.metadata.props[propName];
          const filterValue = params.filter[propName];
          if (filterValue != null) {
            try {
              (this.viewModel as any)[propName] = mapValueToModel(
                filterValue,
                prop
              );
            } catch (e) {
              // mapValueToModel will throw for unmappable values.
              console.error(`Could not map filter parameter ${propName}. ${e}`);
            }
          }
        }
      }

      bindKeyToRouteOnCreate(this, this.viewModel);
    }

    this.viewModel.$startAutoSave(this, { wait: 500 });
  },
});
</script>

<style lang="scss">
.c-admin-editor-page {
  max-width: 1300px;

  .c-admin-editor-page--methods {
    margin-top: 30px;
  }
}
</style>
