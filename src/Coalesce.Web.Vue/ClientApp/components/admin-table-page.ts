
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

      var caller = this.person!.$apiClient.$makeCaller("item", c => () => c.changeSpacesToDashesInName(1));
      caller.result
    
      //this.person.$startAutoSave(this)
    
  }

  

  items: models.Person[] = [];
}

