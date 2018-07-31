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
            <v-spacer/>
            <v-btn flat >
              <v-icon left >cancel</v-icon>
              Cancel
            </v-btn>
            
            <v-btn flat color="primary"  
              @click.native="person.$save()" 
              :loading="person.$save.isLoading"
              :disabled="person.$save.isLoading"
            >
              <v-icon left >save</v-icon>
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
          :items="items"
          :search="search"
          :pagination.sync="pagination"
          :total-items="count"
          :loading="isLoading"
          class="elevation-1"
          >
          <template slot="items" slot-scope="props">
            <td v-for="prop in showProps" :key="prop.name" :class="'prop-' + prop.name" >
              <c-display :item="props.item" :prop="prop.name"/>
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

import { PersonViewModel, CaseViewModel } from '../viewmodels.g'
import { PersonApiClient } from '../api-clients.g';


@Component({
  name: 'admin-table-page',
  components: {
    CDisplay, CInput
  }
})
export default class extends Vue {
  metadata = metadata.Person
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

  mounted() {

    this.person.$load(1)
      .then(r => {
        this.person!.$apiClient.removePersonById(200)
        this.person!.$apiClient.fullNameAndAge(1)
        this.person!.$apiClient.personCount("Roberts")
        this.person!.$apiClient.getUser()
      })

      var caller = this.person!.$apiClient.$makeCaller("item", c => c.changeSpacesToDashesInName(1));
      caller.result
    
      //this.person.$startAutoSave(this)
    
  }

  

  items: models.Person[] = [];
}


</script>

