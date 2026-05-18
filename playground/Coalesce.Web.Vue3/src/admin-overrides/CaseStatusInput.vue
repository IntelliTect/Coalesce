<template>
  <c-input v-bind="$attrs" :model="model" for="status">
    <template #selection>
      <CaseStatusDisplay :model-value="model.status" />
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :title="undefined">
        <CaseStatusDisplay :model-value="item.strValue" />
      </v-list-item>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import CaseStatusDisplay from "./CaseStatusDisplay.vue";
import { Case } from "@/models.g";

const props = defineProps<{
  model: Case;
}>();
</script>
