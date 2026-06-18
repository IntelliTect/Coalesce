<template>
  <v-btn variant="tonal" color="info" @click="onClick">
    <v-icon start>fa fa-envelope</v-icon>
    Send Email
  </v-btn>
</template>

<script setup lang="ts">
import { ViewModel } from "coalesce-vue";

const props = defineProps<{ model: ViewModel }>();

function onClick() {
  alert(`Send email to ${props.model.$display}`);
}
</script>
