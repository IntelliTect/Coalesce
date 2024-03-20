<template>
  <div>
    <span>123</span>
    <v-btn @click="reset">Reset</v-btn>
    <v-btn @click="person.companyId = null">Reset direct</v-btn>

    <c-select :model="person" for="company"> </c-select>

    <v-btn @click="person.birthDate = null">Reset direct</v-btn>
    <c-input :model="person" for="birthDate"> </c-input>
  </div>
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
import { useRouter } from "vue-router";

const router = useRouter();

const person = new PersonViewModel();
person.arbitraryCollectionOfStrings = ["asdf"];

useBindToQueryString(person, "companyId", "companyId", parseInt, "replace");
function reset() {
  router.replace({ query: {} });
}
</script>
