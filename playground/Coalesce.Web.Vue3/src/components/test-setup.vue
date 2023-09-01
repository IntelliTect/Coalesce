<template>
  <v-container>
    <v-text-field v-model="firstName" />
    <v-text-field v-model="lastName" />

    <v-btn @click="clearQueryStrings"> Clear </v-btn>
    <v-btn @click="firstName = ''"> Clear 1 </v-btn>
    <v-btn @click="lastName = ''"> Clear 2 </v-btn>
  </v-container>
</template>

<script setup lang="ts">
  import { CaseApiClient } from "@/api-clients.g";
import { CaseViewModel, PersonViewModel } from "@/viewmodels.g";
import { useBindToQueryString } from "coalesce-vue";
  import {ref} from 'vue'

  // const vm = new CaseViewModel({title: 'asd'});
  const vm = new CaseViewModel();
  await vm.$load(1);
  debugger;
  vm.assignedTo = new PersonViewModel({firstName: 'bob', companyId: 1})

  vm.$bulkSave();
  // const caseApiClient = new CaseApiClient();
  // caseApiClient.bulkSave({
  //   "items": [
  //       { "type": "Case", action: 'save', "data": { "title": "asdf" }, "refs": {"assignedToId": 2}, root: true},
  //       { "type": "Person", action: 'save', "data": { "firstName": "bob", "companyId": 1 }, "refs": {"personId": 2}},
  //       { "type": "Case", action: 'save', "data": { "caseKey": 1, "title": "case 1" }},
  //       // { "type": "Person", action: 'delete', "data": { "personId": 2125 }}
  //   ],
// })


  const firstName = ref('');
  const lastName = ref('');

  useBindToQueryString(firstName, "value", "firstName");
  useBindToQueryString(lastName, "value", "lastName");

  function clearQueryStrings() {
    // If both first & last name are set,
    // only one of the query strings is removed
    // from the URL bar
    firstName.value = '';
    lastName.value = '';
  }
</script>