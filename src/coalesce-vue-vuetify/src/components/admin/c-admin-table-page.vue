<template>
  <v-container fluid class="c-admin-table-page">
    <c-admin-table 
      class="c-admin-table-page--table"
      :list="listVM">
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
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags, mapParamsToDto, mapQueryToParams, ListParameters } from 'coalesce-vue';

import CAdminMethods from './c-admin-methods.vue';
import CAdminTable from './c-admin-table.vue';

@Component({
  name: 'c-admin-table-page',
  components: {
    CAdminMethods, CAdminTable
  }
})
export default class extends MetadataComponent {

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
        throw Error("c-admin-table-page: If prop `list` is not provided, `model-name` is required.")
      } else if (!ListViewModel.typeLookup![this.type]) {
        // TODO: Bake a `getOrThrow` into `typeLookup`.
        throw Error(`No model named ${this.type} is registered to ListViewModel.typeLookup`)
      }
      this.listVM = new ListViewModel.typeLookup![this.type]
    }

    // Pull initial parameters from the querystring before we setup watchers.
    this.listVM.$params = mapQueryToParams(this.$route.query, ListParameters, this.listVM.$metadata)

    this.$watch(
      () => mapParamsToDto(this.listVM.$params),
      (mappedParams) => {
        this.$router.replace({query: {
          // ...this.$route.query,
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
    
    .c-admin-table-page--methods {
      margin-top: 30px;
    }

  }

</style>
