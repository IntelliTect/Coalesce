<template>
  <h1>Narrow field, long selection</h1>
  <v-row>
    <v-col cols="3">
      <v-card class="pa-3" title="Default display">
        <c-select v-model:object-value="person1" for="Person" clearable />
      </v-card>
    </v-col>
    <v-col cols="3">
      <v-card class="pa-3" title="#item slot">
        <c-select v-model:object-value="person2" for="Person" clearable>
          <template #item="{ item }">
            {{ item.name }}
            <v-chip size="small" color="green" class="ml-2">Active</v-chip>
          </template>
        </c-select>
      </v-card>
    </v-col>
    <v-col cols="3">
      <v-card class="pa-3" title="Outlined + label">
        <c-select
          v-model:object-value="person3"
          for="Person"
          label="Primary Contact"
          variant="outlined"
          clearable
        >
          <template #item="{ item }">
            {{ item.name }}
            <v-chip size="small" color="green" class="ml-2">Active</v-chip>
          </template>
        </c-select>
      </v-card>
    </v-col>
    <v-col cols="3">
      <v-card class="pa-3" title="Multiple (still wraps)">
        <c-select v-model="people" for="Person" multiple clearable />
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { Person } from "@/models.g";
import { ref } from "vue";

function person(name: string, id: number) {
  return new Person({ personId: id, name });
}

const person1 = ref<Person>(person("Anastasia Wojciechowska-Lindqvist", 1));
const person2 = ref<Person>(person("Bartholomew Fitzgerald-Montgomery", 2));
const person3 = ref<Person>(person("Alexandria Featherstonehaugh-Whitmore", 3));
const people = ref<Person[]>([
  person("Maximilian Van Der Bergstrom", 4),
  person("Christopher Papadopoulos-Rasmussen", 5),
]);
</script>
