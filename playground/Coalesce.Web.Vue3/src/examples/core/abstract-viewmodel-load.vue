<template>
  <c-loader-status :loaders="{ '': [vm.$load] }" no-initial-content>
    {{ new Date().valueOf() }}

    {{ vm.id }}
    <div>isLoading: {{ vm.$load.isLoading }}</div>
    <div>wasSuccessful: {{ vm.$load.wasSuccessful }}</div>

    <c-display :model="vm" for="abstractClassString"></c-display>

    {{ vm.$apiClient.$metadata.name }}

    <v-btn @click="vm.$load()"> Load </v-btn>
  </c-loader-status>
</template>

<script setup lang="ts">
import { AbstractClassViewModel } from "@/viewmodels.g";

const vm = new AbstractClassViewModel({});
vm.$load(1);
</script>
