<template>
  <v-container grid-list-lg>

    <c-loader-status :loaders="{'no-initial-content': [caseVm.$load]}">
      <c-select-string-value :model="caseVm" for="title" method="getCaseTitles" eager />

      <c-select :model="caseVm" for="assignedTo"> </c-select>
      <c-select :model="caseVm" for="assignedTo" density="compact"> </c-select>
      <c-select :model="caseVm" for="assignedTo" density="compact" variant="outlined"> </c-select>

      <img v-if="caseVm.caseKey" :src="caseVm.downloadImage.getResultObjectUrl()" style="max-width: 100%" />

      <c-admin-table :list="personList"></c-admin-table>

      <!--<video v-if="caseVm.caseKey" :src="caseVm.downloadImage.getResultObjectUrl(this)" controls style="max-width: 100%">
  </video>-->
      <!--
  <video v-if="caseVm.caseKey" :src="caseVm.downloadImage.url" controls style="max-width: 100%">
  </video> -->
      <v-form>
        <c-input :model="caseVm" for="title"></c-input>
        <c-input :model="caseVm" for="description" textarea placeholder="asdf"></c-input>
        <c-input :model="caseVm" for="openedAt"></c-input>
        <c-input :model="caseVm" for="assignedTo" disabled></c-input>
        <c-select :model="caseVm" for="assignedTo" readonly></c-select>
        <c-select for="Person" v-model="caseVm.assignedTo" placeholder="asdf"></c-select>
        <c-input :model="caseVm" for="reportedBy" placeholder="asdf"></c-input>
        <c-input :model="caseVm" for="attachmentSize"></c-input>
        <c-input :model="caseVm" for="severity"></c-input>
        <c-input :model="caseVm" for="status"></c-input>
        <c-input :model="caseVm" for="caseProducts"></c-input>
        <c-display :model="caseVm" for="title" />
      </v-form>
      <v-text-field v-model="caseVm.title" label="vuetify direct"></v-text-field>
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

  caseVm = new CaseViewModel();

  pagination = {
    sortBy: "",
    page: 1,
    rowsPerPage: 10,
    descending: false,
  };
  count: number = 0;
  search: string = "";
  nextPage() {}
  previousPage() {}

  createMethods = {
    getLabel(search: string, items: Person[]) {
      const searchLower = search.toLowerCase();
      if (items.some((a) => a.name?.toLowerCase().indexOf(searchLower) == 0)) {
        return false;
      }
      return search;
    },
    async getItem(search: string, label: string) {
      var vm = new PersonViewModel();
      vm.firstName = label;
      vm.height = 2;
      vm.companyId = 3;
      await vm.$save();
      return vm;
    },
  };

  async delete() {
    console.log("delete");
  }

  get showProps() {
    return Object.values(metadata.Person.props).filter(
      (p) => p.role != "primaryKey" && p.role != "foreignKey"
    );
  }

  get headers() {
    return this.showProps.map((o) => ({ text: o.displayName, value: o.name }));
  }

  @Watch("pagination")
  getData() {
    this.isLoading = true;

    new PersonApiClient()
      .list({
        page: this.pagination.page,
        pageSize: this.pagination.rowsPerPage,
        search: this.search,
        orderBy: this.pagination.descending
          ? undefined
          : this.pagination.sortBy,
        orderByDescending: this.pagination.descending
          ? this.pagination.sortBy
          : undefined,
      })
      .then((res) => {
        const listResult = res.data;
        const list = listResult.list;
        this.isLoading = false;
        if (!list) return;

        this.items = list;
        this.pagination.page = listResult.page ?? 0;
        this.pagination.rowsPerPage = listResult.pageSize ?? 0;
        this.count = listResult.totalCount ?? 0;

        // this.person = new PersonViewModel(list[0]);
      });
  }

  async created() {
    this.personList.$params.noCount = true;

    await this.caseVm.$load(15);
    await this.caseVm.downloadImage(),
    this.caseVm.$startAutoSave(this);
    await this.company.$load(1);

    //await this.person.$load(1);
  }

  async mounted() {
    //new CaseViewModel({ caseKey: 1 }).uploadByteArray(new Uint8Array([60, 61, 62, 63]))
    //new CaseViewModel({ caseKey: 1 }).uploadByteArray("abcd")
    //var caller = this.person!.$apiClient.$makeCaller("item", c => c.changeSpacesToDashesInName(1));
    //caller.result
    //this.person.$startAutoSave(this, { wait: 0 })
  }

  items: models.Person[] = [];
}
</script>
