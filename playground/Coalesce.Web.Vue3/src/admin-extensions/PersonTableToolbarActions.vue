<template>
  <v-btn size="small" variant="outlined" color="secondary" @click="onClick">
    <v-icon start>fa fa-download</v-icon>
    Export
  </v-btn>
</template>

<script setup lang="ts">
import { ListViewModel } from "coalesce-vue";

const props = defineProps<{ list: ListViewModel }>();

function onClick() {
  alert(`Export ${props.list.$items.length} items`);
}
</script>
