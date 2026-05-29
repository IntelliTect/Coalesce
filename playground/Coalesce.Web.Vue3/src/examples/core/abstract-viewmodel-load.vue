<template>
  <c-loader-status :loaders="{ '': [loader] }" no-initial-content>
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
import { computed } from "vue";

const loader = AbstractClassViewModel.load(1);
const vm = computed(() => loader.result!);
</script>
