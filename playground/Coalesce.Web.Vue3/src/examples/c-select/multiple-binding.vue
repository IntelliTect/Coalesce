<template>
  <h1>v-model:keyValue (+ query bind)</h1>
  <v-row>
    <v-col>
      <c-select for="Person" multiple v-model:keyValue="selectedIds"></c-select>
    </v-col>
    <v-col>
      {{ selectedIds }}
    </v-col>
  </v-row>

  <h1>v-model:objectValue (+ create)</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        multiple
        v-model:objectValue="selectedModels"
        open-on-clear
        :create="{
          getLabel(search: string, items: Person[]) { return items.length == 0 ? search : false },
          async getItem(search: string, label: string) { return new Person({ name: label }) }
        }"
      ></c-select>
    </v-col>
    <v-col>
      [{{ selectedModels.map((x) => modelDisplay(x)).join(", ") }}]
    </v-col>
  </v-row>

  <h1>v-model:keyValue and v-model:objectValue (+ rules)</h1>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        multiple
        v-model:keyValue="selectedIds2"
        v-model:objectValue="selectedModels2"
        :rules="[(v) => `Rule test: rule received ${v}`]"
      ></c-select>
    </v-col>
    <v-col>
      <div>{{ selectedIds2 }}</div>
      <div>[{{ selectedModels2.map((x) => modelDisplay(x)).join(", ") }}]</div>
    </v-col>
  </v-row>

  <h1>for method parameter</h1>
  <v-row>
    <v-col>
      <c-select :model="personList.methodWithEntityParameter" for="people" />
      <h3>Via c-input</h3>
      <c-input :model="personList.methodWithEntityParameter" for="people" />
    </v-col>
    <v-col>
      <v-btn @click="personList.methodWithEntityParameter.invokeWithArgs()"
        >Execute</v-btn
      >
      <div>
        [{{
          personList.methodWithEntityParameter.args.people
            ?.map((x) => modelDisplay(x))
            .join(", ")
        }}]
      </div>
    </v-col>
  </v-row>

  <h1>plain v-autocomplete multiple</h1>
  <v-row>
    <v-col>
      <v-autocomplete
        multiple
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
import { Case, Person } from "@/models.g";
import { PersonListViewModel } from "@/viewmodels.g";
import { modelDisplay, useBindToQueryString } from "coalesce-vue";
import { ref } from "vue";

const selectedIds = ref<number[]>([]);
useBindToQueryString(selectedIds, {
  queryKey: "ids",
  parse: JSON.parse,
  stringify: JSON.stringify,
});

const personList = new PersonListViewModel();
personList.methodWithEntityParameter.args.person = new Person({ companyId: 1 });

const caseVm = new Case();
const selectedModels = ref<Person[]>([]);

const selectedIds2 = ref<number[]>([]);
const selectedModels2 = ref<Person[]>([]);

const selectedModels3 = ref<Person[]>([]);
const selectedPlain = ref<string[]>([]);
</script>
