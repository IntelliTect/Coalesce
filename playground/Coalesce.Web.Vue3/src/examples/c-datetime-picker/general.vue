<template>
  <h1>Date Only + date-picker-props</h1>
  <v-row>
    <v-col>
      <c-datetime-picker
        :model="vm"
        for="birthDate"
        clearable
        :date-picker-props="{
          firstDayOfWeek: 3,
          viewMode: 'year',
        }"
        showTodayButton
      ></c-datetime-picker>
    </v-col>
    <v-col>
      {{ vm.birthDate }}
    </v-col>
  </v-row>

  <h1>Time Only</h1>
  <v-row>
    <v-col>
      <c-datetime-picker
        :model="vm"
        for="nextUpgrade"
        date-kind="time"
        clearable
        :step="15"
        color="pink"
      ></c-datetime-picker>
    </v-col>
    <v-col>
      {{ vm.nextUpgrade }}
    </v-col>
  </v-row>

  <h1>Date + Time Only</h1>
  <v-row>
    <v-col>
      <c-datetime-picker
        :model="vm"
        for="lastBath"
        color="purple"
        showTodayButton
      ></c-datetime-picker>
    </v-col>
    <v-col>
      {{ vm.lastBath }}
    </v-col>
  </v-row>

  <h1>Vuetify icons + slots</h1>
  <v-row>
    <v-col>
      <c-datetime-picker
        :model="vm"
        for="lastBath"
        color="purple"
        prepend-inner-icon="fa fa-exclamation-circle"
      >
        <template #append-inner="{ isFocused }">
          <v-chip>append-inner (focused: {{ isFocused.value }})</v-chip>
        </template>
      </c-datetime-picker>
    </v-col>
    <v-col>
      prepend-inner-icon (prop)
      <br />
      append-inner (slot)
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { PersonViewModel } from "@/viewmodels.g";

const vm = new PersonViewModel();
vm.$load(15);
</script>
