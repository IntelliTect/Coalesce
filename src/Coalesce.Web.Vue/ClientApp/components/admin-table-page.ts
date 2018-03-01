
import { Vue, Component, Watch } from 'vue-property-decorator';
import { CDisplay, CInput } from '../coalesce/components';
import { ApiClient, IHaveMetadata, convertToModel } from '../coalesce'
import * as metadata from '../metadata.g';
import { Person } from '../metadata.g';
import * as models from '../models.g';

import { PersonViewModel } from '../viewmodels-sandbox'


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

    new ApiClient<models.Person>(metadata.Person)
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
    this.person.$startAutoSave(this)
    
    
  }

  

  items: IHaveMetadata[] = [];
}

