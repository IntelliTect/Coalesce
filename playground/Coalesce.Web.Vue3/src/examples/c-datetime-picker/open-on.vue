<template>
  <h1>openOn</h1>
  <v-row v-for="mode in modes" :key="mode" class="align-center">
    <v-col>
      <c-datetime-picker
        v-model="values[mode]"
        :label="mode"
        :open-on="mode"
        date-kind="date"
        clearable
      ></c-datetime-picker>
    </v-col>
    <v-col :id="'value-' + mode"> {{ values[mode] }} </v-col>
  </v-row>

  <h1>v-model:menu</h1>
  <v-row class="align-center">
    <v-col>
      <c-datetime-picker
        v-model="externalDate"
        v-model:menu="menu"
        label="open-on=none"
        open-on="none"
        clearable
      ></c-datetime-picker>
    </v-col>
    <v-col>
      <v-btn id="toggle-menu" @click="menu = !menu">Toggle menu</v-btn>
    </v-col>
    <v-col id="menu-state"> menu: {{ menu }} </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref } from "vue";

const modes = ["field", "icon", "focus", "picker-only", "none"] as const;

const values = ref<Record<(typeof modes)[number], Date | null>>({
  field: new Date(2026, 5, 6),
  icon: new Date(2026, 5, 6),
  focus: new Date(2026, 5, 6),
  "picker-only": new Date(2026, 5, 6),
  none: new Date(2026, 5, 6),
});

const externalDate = ref<Date | null>(new Date(2026, 5, 6));
const menu = ref(false);
</script>
