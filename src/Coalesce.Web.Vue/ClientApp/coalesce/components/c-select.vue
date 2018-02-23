<template>
  <v-select
    :label="propMeta.displayName"
    :loading="loading"
    :items="listItems"
    :search-input.sync="search"
    :item-text="propMeta.typeDef.displayProp.name"
    :item-value="propMeta.typeDef.keyProp.name"
    :return-object="true"
    v-model="item[propMeta.name]"
    :filter="() => true"
    @onKeyDown.delete.stop="item[propMeta.name] = null"
    autocomplete
    clearable
    open-on-clear
  dense
  >
  <!-- multiple chips -->

  
    <template slot="item" slot-scope="data">
      <!-- <c-display :item="data.item" /> -->
      <v-list-tile-content>
        <v-list-tile-title >
          <c-display :item="data.item" :prop="propMeta.typeDef.displayProp" />
        </v-list-tile-title>
      </v-list-tile-content>
    </template>
    <template slot="selection" slot-scope="data">
      <c-display :item="item" :prop="propMeta" class="input-group__selections__comma"/>
    </template>

  </v-select>
</template>

<script lang="ts">
  
  import { Vue, Component, Watch } from 'vue-property-decorator';
  //@ts-ignore
  // import { VSelect } from 'vuetify/es5/components/VSelect';
  import CDisplay from './c-display';
  import MetadataComponent from './c-metadata-component'
  import { ModelProperty } from '../core';
import { ApiClient } from '../index';

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
    
    @Watch('search')
    queryDropdownItems() {
      
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
    }

    mounted() {
      this.queryDropdownItems();
      this.loading = false;
    }
  }
</script>

