<template>
  <div class="c-admin-table-page">
    <c-admin-table 
      class="c-admin-table-page--table"
      :model="listVM">
    </c-admin-table>
    <c-methods 
      class="c-admin-table-page--methods" 
      :model="listVM"
    ></c-methods>
  </div>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags } from 'coalesce-vue';

import CMethodsDialog from './c-methods-dialog.vue';
import CAdminTable from './c-admin-table.vue';

const simpleParams = {
  includes: String,
  search: String,
  page: Number,
  pageSize: Number,
  orderBy: String,
  orderByDescending: String,
} as const
    
@Component({
  name: 'c-admin-table-page',
  components: {
    CMethodsDialog, CAdminTable
  }
})
export default class extends MetadataComponent {

  // TODO: Replace with the `for` prop.
  @Prop({required: false, type: String, default: null})
  public modelName!: string | null;

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
      if (!this.modelName) {
        throw Error("c-admin-table-page: If prop `list` is not provided, `model-name` is required.")
      } else if (!ListViewModel.typeLookup![this.modelName]) {
        // TODO: Bake a `getOrThrow` into `typeLookup`.
        throw Error(`No model named ${this.modelName} is registered to ListViewModel.typeLookup`)
      }
      this.listVM = new ListViewModel.typeLookup![this.modelName]
    }

    // TODO: Bind parameters to the querystring with coalesce-vue's `bindToQueryString`.
  }

}
</script>


<style lang="scss">
  .c-admin-table-page {
    margin-top: 16px;
    > * {
      margin-bottom: 30px;
    }
  }

</style>
