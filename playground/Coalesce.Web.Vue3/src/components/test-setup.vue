<template>
  <v-container>
    <c-select
      for="Person"
      :params="{ pageSize: 400 }"
      v-model="person"
    ></c-select>
    <!-- <v-btn @click="add" prepend-icon="fa fa-plus"> Add Product </v-btn>
    <v-btn
      @click="vm.$bulkSave()"
      :loading="vm.$bulkSave.isLoading"
      prepend-icon="fa fa-save"
    >
      Save
    </v-btn>
    <v-btn
      @click="vm.$load()"
      :loading="vm.$load.isLoading"
      prepend-icon="fa fa-sync"
    >
      Load
    </v-btn>
    <input
      v-model="vm.caseKey"
      :disabled="vm.caseKey && !vm.$getPropDirty('caseKey')"
      placeholder="PK to load"
    />

    <c-loader-status :loaders="{ '': [vm.$bulkSave] }"></c-loader-status>
    <h2>Case (pk: {{ vm.$primaryKey }})</h2>

    <c-input :model="vm" for="title"></c-input>
    <c-input :model="vm" for="caseProducts"></c-input>
    <div v-for="x in vm.caseProducts">
      <v-btn
        variant="text"
        size="x-small"
        @click="
          x.product?.$remove();
          x.product = null;
          x.$remove();
        "
        icon="fa fa-trash"
      ></v-btn>
      <v-btn
        variant="text"
        size="x-small"
        @click="
          x.product?.$remove();
          x.product = newProduct();
        "
        icon="fa fa-sync"
      ></v-btn>
      {{ x.product?.name }}: (pk: {{ x.$primaryKey }}, ref {{ x.$stableId }})
    </div> -->
  </v-container>
</template>

<script setup lang="ts">
import { CaseApiClient } from "@/api-clients.g";
import {
  CaseViewModel,
  PersonViewModel,
  ProductViewModel,
} from "@/viewmodels.g";
import { useBindToQueryString } from "coalesce-vue";
import { ref } from "vue";

const person = ref();
// const vm = new CaseViewModel({title: 'asd'});
const vm = new CaseViewModel();
// vm.$load(1);
vm.assignedTo = new PersonViewModel({ firstName: "bob", companyId: 1 });

vm.$bulkSave.setConcurrency("debounce");
async function add() {
  vm.addToCaseProducts({
    product: newProduct(),
  });
}
async function del() {
  vm.caseProducts![0].$remove();
}

function newProduct() {
  return new ProductViewModel({
    name: products[Math.ceil(Math.random() * products.length) - 1],
  });
}

const products = [
  "JavaScript",
  "Python",
  "Java",
  "C#",
  "C++",
  "Ruby",
  "PHP",
  "Swift",
  "Go",
  "TypeScript",
  "Kotlin",
  "Rust",
  "Scala",
  "Perl",
  "HTML/CSS",
  "SQL",
  "Node.js",
  "React.js",
  "Angular",
  "Vue.js",
  "Django",
  "Ruby on Rails",
  "Spring Framework",
  ".NET Core",
  "Express.js",
  "TensorFlow",
  "PyTorch",
  "Unity",
  "Elixir",
  "GraphQL",
];
// const caseApiClient = new CaseApiClient();
// caseApiClient.bulkSave({
//   "items": [
//       { "type": "Case", action: 'save', "data": { "title": "asdf" }, "refs": {"assignedToId": 2}, root: true},
//       { "type": "Person", action: 'save', "data": { "firstName": "bob", "companyId": 1 }, "refs": {"personId": 2}},
//       { "type": "Case", action: 'save', "data": { "caseKey": 1, "title": "case 1" }},
//       // { "type": "Person", action: 'delete', "data": { "personId": 2125 }}
//   ],
// })

const firstName = ref("");
const lastName = ref("");

useBindToQueryString(firstName, "value", "firstName");
useBindToQueryString(lastName, "value", "lastName");

function clearQueryStrings() {
  // If both first & last name are set,
  // only one of the query strings is removed
  // from the URL bar
  firstName.value = "";
  lastName.value = "";
}
</script>
