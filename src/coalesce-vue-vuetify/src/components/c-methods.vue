
<template>
  <v-expansion-panels v-if="methods.length">
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
          :for="method">
        </c-method>
      </v-expansion-panel-content>
    </v-expansion-panel>
  </v-expansion-panels>
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

  get viewModel(): ViewModel | ListViewModel {
    if (this.model instanceof ViewModel) return this.model;
    if (this.model instanceof ListViewModel) return this.model;
    throw Error("c-method: prop `model` is required, and must be a ViewModel or ListViewModel.");
  }

  get metadata() {
    return this.viewModel.$metadata as ModelType;
  }

  get methods() {
    return Object
      .values(this.metadata.methods)
      .filter(m => this.viewModel instanceof ViewModel ? !m.isStatic : m.isStatic)
  }
}

</script>

<style lang="scss">
</style>