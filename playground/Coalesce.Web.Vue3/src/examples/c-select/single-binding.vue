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
</template>

<script setup lang="ts">
import { Person } from "@/models.g";
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
</script>
