<template>
  <v-btn size="small" variant="outlined" color="warning" @click="onClick">
    <v-icon start>fa fa-archive</v-icon>
    Archive
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel } from "coalesce-vue";

const props = defineProps<{ model: ViewModel }>();

function onClick() {
  alert(`Archive ${props.model.$display}`);
}
</script>
