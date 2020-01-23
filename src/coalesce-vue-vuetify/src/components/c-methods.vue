
<template>

  <v-expand-transition>
    <v-expansion-panels v-if="methods.length" class="c-methods">
      <v-toolbar
        class="c-admin-editor-page--toolbar"
        dense color="primary darken-1" dark
      >

        <v-toolbar-title >
          Actions
        </v-toolbar-title>

        <v-divider class="mx-4 my-0" vertical></v-divider>

        <v-toolbar-title v-if="!isStatic">
          <c-display :model="model"></c-display>
        </v-toolbar-title>
      </v-toolbar>
      <v-expansion-panel
        v-for="method in methods"
        :key="method.name"
      >
        <v-expansion-panel-header>
          <div>{{method.displayName}}</div>
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <c-method
            :model="model"
            :for="method"
            :autoReloadModel="autoReloadModel"
          >
          </c-method>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-expand-transition>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ViewModel, Property, Method, ModelType, ListViewModel } from 'coalesce-vue';
import CMethod from './c-method.vue'

@Component({
  name: 'c-methods',
  components: {
    CMethod
  }
})
export default class CMethods extends Vue {
  @Prop({required: true, type: Object})
  public model!: ViewModel<Model<ModelType>> | ListViewModel;

  @Prop({required: false, type: Boolean, default: false})
  public autoReloadModel!: boolean;

  get viewModel(): ViewModel | ListViewModel {
    if (this.model instanceof ViewModel) return this.model;
    if (this.model instanceof ListViewModel) return this.model;
    throw Error("c-method: prop `model` is required, and must be a ViewModel or ListViewModel.");
  }

  get metadata() {
    return this.viewModel.$metadata as ModelType;
  }

  get isStatic() {
    return this.viewModel instanceof ListViewModel
  }

  get methods() {
    if (this.viewModel instanceof ViewModel && !this.viewModel.$primaryKey) {
      return []
    }

    return Object
      .values(this.metadata.methods)
      .filter(m => !!m.isStatic == this.isStatic)
  }
}

</script>

<style lang="scss">
</style>