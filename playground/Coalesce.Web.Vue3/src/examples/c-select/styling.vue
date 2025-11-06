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

  <h1>c-select slots</h1>
  <c-select for="Person" multiple v-model="people" variant="outlined">
    <!-- c-select specific slots -->
    <template #selected-item="{ item, index, remove }">
      <v-chip
        size="small"
        closable
        @click:close="remove"
        color="purple"
        class="ma-1"
      >
        <v-icon start>fa fa-star</v-icon>
        {{ item.name }} (#{{ index + 1 }})
      </v-chip>
    </template>

    <template #list-item="{ item, search, selected }">
      <div class="d-flex align-center">
        <v-avatar size="24" color="primary" class="mr-2">
          <span class="text-caption">{{ item.name?.[0] }}</span>
        </v-avatar>
        <div>
          <div class="font-weight-bold">{{ item.name }}</div>
          <div class="text-caption text-grey">
            ID: {{ item.personId }} |
            {{ selected ? "✓ Selected" : "Not selected" }}
            {{ search ? `| Search: "${search}"` : "" }}
          </div>
        </div>
      </div>
    </template>
  </c-select>

  <h1>compact/outlined via v-defaults-provider</h1>
  <v-defaults-provider
    :defaults="{
      VTextField: { density: 'compact', variant: 'outlined' },
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

  <h1>disabled</h1>
  <v-row>
    <v-col cols="6">
      <h2>c-select</h2>
      <c-select :model="caseVm" for="assignedTo" disabled variant="outlined" />
    </v-col>
    <v-col cols="6">
      <h2>v-select</h2>
      <v-select :model="caseVm" for="assignedTo" disabled variant="outlined" />
    </v-col>
  </v-row>
  <h1>readonly</h1>
  <v-row>
    <v-col cols="6">
      <c-select :model="caseVm" for="assignedTo" readonly variant="outlined" />
    </v-col>
    <v-col cols="6">
      <v-select :model="caseVm" for="assignedTo" readonly variant="outlined" />
    </v-col>
  </v-row>

  <h1>readonly multiple</h1>
  <v-row>
    <v-col cols="6">
      <c-input :model="caseVm" for="caseProducts" readonly variant="outlined" />
    </v-col>
  </v-row>

  <h1>manual tabindex</h1>
  <v-row>
    <v-col cols="6">
      <c-input
        :model="caseVm"
        for="assignedTo"
        variant="outlined"
        tabindex="1"
      />
      <v-text-field tabindex="2" label="plain text field"></v-text-field>
      <c-input
        :model="caseVm"
        for="assignedTo"
        variant="outlined"
        tabindex="3"
      />
    </v-col>
  </v-row>

  <h1>autofocus in v-dialog</h1>
  <v-dialog v-model="dialogOpen" max-width="500">
    <template #activator="{ props }">
      <v-btn v-bind="props">Open Dialog</v-btn>
    </template>
    <v-card>
      <v-card-title>Select Person</v-card-title>
      <v-card-text>
        <c-select :model="caseVm" for="assignedTo" autofocus></c-select>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { Person } from "@/models.g";
import { CaseViewModel, PersonViewModel } from "@/viewmodels.g";
import { modelDisplay, useBindToQueryString } from "coalesce-vue";
import { reactive, ref } from "vue";

const caseVm = new CaseViewModel();
caseVm.$load(15);
caseVm.$useAutoSave();

const people = ref<Person[]>([]);

const dialogOpen = ref(false);
</script>
