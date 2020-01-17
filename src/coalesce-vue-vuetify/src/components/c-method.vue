
<template>
  <div class="c-method">
    <v-container fluid  grid-list-lg class="c-method--params">
      <v-layout wrap >
        <v-flex 
          v-for="param in filteredParams"
          :key="param.name"
          xs6 md4 lg3>
          <c-input v-model="caller.args[param.name]" :for="param" />
        </v-flex>
      </v-layout>
    </v-container>
    <div class="c-method--execute">
      <v-btn  
        class="mr-3"
        color="primary" 
        @click="caller.invokeWithArgs()"
        :loading="caller.isLoading">
        Execute
      </v-btn>
      
      <span class="c-methods--state">
        <v-chip v-if="caller.isLoading" color="cyan" dark>
          Loading
        </v-chip>
        <v-chip v-else-if="caller.wasSuccessful === null" color="gray" dark>
          Not Called
        </v-chip>
        <v-chip v-else-if="caller.wasSuccessful === true" color="success" dark>
          Success
        </v-chip>
        <v-chip v-else-if="caller.wasSuccessful === false" color="error" dark>
          Error
        </v-chip>
      </span>
    </div>
    
    <div class="c-method--result">
      <pre
        class="c-method--result-error error--text"
        v-if="caller.message"
      >{{caller.message}}</pre>
      
      <pre
        class="c-method--result-value"
        v-if="caller.result"
      ><c-display 
        v-model="caller.result" 
        :for="meta.return" /></pre>
    </div>
  </div>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent, { getValueMeta } from './c-metadata-component'
import { Model, ClassType, ViewModel, Property, Method, ModelType, ListViewModel } from 'coalesce-vue';
import CInput from './c-input'

@Component({
  name: 'c-method',
  components: {
    CInput
  }
})
export default class CMethod extends MetadataComponent {

  get methodMeta() {
    const meta = getValueMeta(this.for, this.modelMeta);
    if (meta && "params" in meta) {
      return meta;
    }
    throw Error("`c-method` requires metadata for a method.")
  }

  get filteredParams() {
    return Object
      .values(this.methodMeta.params)
      // For model instance methods, don't include an input for the primary key, since it is implicit.
      .filter(p => !this.methodMeta.isStatic ? p.name != 'id' : true)
  }

  get caller() {
    const caller = (this.viewModel as any)[this.methodMeta.name];
    if (!caller) throw Error(`Method '${this.methodMeta.name}' doesn't exist on provided model.`);
    return caller;
  }

  get viewModel(): ViewModel | ListViewModel {
    if (this.model instanceof ViewModel) return this.model;
    if (this.model instanceof ListViewModel) return this.model;
    throw Error("c-method: prop `model` is required, and must be a ViewModel or ListViewModel.");
  }
}

</script>

<style lang="scss">
.c-method--result {
  margin-top: 10px;
}
.c-method--result-error {
  white-space: pre-wrap;
  word-break: break-word;
}
</style>