<template>
  <h1>c-select slots</h1>
  <v-row>
    <v-col>
      <c-select-many-to-many :model="caseVm" for="caseProducts">
        <template #prepend>
          <v-chip>v-input</v-chip>
        </template>
        <template #append-inner>
          <v-chip>v-field</v-chip>
        </template>
        <template #selected-item="{ item }">
          <div class="bg-purple text-white pa-2 mx-2">{{ item.name }}</div>
        </template>
        <template #list-item="{ item }">
          <div class="bg-indigo text-white pa-2 mx-2">{{ item.name }}</div>
        </template>
      </c-select-many-to-many>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { CaseViewModel, PersonListViewModel } from "@/viewmodels.g";

const caseVm = new CaseViewModel();
caseVm.$load(14);
</script>
