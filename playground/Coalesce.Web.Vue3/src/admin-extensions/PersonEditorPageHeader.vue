<template>
  <v-alert type="warning" variant="tonal" density="compact" class="mb-4">
    Editing person record. Changes are auto-saved.
  </v-alert>
</template>

<script setup lang="ts">
import { ViewModel } from "coalesce-vue";

defineProps<{ model: ViewModel }>();
</script>
