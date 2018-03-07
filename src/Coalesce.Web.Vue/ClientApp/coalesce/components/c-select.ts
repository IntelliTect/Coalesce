
import { Vue, Component, Watch } from 'vue-property-decorator';
//@ts-ignore
// import { VSelect } from 'vuetify/es5/components/VSelect';
import CDisplay from './c-display';
import MetadataComponent from './c-metadata-component'
import { ModelProperty } from 'coalesce-vue/lib/metadata';
import { ApiClient } from 'coalesce-vue/lib/api-client';
import debounce from 'lodash-es/debounce';

@Component({
  name: 'c-select',
  components: {
    CDisplay
  }
})
export default class extends MetadataComponent {
  public loading: boolean = false;
  public search: string | null = null;
  public items: any[] = [];

  get listItems() {
    const items = this.items.slice();
    const selected = (this.item as any)[this.propMeta.name];
    
    // Appending this to the bottom is intentional - chances are, if a person opens a dropdown that already has a value selected, they don't want to re-select the value that's already selected.
    if (selected) items.push(selected);
    
    return items;
  }
  
  private debouncedQuery!: () => void

  @Watch('search')
  queryDropdownItems() {
    this.debouncedQuery();
  }

  mounted() {
    // This needs to be late initialized so we have the correct "this" reference.
    
    this.debouncedQuery = debounce(() => {
      this.loading = true;
      
      const propMeta = this.propMeta;
      if (propMeta.type != "model") 
        throw `Property ${propMeta.name} must be a model property to use c-select.`

      new ApiClient(propMeta.typeDef)
        .list({pageSize: 500, search: this.search || undefined})
        .then(resp => {
          this.items = resp.data.list || [];
          this.loading = false;
        });
    }, 500);

    this.loading = false;
  }
}