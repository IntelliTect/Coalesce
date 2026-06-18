<template>
  <v-alert type="info" variant="tonal" density="compact" class="mb-4">
    Manage all people in the system. Use the toolbar to export or filter.
  </v-alert>
</template>

<script setup lang="ts">
import { ListViewModel } from "coalesce-vue";

defineProps<{ list: ListViewModel }>();
</script>
