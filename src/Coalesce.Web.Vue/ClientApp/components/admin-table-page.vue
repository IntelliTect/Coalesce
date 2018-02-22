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
            <v-btn flat color="primary">
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
  
  
  import * as moment from 'moment';
  import { Vue, Component, Watch } from 'vue-property-decorator';
  import { CDisplay, CInput } from '../coalesce/components';
  import { hydrateMetadata } from '../coalesce'
  import * as $metadata from '../model.g';

  @Component({
    name: 'admin-table-page',
    components: {
      CDisplay, CInput
    }
  })
  export default class extends Vue {
    metadata = $metadata.Person
    person = null;

    selected: any[] = [{name: "Steve"}];
    loading= false;
    inputItems = this.selected.slice();
    ddSearch = "";
    @Watch('ddSearch')
    queryDropdownItems() {
      this.loading= true;
      fetch(`http://localhost:11202/api/Person/List?pageSize=500&search=${this.ddSearch}`)
        .then(response => response.json())
        .then(resp => {
          this.inputItems = resp.list;
          this.loading = false;
        });
    }

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
      .values($metadata.Person.props)
      .filter(p => p.role != "primaryKey" && p.role != "foreignKey"); 
    };

    get headers() { return this.showProps.map(o => ({text: o.displayName, value: o.name})) };

    @Watch('pagination')
    getData() {
      this.isLoading = true;
      fetch(`http://localhost:11202/api/Person/List?page=${this.pagination.page}&pageSize=${this.pagination.rowsPerPage}&search=${this.search}&orderBy=${this.pagination.descending ? '' : this.pagination.sortBy}&orderByDescending=${this.pagination.descending ? this.pagination.sortBy : ''}`)
        .then(response => response.json())
        .then(resp => {
          const list = resp.list;
          list.forEach((i: any) => hydrateMetadata(i, $metadata.Person));
          this.items = list;
          this.pagination.page = resp.page;
          this.pagination.rowsPerPage = resp.pageSize;
          this.count = resp.totalCount;
          this.isLoading = false;
          this.person = this.items[0];
        });
    }

    mounted() {
      //this.getData();
    }

    

    items = [];
  }
</script>

