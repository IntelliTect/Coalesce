<template>
  <h1>v-model:keyValue (+ query bind)</h1>
  <v-row>
    <v-col>
      <c-select for="Person" v-model:keyValue="selectedId" clearable></c-select>
    </v-col>
    <v-col>
      {{ selectedId }}
    </v-col>
  </v-row>

  <h1>v-model:objectValue</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        v-model:objectValue="selectedModel"
        clearable
      ></c-select>
    </v-col>
    <v-col>
      <div>{{ selectedModel?.name }}</div>
    </v-col>
  </v-row>

  <h1>v-model:keyValue and v-model:objectValue</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        v-model:keyValue="selectedId2"
        v-model:objectValue="selectedModel2"
        clearable
      ></c-select>
    </v-col>
    <v-col>
      <div>{{ selectedId2 }}</div>
      <div>{{ selectedModel2?.name }}</div>
    </v-col>
  </v-row>

  <h1>v-model via c-input</h1>
  <v-row>
    <v-col>
      <c-input for="Person" v-model="selectedModel3" clearable></c-input>
    </v-col>
    <v-col>
      {{ selectedModel3?.name }}
    </v-col>
  </v-row>

  <h1>plain v-autocomplete</h1>
  <v-row>
    <v-col>
      <v-autocomplete
        label="Cats"
        chips
        closableChips
        clearable
        :items="['Grace', 'Freya', 'Autumn', 'Junipur', 'Molly']"
        v-model="selectedPlain"
      />
    </v-col>
    <v-col>
      {{ selectedPlain }}
    </v-col>
  </v-row>

  <h1>c-select with create (start position)</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        v-model:objectValue="selectedWithCreate"
        clearable
        :create="createConfig"
      ></c-select>
    </v-col>
    <v-col>
      <div>{{ selectedWithCreate?.name }}</div>
      <div>ID: {{ selectedWithCreate?.personId }}</div>
    </v-col>
  </v-row>

  <h1>c-select with create (end position)</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        v-model:objectValue="selectedWithCreateEnd"
        clearable
        :create="{ ...createConfig, position: 'end' }"
      ></c-select>
    </v-col>
    <v-col>
      <div>{{ selectedWithCreateEnd?.name }}</div>
      <div>ID: {{ selectedWithCreateEnd?.personId }}</div>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { Person } from "@/models.g";
import { PersonViewModel } from "@/viewmodels.g";
import { modelDisplay, useBindToQueryString } from "coalesce-vue";
import { ref } from "vue";

const selectedId = ref<number>();
useBindToQueryString(selectedId, {
  queryKey: "id",
  parse: parseInt,
});

const selectedModel = ref<Person>();

const selectedId2 = ref<number>();
const selectedModel2 = ref<Person>();

const selectedModel3 = ref<Person>();
const selectedPlain = ref<string>();
const selectedWithCreate = ref<Person>();
const selectedWithCreateEnd = ref<Person>();

const createConfig = {
  getLabel: (search: string, items: Person[]) =>
    search ? `Create person '${search}'` : null,
  getItem: async (search: string, label: string) => {
    // Simulate API call delay
    await new Promise<void>((resolve) => setTimeout(resolve, 1000));
    const person = new PersonViewModel({
      firstName: search,
      companyId: 1,
    });
    await person.$save();
    return person;
  },
};
</script>
