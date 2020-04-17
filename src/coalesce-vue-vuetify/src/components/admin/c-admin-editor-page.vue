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
import { ViewModel, ModelType, BehaviorFlags, ListParameters, mapQueryToParams, mapToModel, mapValueToModel, bindKeyToRouteOnCreate, modelDisplay } from 'coalesce-vue';

import CAdminMethods from './c-admin-methods.vue';
import CAdminEditor from './c-admin-editor.vue';

@Component({
  name: 'c-admin-table-page',
  components: {
    CAdminEditor, CAdminMethods
  }
})
export default class extends Vue {
  /** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
  get pageTitle() {
    if (this.isCreate) {
      return "Create - " + this.metadata.displayName;
    }

    let display = this.viewModel ? modelDisplay(this.viewModel) : null;
    if (!display) {
      return this.metadata.displayName;
    }

    const maxLen = 30;
    if (display.length <= maxLen) {
      return display + " - " + this.metadata.displayName;
    }
    return display.slice(0, maxLen) + '... - ' + this.metadata.displayName;
  }

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

      const params = mapQueryToParams(this.$route.query, ListParameters, this.metadata);
      if (params.filter) {
        for (const propName in this.metadata.props) {
          const prop = this.metadata.props[propName]
          const filterValue = params.filter[propName];
          if (filterValue != null) {
            try {
              (this.viewModel as any)[propName] = mapValueToModel(filterValue, prop)
            } catch (e) {
              // mapValueToModel will throw for unmappable values.
              console.error(`Could not map filter parameter ${propName}. ${e}`)
            }
          }
        }
      }
  
      bindKeyToRouteOnCreate(this, this.viewModel);
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
