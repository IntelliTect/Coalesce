<template>
  <!-- TODO: these aren't actually tests. Convert them to actual tests (somehow...) -->
  <!-- TODO: Can we figure out how to build some tests against the intellisense experience (e.g. typescript's fourslash tests)  -->
  <div>
    <c-select :model="vm" for="advisor" />
    <c-select :model="vm" :for="vm.$metadata.props.advisor" />
    <c-select :model="model" for="advisor" />
    <c-select :model="(model as any)" :for="anyString" />
    <c-select :model="genericModel" :for="anyString" />
    <c-select for="Student" v-model="selectedAny" />
    <c-select :for="StudentMeta.props.advisor" v-model="selectedAny" />

    <c-select :model="vm.manyParams" for="model" />
    <c-select :model="(vm.manyParams as any)" for="model" />
    <c-select :model="genericCaller" for="model" />
    <c-select :model="genericCaller" for="specificString" />
    <c-select :model="genericCaller" :for="anyString" />

    <c-datetime-picker :model="vm" for="birthDate" />
    <c-datetime-picker :model="model" for="birthDate" />
    <c-datetime-picker :model="(model as any)" for="birthDate" />
    <c-datetime-picker for="Student.birthDate" v-model="selectedDate" />
    <c-datetime-picker
      :for="model.$metadata.props.birthDate"
      v-model="selectedDate"
    />
    <c-datetime-picker :model="model" :for="model.$metadata.props.birthDate" />
    <c-datetime-picker v-model="selectedDate" />

    <c-datetime-picker :model="vm.manyParams" for="startDate" />
    <c-datetime-picker :model="(vm.manyParams as any)" for="startDate" />
    <c-datetime-picker :model="genericCaller" for="startDate" />
    <c-datetime-picker :model="genericCaller" for="specificString" />
    <c-datetime-picker :model="genericCaller" :for="anyString" />

    <!-- ERROR EXPECTED: -->
    <c-select :model="123" for="num" />
    <c-select :model="vm.manyParams" for="num" />
    <c-select :model="model" for="name" />
    <c-select :model="vm" for="name" />
    <c-select :for="StudentMeta.props.name" v-model="selectedAny" />

    <c-datetime-picker :model="vm" for="model" />
    <c-datetime-picker :model="vm" for="asdf" />
    <c-datetime-picker :model="vm.manyParams" for="model" />

    <c-select-many-to-many :model="vm" for="model" />
    <c-select-many-to-many :model="vm" for="asdf" />
    <c-select-many-to-many v-model="selectedAny" />
    <c-select-many-to-many :model="vm" />
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
const selectedAny = ref<any>();
const selectedDate = ref<Date>();

const anyString: string = "foo";
const genericModel: Model = model;
const genericCaller: AnyArgCaller = vm.manyParams;
</script>
