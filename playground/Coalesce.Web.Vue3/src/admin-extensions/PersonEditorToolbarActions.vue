<template>
  <v-btn variant="text" prepend-icon="fa fa-archive" @click="onClick">
    <span class="hidden-sm-and-down">Archive</span>
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel } from "coalesce-vue";

const props = defineProps<{ model: ViewModel; editable: boolean }>();

function onClick() {
  alert(`Archive ${props.model.$display()}`);
}
</script>
