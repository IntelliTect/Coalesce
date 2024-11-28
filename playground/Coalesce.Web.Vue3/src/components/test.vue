<template>
  <v-container grid-list-lg>
    <c-loader-status :loaders="{ 'no-initial-content': [caseVm.$load] }">
      <c-select
        for="Person"
        v-model="people"
        multiple
        density="compact"
        variant="outlined"
      >
      </c-select>

      <v-card class="my-2">
        <v-row>
          <v-col>
            <c-select
              :model="caseVm"
              for="assignedTo"
              variant="outlined"
              density="compact"
              class="my-4"
              error-messages="sdfsdf"
              :params="{useRef: true}"
            >
            </c-select>
          </v-col>
          <v-col>
            <c-input
              class="my-4"
              :model="caseVm"
              for="caseProducts"
              density="compact"
              variant="outlined"
              error-messages="sdfsdf"
            ></c-input>
          </v-col>
        </v-row>
      </v-card>
      <c-input :model="caseVm" for="title"></c-input>
      <c-select-string-value
        :model="caseVm"
        for="title"
        method="getCaseTitles"
      />
      <v-btn @click="disabled = !disabled">Disable Toggle</v-btn>
      <v-form :disabled="disabled">
        <c-datetime-picker
          label="DateTime min/max/step/allowed"
          density="compact"
          variant="outlined"
          date-kind="datetime"
          :min="new Date(1722627824331)"
          :max="new Date(1725034169880)"
          :allowedDates="(v: Date) => v.getDay() > 0 && v.getDay() < 6"
          step="10"
          v-model="date"
          clearable
        />

        <c-datetime-picker
          label="EST"
          density="compact"
          variant="outlined"
          v-model="date"
          timeZone="America/New_York"
        />
      </v-form>

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
      <c-input :model="caseVm" for="openedAt" variant="outlined"> </c-input>

      <br />
      {{ date }}

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
import { Person, Statuses } from "../models.g";

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
  disabled = false;
  date = new Date(1722558611283);
  caseVm = new CaseViewModel();
  people = [];

  async created() {
    this.personList.$dataSource =
      new Person.DataSources.NamesStartingWithAWithCases({
        allowedStatuses: [Statuses.Open, Statuses.InProgress],
      });
    this.personList.$params.noCount = true;
    this.personList.$load();

    this.caseVm.$params.useRef = true;
    await this.caseVm.$load(15);
    await this.caseVm.downloadImage();
    await this.company.$load(1);

    //await this.person.$load(1);
  }

  async mounted() {}

  items: models.Person[] = [];
}
</script>
