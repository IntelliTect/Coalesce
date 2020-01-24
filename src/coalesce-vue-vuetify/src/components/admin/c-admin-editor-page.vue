<template>
  <v-container class="c-admin-editor-page">
    <c-admin-editor 
      class="c-admin-editor-page--editor" 
      :model="viewModel"
    />

    <c-admin-methods 
      class="c-admin-editor-page--methods" 
      :model="viewModel" 
      auto-reload-model
    />
  </v-container>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from '../c-metadata-component'
import { ViewModel, ModelType, BehaviorFlags } from 'coalesce-vue';

import CAdminMethods from './c-admin-methods.vue';
import CAdminEditor from './c-admin-editor.vue';

@Component({
  name: 'c-admin-table-page',
  components: {
    CAdminEditor, CAdminMethods
  }
})
export default class extends MetadataComponent {

  @Prop({required: true, type: String})
  public type!: string;

  @Prop({required: false, type: [String, Number]})
  public id!: string | number;

  viewModel: ViewModel<any,any> = null as any;

  get metadata(): ModelType {
    if (this.viewModel) {
      return this.viewModel.$metadata
    }
    throw `No metadata available.`
  }
  
  get isCreate() {
    return !this.viewModel!.$primaryKey;
  }

  get hasInstanceMethods() {
    return !this.isCreate && this.metadata && Object.values(this.metadata.methods).some(m => !m.isStatic)
  }

  async created() {
    if (!ViewModel.typeLookup![this.type]) {
      // TODO: Bake a `getOrThrow` into `typeLookup`.
      throw Error(`No model named ${this.type} is registered to ViewModel.typeLookup`)
    }
    
    this.viewModel = new ViewModel.typeLookup![this.type];

    if (this.id) {
      await this.viewModel.$load(this.id);
    } else {
      // TODO: Pull intiial data from the querystring if there is any (like what knockout does).

      this.$watch(
        () => this.viewModel.$primaryKey,
        pk => {
          const { href } = this.$router.resolve({
            ...this.$route,
            params: {
              ...this.$route.params,
              id: this.viewModel.$primaryKey
            }
          });
          // Manually replace state with the HTML5 history API
          // so that vue-router doesn't notice the route change
          // and therefore won't trigger any route transitions
          // or router-view component reconstitutions.
          window.history.replaceState(null, window.document.title, href)
        }
      )
    }

    this.viewModel.$startAutoSave(this, { wait: 500 })
    
  }

}
</script>


<style lang="scss">
  .c-admin-editor-page {
    max-width: 1300px;

    .c-admin-editor-page--methods {
      margin-top: 30px;
    }
  }

</style>
