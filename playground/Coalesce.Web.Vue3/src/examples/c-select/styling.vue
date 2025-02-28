<template>
  <h1>prepend/append icon props</h1>
  <c-select
    :model="caseVm"
    for="assignedTo"
    prepend-icon="fa fa-circle-chevron-left"
    prepend-inner-icon="fa fa-chevron-left"
    append-inner-icon="fa fa-chevron-right"
    append-icon="fa fa-circle-chevron-right"
  >
  </c-select>

  <c-select
    :model="caseVm"
    for="assignedTo"
    prepend-icon="fa fa-circle-chevron-left"
    prepend-inner-icon="fa fa-chevron-left"
  >
    <template #append> sdfsdf </template>
  </c-select>

  <h1>slots passthrough</h1>
  <c-select
    :model="caseVm"
    for="assignedTo"
    prepend-icon="fa fa-circle-chevron-left"
    prepend-inner-icon="fa fa-chevron-left"
    append-inner-icon="fa fa-chevron-right"
    append-icon="fa fa-circle-chevron-right"
  >
    <template #prepend>
      <v-chip>v-input</v-chip>
    </template>
    <template #append-inner="{ isActive }">
      <v-chip>v-field (active: {{ isActive.value }})</v-chip>
    </template>
  </c-select>

  <h1>compact/outlined via v-defaults-provider</h1>
  <v-defaults-provider
    :defaults="{
      VInput: { density: 'compact' },
      VField: { variant: 'outlined' },
    }"
  >
    <c-select :model="caseVm" for="assignedTo"> </c-select>
  </v-defaults-provider>

  <h1>compact/outlined via direct props</h1>
  <c-select
    :model="caseVm"
    for="assignedTo"
    density="compact"
    variant="outlined"
  >
  </c-select>
</template>

<script setup lang="ts">
import { Person } from "@/models.g";
import { CaseViewModel } from "@/viewmodels.g";
import { modelDisplay, useBindToQueryString } from "coalesce-vue";
import { ref } from "vue";

const caseVm = new CaseViewModel();
caseVm.$load(15);
caseVm.$useAutoSave();
</script>
