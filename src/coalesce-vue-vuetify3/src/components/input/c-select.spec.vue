<template>
  <!-- TODO: these aren't actually tests. Convert them to actual tests (somehow...) -->
  <!-- TODO: Can we figure out how to build some tests against the intellisense experience (e.g. typescript's fourslash tests)  -->
  <div>
    <c-select :model="vm" for="advisor"></c-select>
    <c-select :model="vm" :for="vm.$metadata.props.advisor"></c-select>
    <c-select :model="model" for="advisor"></c-select>
    <c-select :model="(model as any)" :for="anyString"></c-select>
    <c-select :model="genericModel" :for="anyString"></c-select>
    <c-select for="Student" v-model="selectedValue"></c-select>
    <c-select
      :for="StudentMeta.props.advisor"
      v-model="selectedValue"
    ></c-select>

    <c-select :model="vm.manyParams" for="model"></c-select>
    <c-select :model="(vm.manyParams as any)" for="model"></c-select>
    <c-select :model="genericCaller" for="model"></c-select>
    <c-select :model="genericCaller" for="specificString"></c-select>
    <c-select :model="genericCaller" :for="anyString"></c-select>

    <!-- ERROR EXPECTED: -->
    <c-select :model="123" for="num"></c-select>
    <c-select :model="vm.manyParams" for="num"></c-select>
    <c-select :model="model" for="name"></c-select>
    <c-select :model="vm" for="name"></c-select>
    <c-select :for="StudentMeta.props.name" v-model="selectedValue"></c-select>
  </div>
</template>

<script setup lang="ts">
import { Student as StudentMeta } from "@test/targets.metadata";
import { Course, Student } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import {
  ApiStateTypeWithArgs,
  ForeignKeyProperty,
  Method,
  KeysOfType,
  Property,
  ItemApiStateWithArgs,
  AnyArgCaller,
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
const genericCaller: AnyArgCaller = vm.manyParams;
</script>
