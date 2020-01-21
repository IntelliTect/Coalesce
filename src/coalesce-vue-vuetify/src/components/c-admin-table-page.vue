<template>
  <div class="c-admin-table-page">
    <c-admin-table 
      class="c-admin-table-page--table"
      :list="listVM">
    </c-admin-table>
    <h1 class="c-admin-table-page--methods-header">Methods</h1>
    <c-methods 
      class="c-admin-table-page--methods" 
      :model="listVM"
    ></c-methods>
  </div>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags, mapParamsToDto, mapQueryToParams, ListParameters } from 'coalesce-vue';

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

    // Pull initial parameters from the querystring before we setup watchers.
    this.listVM.$params = mapQueryToParams(this.$route.query, ListParameters, this.listVM.$metadata)

    this.$watch(
      () => mapParamsToDto(this.listVM.$params),
      (mappedParams) => {
        this.$router.replace({query: {
          ...this.$route.query,
          // mappedParams contains numbers and strings. 
          // Vue-router only claims to accept strings. Vue-router can just deal with it.
          ...(mappedParams as any)
        }})
      },
      { deep: true }
    )

    // When the query changes, grab the new value.
    this.$watch(
      () => this.$route.query, 
      (v: any) => {
        this.listVM.$params = mapQueryToParams(v, ListParameters, this.listVM.$metadata)
      }
    );
  }

}
</script>


<style lang="scss">
  .c-admin-table-page {
    padding: 16px;
    // margin-top: 16px;
    > *:not(h1) {
      margin-bottom: 30px;
    }

  }

</style>
