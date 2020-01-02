<template>
  <v-container grid-list-lg>
    <v-layout v-if="person != null" >
      <v-flex xs12>
        <v-card >
          <v-card-actions>
            <v-container >
              <v-layout  wrap >
                <v-flex sm6 md4 xl3 v-for="prop in showProps" :key="prop.name" class="py-0">
                  <c-input :item="person" :prop="prop"/>
                </v-flex>
              </v-layout>
            </v-container>
          </v-card-actions>
          <v-card-actions right>
              <v-spacer />
              <v-btn flat>
                  <v-icon left>cancel</v-icon>
                  Cancel
              </v-btn>

              {{person.$save.isLoading}}
              <v-btn flat color="primary"
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
    <v-layout >

      <v-flex xs12>
        <v-data-table
          :headers="headers"
          :items="company.employeesViewModels"
          :search="search"
          :pagination.sync="pagination"
          :total-items="count"
          :loading="isLoading"
          class="elevation-1"
          >
          <template slot="items" slot-scope="props">
              <td>
                  <v-btn flat color="primary"
                         @click.native="props.item.$save()"
                         :loading="props.item.$save.isLoading"
                         :disabled="props.item.$save.isLoading">
                      <v-icon left>save</v-icon>
                      Save
                  </v-btn>
              </td>
            <td v-for="prop in showProps" :key="prop.name" :class="'prop-' + prop.name" >
              <c-input :item="props.item" :prop="prop.name"/>
            </td>
          </template>
        </v-data-table>
      </v-flex>
    </v-layout>
    
  </v-container>


</template>

<script lang="ts">

import { Vue, Component, Watch } from 'vue-property-decorator';
import { CDisplay, CInput } from '../coalesce/components';
import { ApiClient, convertToModel, ModelType, ModelApiClient } from 'coalesce-vue'
import { Person } from '../metadata.g';
import * as metadata from '../metadata.g';
import * as models from '../models.g';

import { PersonViewModel, CaseViewModel, CompanyViewModel } from '../viewmodels.g'
import { PersonApiClient } from '../api-clients.g';


@Component({
  name: 'admin-table-page',
  components: {
    CDisplay, CInput
  }
})
export default class extends Vue {
    metadata = metadata.Person
    company = new CompanyViewModel();
  person: PersonViewModel = new PersonViewModel();
  isLoading: boolean = false;
  
  pagination = {
    sortBy: '', 
    page: 1, 
    rowsPerPage: 10, 
    descending: false
  };
  count: number = 0;
  search: string = "";
  nextPage(){}
  previousPage(){}

  get showProps() { return Object
    .values(metadata.Person.props)
    .filter(p => p.role != "primaryKey" && p.role != "foreignKey"); 
  };

  get headers() { return this.showProps.map(o => ({text: o.displayName, value: o.name})) };

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

    async mounted() {
        await this.company.$load(1);

        await this.person.$load(1)

          //var caller = this.person!.$apiClient.$makeCaller("item", c => c.changeSpacesToDashesInName(1));
          //caller.result
    
          this.person.$startAutoSave(this)
    
  }

  

  items: models.Person[] = [];
}


</script>

