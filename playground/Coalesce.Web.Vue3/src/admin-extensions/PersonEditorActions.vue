<template>
  <v-btn variant="text" prepend-icon="fa fa-envelope" @click="onClick">
    <span class="hidden-sm-and-down">Send Email</span>
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel } from "coalesce-vue";

const props = defineProps<{ model: ViewModel; editable: boolean }>();

function onClick() {
  alert(`Send email to ${props.model.$display()}`);
}
</script>
