<template>
  <!-- Test that passthrough slots aren't a type error -->
  <c-input :model="vm" for="enumNullable" hide-details="auto" v-bind="$attrs">
    <template #prepend-inner="{ isActive }">
      <span>foo: {{ isActive.value }}</span>
    </template>
    <template #item="slotProps">
      <span>{{
        (() => {
          //@ts-expect-error item.raw is enum metadata, cast to string should be invalid
          slotProps.item.raw as string;

          return slotProps.item.raw.displayName as string;
        })()
      }}</span>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

const vm = new ComplexModelViewModel();
</script>
