<template>
  <c-input v-bind="$attrs" :model="model" :for="props.for">
    <template #selection>
      <CaseStatusDisplay :model="model" />
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :title="undefined">
        <CaseStatusDisplay :value="item.strValue" />
      </v-list-item>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import type { Model, Property } from "coalesce-vue";
import CaseStatusDisplay from "./CaseStatusDisplay.vue";

const props = defineProps<{
  model: Model<any>;
  for: Property;
}>();
</script>
