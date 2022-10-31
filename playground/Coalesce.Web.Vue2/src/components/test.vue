<template>
  <v-container grid-list-lg>


    <c-input :model="caseVm" for="openedAt" @input="trace" />
    <c-datetime-picker v-model="caseVm.openedAt" @input="trace" />
    <c-datetime-picker :model="caseVm" for="openedAt" @input="trace" />


    <c-select-string-value :model="caseVm" for="title" method="getCaseTitles" eager />

    {{selectedTitle}}
    <div @click="selectedTitle = null">Reset</div>
    <c-select-string-value for="Case" v-model="selectedTitle" method="getCaseTitles" eager @input="trace" clearable />

    <video v-if="caseVm.caseKey" :src="caseVm.downloadImage.url" controls style="max-width: 100%">
    </video>

    <!--<video v-if="caseVm.caseKey" :src="caseVm.downloadImage.getResultObjectUrl(this)" controls style="max-width: 100%">
  </video>-->

    <c-input :model="person" for="height" />

    <v-layout v-if="person != null">
      <v-flex xs12>
        <v-card>
          <c-display :model="person" for="birthDate" format="yyyy MM dd zzzz" />
          <v-card-actions>
            <v-container>
              <v-layout wrap>
                <v-flex sm6 md4 xl3 v-for="prop in showProps" :key="prop.name" class="py-0">
                  <c-input :model="person" :for="prop" />
                </v-flex>
              </v-layout>
            </v-container>
          </v-card-actions>
          <v-card-actions right>
            <v-spacer />
            <v-btn text>
              <v-icon left>cancel</v-icon>
              Cancel
            </v-btn>
            <v-btn text color="primary"
                   @click.native="person.$save()"
                   :loading="person.$save.isLoading"
                   :disabled="person.$save.isLoading">
              <v-icon left>save</v-icon>
              Save
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-flex>

    </v-layout>

    <v-layout>

      <!--<v-flex xs12>
      <v-data-table
                    v-if="company.employees"
        :headers="headers"
        :items="company.employees"
        :search="search"
        :pagination.sync="pagination"
        :server-items-length="count"
        :loading="isLoading"
        class="elevation-1"
        >
        <template slot="items" slot-scope="props">
            <td>
                <v-btn text color="primary"
                       @click.native="props.item.$save()"
                       :loading="props.item.$save.isLoading"
                       :disabled="props.item.$save.isLoading">
                    <v-icon left>save</v-icon>
                    Save
                </v-btn>
            </td>
          <td v-for="prop in showProps" :key="prop.name" :class="'prop-' + prop.name" >
            <c-input :model="props.item" :for="prop.name"/>
          </td>
        </template>
      </v-data-table>
    </v-flex>-->
    </v-layout>

  </v-container>


</template>

<script lang="ts">

  import { Vue, Component, Watch } from 'vue-property-decorator';
  import { ApiClient, convertToModel, ModelType, ModelApiClient } from 'coalesce-vue'
  import { Person } from '../metadata.g';
  import * as metadata from '../metadata.g';
  import * as models from '../models.g';

  import { PersonViewModel, CaseViewModel, CompanyViewModel, PersonListViewModel } from '../viewmodels.g'
  import { CaseApiClient, PersonApiClient } from '../api-clients.g';


  @Component({
    name: 'admin-table-page',
    components: {}
  })
  export default class Test extends Vue {
    metadata = metadata.Person
    company = new CompanyViewModel();
    person: PersonViewModel = new PersonViewModel();
    personList = new PersonListViewModel();
    isLoading: boolean = false;
    selectedTitle = "";

    caseVm = new CaseViewModel();

    pagination = {
      sortBy: '',
      page: 1,
      rowsPerPage: 10,
      descending: false
    };
    count: number = 0;
    search: string = "";
    nextPage() { }
    previousPage() { }

    get showProps() {
      return Object
        .values(metadata.Person.props)
        .filter(p => p.role != "primaryKey" && p.role != "foreignKey");
    };

    trace(...args: any[]) {
      console.log("trace", args)
    }

    get headers() { return this.showProps.map(o => ({ text: o.displayName, value: o.name })) };

    @Watch('pagination')
    getData() {
      this.isLoading = true;

      new PersonApiClient()
        .list({
          page: this.pagination.page,
          pageSize: this.pagination.rowsPerPage,
          search: this.search,
          orderBy: this.pagination.descending ? undefined : this.pagination.sortBy,
          orderByDescending: this.pagination.descending ? this.pagination.sortBy : undefined,
        })
        .then(res => {
          const listResult = res.data;
          const list = listResult.list;
          this.isLoading = false;
          if (!list) return;

          this.items = list;
          this.pagination.page = listResult.page;
          this.pagination.rowsPerPage = listResult.pageSize;
          this.count = listResult.totalCount;

          // this.person = new PersonViewModel(list[0]);
        })
    }

    async created() {

      this.caseVm.$load(16);
      this.caseVm.$startAutoSave(this);
      //await this.caseVm.downloadImage()
       this.company.$load(1);

      this.person.$load.useResponseCaching();
      await this.person.$load(1)
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

