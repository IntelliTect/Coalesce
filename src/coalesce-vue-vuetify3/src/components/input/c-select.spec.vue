<template>
  <!-- TODO: these aren't actually tests. Convert them to actual tests (somehow...) -->
  <!-- TODO: Can we figure out how to build some tests against the intellisense experience (e.g. typescript's fourslash tests)  -->
  <div>
    <c-select :model="vm" for="singleTest" />
    <c-select :model="vm" for="singleTestId" />
    <c-select :model="vm" :for="vm.$metadata.props.singleTest" />
    <c-select :model="vm" :for="vm.$metadata.props.singleTestId" />
    <c-select :model="model" for="singleTest" />
    <c-select :model="model" for="singleTestId" />
    <c-select :model="(model as any)" :for="anyString" />
    <c-select :model="genericModel" :for="anyString" />
    <c-select for="Test" v-model="selectedAny" />
    <c-select :for="ComplexMeta.props.singleTest" v-model="selectedAny" />

    <c-select :model="vm.methodWithManyParams" for="model" />
    <c-select :model="(vm.methodWithManyParams as any)" for="model" />
    <c-select :model="genericCaller" for="model" />
    <c-select :model="genericCaller" for="specificString" />
    <c-select :model="genericCaller" :for="anyString" />

    <c-datetime-picker :model="vm" for="systemDateOnly" />
    <c-datetime-picker :model="model" for="systemTimeOnly" />
    <c-datetime-picker :model="(model as any)" for="specificString" />
    <c-datetime-picker for="ComplexModel.dateTime" v-model="selectedDate" />
    <c-datetime-picker
      :for="model.$metadata.props.dateTimeNullable"
      v-model="selectedDate"
    />
    <c-datetime-picker
      :model="model"
      :for="model.$metadata.props.dateTimeOffset"
    />
    <c-datetime-picker v-model="selectedDate" />

    <c-datetime-picker :model="vm.methodWithManyParams" for="dateTime" />
    <c-datetime-picker
      :model="(vm.methodWithManyParams as any)"
      for="dateTime"
    />
    <c-datetime-picker :model="genericCaller" for="startDate" />
    <c-datetime-picker :model="genericCaller" for="specificString" />
    <c-datetime-picker :model="genericCaller" :for="anyString" />

    <c-select-many-to-many :model="caseVm" for="caseProducts" />
    <c-select-many-to-many
      :model="caseVm"
      :for="caseVm.$metadata.props.caseProducts"
    />

    <!-- ERROR EXPECTED: -->
    <c-select :model="123" for="num" />
    <c-select :model="vm.methodWithManyParams" for="integer" />
    <c-select :model="model" for="name" />
    <c-select :model="vm" for="name" />
    <c-select :for="StudentMeta.props.name" v-model="selectedAny" />

    <c-datetime-picker :model="vm" for="long" />
    <c-datetime-picker :model="vm" for="asdf" />
    <c-datetime-picker :model="vm.methodWithManyParams" for="long" />

    <c-select-many-to-many :model="vm" for="long" />
    <c-select-many-to-many :model="vm" for="asdf" />
    <c-select-many-to-many v-model="selectedAny" />
    <c-select-many-to-many :model="vm" />
    <c-select-many-to-many
      :model="caseVm"
      :for="caseVm.$metadata.props.caseKey"
    />
    <!-- many-to-many MUST always use model/for: -->
    <c-select-many-to-many for="Case" v-model="caseVm.caseProducts" />

    <!-- Nice to have, but currently not supported: -->
    <!-- Reference manytomany by the value given to [manytomanyattribute] -->
    <c-select-many-to-many :model="caseVm" for="products" />
  </div>
</template>

<script setup lang="ts">
import { ComplexModel as ComplexMeta } from "@test/metadata.g";
import { ComplexModel } from "@test/models.g";
import { CaseViewModel, ComplexModelViewModel } from "@test/viewmodels.g";
import { AnyArgCaller, Model } from "coalesce-vue";
import { ref } from "vue";
const model = new ComplexModel({
  name: "bob",
});
const vm = new ComplexModelViewModel();
const caseVm = new CaseViewModel();
const selectedAny = ref<any>();
const selectedDate = ref<Date>();

const anyString: string = "foo";
const genericModel: Model = model;
const genericCaller: AnyArgCaller = vm.methodWithManyParams;
</script>
