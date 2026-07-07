<template>
  <v-btn variant="text" @click="onClick">
    <v-icon start>fa fa-download</v-icon>
    <span class="hidden-sm-and-down">Export</span>
  </v-btn>
</template>

<script setup lang="ts">
import { ListViewModel } from "coalesce-vue";

const props = defineProps<{ list: ListViewModel; editable: boolean | null }>();

function onClick() {
  alert(`Export ${props.list.$items.length} items`);
}
</script>
