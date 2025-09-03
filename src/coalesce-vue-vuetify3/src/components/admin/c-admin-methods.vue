<template>
  <v-card class="c-admin-methods" v-if="methods.length">
    <v-toolbar
      class="c-admin-methods--toolbar"
      density="compact"
      :color="color"
    >
      <v-toolbar-title style="flex: 0 1 auto"> Actions </v-toolbar-title>

      <v-divider class="mx-4 my-0" vertical></v-divider>

      <v-toolbar-title v-if="'$primaryKey' in viewModel">
        <c-display :model="viewModel"></c-display>
      </v-toolbar-title>
    </v-toolbar>
    <v-expansion-panels class="c-methods" variant="accordion">
      <v-expansion-panel
        v-for="method in methods"
        :key="method.name"
        :class="'method-' + method.name"
      >
        <v-expansion-panel-title static>
          <div>
            <div>{{ method.displayName }}</div>

            <div v-if="method.description" class="c-admin-method--description">
              {{ method.description }}
            </div>
          </div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <c-admin-method
            :model="model"
            :for="method"
            :autoReloadModel="autoReloadModel"
          >
          </c-admin-method>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from "vue";
import {
  ViewModel,
  ModelType,
  ListViewModel,
  HiddenAreas,
  ServiceViewModel,
} from "coalesce-vue";

type AnyViewModel = ViewModel | ListViewModel | ServiceViewModel;

defineOptions({
  name: "c-admin-methods",
});

const props = defineProps<{
  model: AnyViewModel;
  area?: HiddenAreas;
  color?: string;
  autoReloadModel?: boolean;
}>();

const viewModel = computed((): AnyViewModel => {
  const model = props.model;
  if (model instanceof ViewModel) return model;
  if (model instanceof ListViewModel) return model;
  if (model instanceof ServiceViewModel) return model;
  throw Error("c-method: prop `model` is required, and must be a ViewModel.");
});

const metadata = computed(() => {
  return viewModel.value.$metadata as ModelType;
});

const isStatic = computed(() => {
  return viewModel.value instanceof ListViewModel;
});

const methods = computed(() => {
  if (viewModel.value instanceof ViewModel && !viewModel.value.$primaryKey) {
    return [];
  }

  return Object.values(metadata.value.methods).filter(
    (m) =>
      !!m.isStatic == isStatic.value &&
      (!props.area || ((m.hidden || 0) & props.area) == 0),
  );
});
</script>

<style lang="scss">
.c-admin-method--description {
  font-size: 12px;
  opacity: 0.7;
  padding-top: 4px;
}
</style>
