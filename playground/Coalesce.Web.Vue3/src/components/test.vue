<template>
  <v-container grid-list-lg>
    <c-loader-status :loaders="{ 'no-initial-content': [caseVm.$load] }">
      <c-select-string-value
        :model="caseVm"
        for="title"
        method="getCaseTitles"
        eager
      />

      <c-datetime-picker
        label="Time"
        density="compact"
        variant="outlined"
        date-kind="time"
        v-model="date"
      />
      <c-datetime-picker
        label="DateTime"
        density="compact"
        variant="outlined"
        date-kind="datetime"
        v-model="date"
        clearable
      />
      <c-datetime-picker
        label="Date"
        density="compact"
        variant="outlined"
        date-kind="date"
        v-model="date"
      />
      <c-datetime-picker
        label="DateTime Native"
        native
        density="compact"
        variant="outlined"
        date-kind="datetime"
        v-model="date"
      />

      {{ date }}
      <c-select for="Person" v-model="caseVm.assignedTo"> </c-select>
      <c-select :model="caseVm" for="assignedTo"> </c-select>
      <c-select :model="caseVm" for="assignedTo" density="compact"> </c-select>
      <c-select
        :model="caseVm"
        for="assignedTo"
        density="compact"
        variant="outlined"
      >
      </c-select>

      <v-defaults-provider
        :defaults="{ VTextField: { variant: 'outlined', density: 'compact' } }"
      >
        <c-select
          :model="caseVm"
          for="assignedTo"
          label="Outlined via v-defaults-provider"
        >
        </c-select>
      </v-defaults-provider>

      <img
        v-if="caseVm.caseKey"
        :src="caseVm.downloadImage.getResultObjectUrl()"
        style="max-width: 100%"
      />
    </c-loader-status>
    <!-- <c-input :model="person" for="height" /> -->
  </v-container>
</template>

<script lang="ts">
import { Base, Component, Watch } from "vue-facing-decorator";
import * as metadata from "../metadata.g";
import * as models from "../models.g";

import {
  PersonViewModel,
  CaseViewModel,
  CompanyViewModel,
  PersonListViewModel,
} from "../viewmodels.g";
import { CaseApiClient, PersonApiClient } from "../api-clients.g";
import { Person } from "../models.g";

@Component({
  components: {},
})
export default class Test extends Base {
  metadata = metadata.Person;
  company = new CompanyViewModel();
  person: PersonViewModel = new PersonViewModel();
  personList = new PersonListViewModel();
  isLoading: boolean = false;
  selectedTitle = null;

  date = null;
  caseVm = new CaseViewModel();

  async created() {
    this.personList.$params.noCount = true;

    await this.caseVm.$load(15);
    await this.caseVm.downloadImage();
    await this.company.$load(1);

    //await this.person.$load(1);
  }

  async mounted() {}

  items: models.Person[] = [];
}
</script>
