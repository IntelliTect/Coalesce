
<template>
  <div class="c-method">
    <v-row class="my-0 c-method--section c-method--params" v-if="filteredParams.length">
      <v-col>
        Parameters
      </v-col>
      <v-col class="py-0">
        <v-row class="my-0">
          <v-col
            v-for="param in filteredParams"
            :key="param.name"
            cols=12 sm=6 md=4 lg=3
          >
            <c-input 
              v-model="caller.args[param.name]" 
              :for="param" 
              hide-details
            />
          </v-col>
        </v-row>
      </v-col>
    </v-row>

    <v-row class="my-0 c-method--section c-method--results">
      <v-col>
        <v-btn  
          color="primary" 
          @click="invoke"
          :loading="caller.isLoading"
          sm
        >
          Execute
        </v-btn>
      </v-col>
      <v-col>
        <c-loader-status 
          :progress-placeholder="false"
          :loaders="{'no-initial-content no-error-content no-loading-content': [caller]}"
          style="min-height: 55px"
        >
          <h3>Result:</h3>
          <c-display 
            v-if="caller.result != null"
            element="pre"
            class="c-method--result-value"
            v-model="caller.result" 
            :for="methodMeta.return"
            :options="resultDisplayOptions"
          />
          <span
            v-else-if="methodMeta.return.type == 'void'.wasSuccessful != null"
            class="c-method--result-void"
          >
            <v-alert type="success" dense>Success</v-alert>
          </span>
          <span
            v-else-if="caller.wasSuccessful != null && caller.result == null"
            class="c-method--result-null"
          >
            <pre>{{"" + caller.result}}</pre>
          </span>
        </c-loader-status>
      </v-col>
    </v-row>

  </div>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent, { getValueMeta } from '../c-metadata-component'
import { Model, ClassType, ViewModel, Property, Method, ModelType, ListViewModel, DisplayOptions } from 'coalesce-vue';
import CInput from '../input/c-input'

const resultDisplayOptions = <DisplayOptions>{
  collection: {
    enumeratedItemsMax: Infinity,
    enumeratedItemsSeparator: "\n"
  }
}
@Component({
  name: 'c-method',
  components: {
    CInput
  }
})
export default class CMethod extends MetadataComponent {
  resultDisplayOptions = resultDisplayOptions;
  
  @Prop({required: false, type: Boolean, default: false})
  public autoReloadModel!: boolean;

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

  async invoke() {
    await this.caller.invokeWithArgs()
    if (this.autoReloadModel) {
      await this.viewModel.$load();
    }
  }
}

</script>

<style lang="scss">
.c-method--section {
  > .col:first-child {
    font-size: 18px;
    flex-grow: 0;
    min-width: 150px;
    text-align: right;
  }
}
.c-method--result {
  margin-top: 10px;

}
.c-method--result-error {
  white-space: pre-wrap;
  word-break: break-word;
}
.c-method--result-null {
  color: #999;
}
</style>