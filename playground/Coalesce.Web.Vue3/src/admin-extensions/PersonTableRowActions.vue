<template>
  <v-btn variant="text" icon title="Quick View" @click="onClick">
    <i aria-hidden="true" class="v-icon notranslate fa fa-eye"></i>
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel, ListViewModel } from "coalesce-vue";

const props = defineProps<{
  model: ViewModel;
  list: ListViewModel;
  editable: boolean;
}>();

function onClick() {
  alert(`Quick view: ${props.model.$display()}`);
}
</script>
