<template>
  <!-- TODO: these aren't actually tests. Convert them to actual tests (somehow...) -->
  <!-- TODO: Can we figure out how to build some tests against the intellisense experience (e.g. typescript's fourslash tests)  -->
  <div>
    <c-select :model="vm" for="advisor"></c-select>
    <c-select :model="vm" :for="vm.$metadata.props.advisor"></c-select>
    <c-select :model="vm.manyParams" for="model"></c-select>
    <c-select :model="model" for="advisor"></c-select>
    <c-select :model="(model as any)" :for="anyString"></c-select>
    <c-select :model="genericModel" :for="anyString"></c-select>
    <c-select for="Student" v-model="selectedValue"></c-select>

    <!-- ERROR EXPECTED: -->
    <c-select :model="123" for="num"></c-select>
    <c-select :model="vm.manyParams" for="num"></c-select>
    <c-select :model="model" for="name"></c-select>
    <c-select :model="vm" for="name"></c-select>
  </div>
</template>

<script setup lang="ts">
import { Course, Student } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import {
  ApiStateTypeWithArgs,
  ForeignKeyProperty,
  Method,
  KeysOfType,
  Property,
} from "coalesce-vue";
import { Model } from "coalesce-vue";
import { ref } from "vue";
const model = new Student({
  name: "bob",
});
const vm = new StudentViewModel();
const selectedValue = ref<any>();

const anyString: string = "foo";
const genericModel: Model = model;
</script>
