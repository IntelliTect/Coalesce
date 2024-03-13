<template>
  <v-expand-transition>
    <v-expansion-panels v-if="methods.length" class="c-methods">
      <v-toolbar
        class="c-admin-editor-page--toolbar"
        dense
        color="primary darken-1"
        dark
      >
        <v-toolbar-title> Actions </v-toolbar-title>

        <v-divider class="mx-4 my-0" vertical></v-divider>

        <v-toolbar-title v-if="!isStatic">
          <c-display :model="model"></c-display>
        </v-toolbar-title>
      </v-toolbar>
      <v-expansion-panel
        v-for="method in methods"
        :key="method.name"
        :class="'method-' + method.name"
      >
        <v-expansion-panel-header>
          <div>{{ method.displayName }}</div>
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <c-admin-method
            :model="model"
            :for="method"
            :autoReloadModel="autoReloadModel"
          >
          </c-admin-method>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-expand-transition>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import {
  Model,
  ViewModel,
  ModelType,
  ListViewModel,
  HiddenAreas,
} from "coalesce-vue";

export default defineComponent({
  name: "c-admin-methods",

  props: {
    model: {
      required: true,
      type: Object as PropType<ViewModel<Model<ModelType>> | ListViewModel>,
    },
    area: {
      required: false,
      type: Number as PropType<HiddenAreas>,
    },
    autoReloadModel: { required: false, type: Boolean, default: false },
  },

  computed: {
    viewModel(): ViewModel | ListViewModel {
      if (this.model instanceof ViewModel) return this.model;
      if (this.model instanceof ListViewModel) return this.model;
      throw Error(
        "c-method: prop `model` is required, and must be a ViewModel or ListViewModel."
      );
    },

    metadata() {
      return this.viewModel.$metadata as ModelType;
    },

    isStatic() {
      return this.viewModel instanceof ListViewModel;
    },

    methods() {
      if (this.viewModel instanceof ViewModel && !this.viewModel.$primaryKey) {
        return [];
      }

      return Object.values(this.metadata.methods).filter(
        (m) =>
          !!m.isStatic == this.isStatic &&
          (!this.area || ((m.hidden || 0) & this.area) == 0)
      );
    },
  },
});
</script>

<style lang="scss"></style>
