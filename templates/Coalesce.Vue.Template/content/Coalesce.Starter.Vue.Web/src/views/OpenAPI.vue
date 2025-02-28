<template>
  <div style="height: 100%; width: 100%">
    <v-progress-linear
      v-if="!loaded"
      indeterminate
      color="primary"
      height="10"
    ></v-progress-linear>
    <iframe
      id="openapi-iframe"
      src="/scalar/current"
      frameborder="0"
      style="height: 100%; width: 100%"
      :style="{ visibility: loaded ? 'visible' : 'hidden' }"
      @load="loaded = true"
    ></iframe>
  </div>
</template>

<script setup lang="ts">
const loaded = ref(false);
</script>

<style>
html:has(#openapi-iframe) {
  overflow-y: hidden;
}
</style>
