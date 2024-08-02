<template>
  <v-container grid-list-lg>
    <c-loader-status :loaders="{ 'no-initial-content': [caseVm.$load] }">
      <c-time-picker></c-time-picker>
      <v-card class="my-10">
        <v-row>
          <v-col>
            <c-select
              :model="caseVm"
              for="assignedTo"
              variant="outlined"
              density="compact"
              class="my-4"
              error-messages="sdfsdf"
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
      <c-select-string-value
        :model="caseVm"
        for="title"
        method="getCaseTitles"
        eager
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
        :defaults="{
          VInput: { density: 'compact' },
          VField: { variant: 'outlined' },
        }"
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

  async created() {
    this.personList.$dataSource =
      new Person.DataSources.NamesStartingWithAWithCases({
        allowedStatuses: [Statuses.Open, Statuses.InProgress],
      });
    this.personList.$params.noCount = true;
    this.personList.$load();

    await this.caseVm.$load(15);
    await this.caseVm.downloadImage();
    await this.company.$load(1);

    //await this.person.$load(1);
  }

  async mounted() {}

  items: models.Person[] = [];
}
</script>
