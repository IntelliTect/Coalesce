<template>
  <v-container fluid class="c-admin-table-page">
    <c-admin-table 
      class="c-admin-table-page--table"
      :list="listVM"
      query-bind
    >
    </c-admin-table>
    
    <c-admin-methods 
      class="c-admin-table-page--methods" 
      :model="listVM"
      auto-reload-model
    ></c-admin-methods>
  </v-container>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from '../c-metadata-component'
import { ListViewModel, ModelType } from 'coalesce-vue';

import CAdminMethods from './c-admin-methods.vue';
import CAdminTable from './c-admin-table.vue';

@Component({
  name: 'c-admin-table-page',
  components: {
    CAdminMethods, CAdminTable
  }
})
export default class extends MetadataComponent {
  /** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
  get pageTitle() {
    return this.metadata.displayName + " List"
  }

  @Prop({required: false, type: String, default: null})
  public type!: string | null;

  @Prop({required: false, type: Object})
  public list!: ListViewModel<any,any> | null;

  listVM: ListViewModel<any,any> = null as any;

  get metadata(): ModelType {
    if (this.listVM) {
      return this.listVM.$metadata
    }
    // TODO: this message is bad.
    throw `No metadata available - no list provided, and couldn't create one.`
  }

  created() {
    // Create our ListViewModel instance if one was not provided.
    if (this.list) {
      this.listVM = this.list;
    } else {
      if (!this.type) {
        throw Error("c-admin-table-page: If prop `list` is not provided, `type` is required.")
      } else if (!ListViewModel.typeLookup![this.type]) {
        // TODO: Bake a `getOrThrow` into `typeLookup`.
        throw Error(`No model named ${this.type} is registered to ListViewModel.typeLookup`)
      }
      this.listVM = new ListViewModel.typeLookup![this.type]
    }
  }

}
</script>


<style lang="scss">
  .c-admin-table-page {
    
    .c-admin-table-page--methods {
      margin-top: 30px;
    }

  }

</style>
