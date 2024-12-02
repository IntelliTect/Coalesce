<template>
  <h1>model/for</h1>
  <v-row>
    <v-col>
      <c-select-many-to-many :model="caseVm" for="caseProducts" />
    </v-col>
    <v-col>
      <v-btn @click="caseVm.$bulkSave()" :loading="caseVm.$bulkSave.isLoading">
        Save
      </v-btn>
      <br />
      <c-display :model="caseVm" for="caseProducts"></c-display>
    </v-col>
  </v-row>

  <h1>autosave</h1>
  <v-row>
    <v-col>
      <c-select-many-to-many :model="caseVm2" for="caseProducts" clearable />
    </v-col>
    <v-col>
      <c-display :model="caseVm2" for="caseProducts"></c-display>
    </v-col>
  </v-row>

  <h1>far-side not loaded</h1>
  <v-row>
    <v-col>
      <c-select-many-to-many :model="caseVm3" for="caseProducts" />
    </v-col>
    <v-col>
      <c-display :model="caseVm3" for="caseProducts"></c-display>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { Case, Person } from "@/models.g";
import { CaseViewModel, PersonListViewModel } from "@/viewmodels.g";
import { modelDisplay, useBindToQueryString } from "coalesce-vue";
import { ref } from "vue";

const caseVm = new CaseViewModel();
caseVm.$load(14);

const caseVm2 = new CaseViewModel();
caseVm2.$load(15);
caseVm2.$useAutoSave();

const caseVm3 = new CaseViewModel();
caseVm3.$dataSource = new CaseViewModel.DataSources.MissingManyToManyFarSide();
caseVm3.$load(15);
</script>
