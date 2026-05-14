<template>
  <!-- Test that passthrough slots aren't a type error -->
  <c-input :model="vm" for="enumNullable" hide-details="auto" v-bind="$attrs">
    <template #prepend-inner="{ isActive }">
      <span>foo: {{ isActive.value }}</span>
    </template>
    <template #item="slotProps">
      <span>{{
        (() => {
          //@ts-expect-error item is enum metadata, cast to string should be invalid
          slotProps.item satisfies string;

          // c-select slots augment metadata to use real enum model types
          slotProps.item.value satisfies Statuses;
          slotProps.item.strValue satisfies string;

          //@ts-expect-error strValue is string, but Statuses is numeric
          slotProps.item.strValue satisfies Statuses;

          return slotProps.item.displayName as string;
        })()
      }}</span>
    </template>
  </c-input>

  <c-input :model="vm" for="stringEnum" hide-details="auto" v-bind="$attrs">
    <template #prepend-inner="{ isActive }">
      <span>foo: {{ isActive.value }}</span>
    </template>
    <template #item="slotProps">
      <span>{{
        (() => {
          //@ts-expect-error item is enum metadata, cast to string should be invalid
          slotProps.item satisfies string;

          // c-select slots augment metadata to use real enum model types
          slotProps.item.value satisfies number;
          slotProps.item.strValue satisfies StringSerializedEnum;

          //@ts-expect-error value is numeric, but enum is string
          slotProps.item.value satisfies StringSerializedEnum;

          return slotProps.item.displayName as string;
        })()
      }}</span>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import { Statuses, StringSerializedEnum } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

const vm = new ComplexModelViewModel();
</script>
