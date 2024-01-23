<template>
  <span>123</span>
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
(() => {
  const person = new PersonViewModel();
  person.arbitraryCollectionOfStrings = ["asdf"];
})();
</script>
