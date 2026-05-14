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

  <!-- Test that item slot is typed correctly for string-format enums -->
  <c-input :model="vm" for="stringEnum" hide-details="auto">
    <template #item="slotProps">
      <span>{{
        (() => {
          //@ts-expect-error item.raw is enum metadata, cast to string should be invalid
          slotProps.item.raw as string;

          return slotProps.item.raw.displayName as string;
        })()
      }}</span>
    </template>
    <template #selection="slotProps">
      <span>{{
        (() => {
          //@ts-expect-error item.raw is enum metadata, cast to string should be invalid
          slotProps.item.raw as string;

          return slotProps.item.raw.strValue as string;
        })()
      }}</span>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

const vm = new ComplexModelViewModel();
</script>
