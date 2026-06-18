<template>
  <v-btn size="x-small" variant="text" color="info" @click="onClick">
    <v-icon>fa fa-eye</v-icon>
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel, ListViewModel } from "coalesce-vue";

const props = defineProps<{ model: ViewModel; list: ListViewModel }>();

function onClick() {
  alert(`Quick view: ${props.model.$display}`);
}
</script>
