<template>
  <!-- Test that passthrough slots aren't a type error -->
  <c-input :model="vm" for="enumNullable" hide-details="auto" v-bind="$attrs">
    <template #prepend-inner>
      <span>foo</span>
    </template>
    <template #item="item">
      <span>{{ item.props.value }}</span>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

const vm = new ComplexModelViewModel();
</script>
