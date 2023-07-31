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
  import { useBindToQueryString } from "coalesce-vue";
  import {ref} from 'vue'

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