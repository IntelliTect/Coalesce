
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
        <template v-if="methodMeta.return.type == 'file' && !filteredParams.length">
          <v-btn  
            color="primary" 
            @click="invoke"
            :loading="caller.isLoading"
          >
            <span v-if="filteredParams.length">Execute</span>
            <span v-else>
              <v-icon left>fa fa-search</v-icon>
              Preview
            </span>
          </v-btn>
          <v-btn  
            class="mt-1"
            color="primary" 
            @click="invokeAndDownload"
            :loading="caller.isLoading"
          >
            <v-icon left>fa fa-download</v-icon>
            Download
          </v-btn>
        </template>
        <v-btn  
          v-else
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
          <h3>
            Result:
            <v-btn 
              v-if="caller.result && methodMeta.return.type == 'file'"
              color="primary" 
              @click="downloadFileResult"
              :loading="caller.isLoading"
              x-small outlined
            >
              <v-icon left small>fa fa-download</v-icon>
              Save to Disk
            </v-btn>
          </h3>

          
          <span
            v-if="caller.wasSuccessful && (methodMeta.return.type == 'void' || caller.message)"
            class="c-method--result-success"
          >
            <v-alert type="success" dense>{{caller.message || 'Success'}}</v-alert>
          </span>

          <div v-if="caller.result && methodMeta.return.type == 'file'">
            <pre>{{caller.result.name}} • {{caller.result.type}} • {{caller.result.size.toLocaleString()}} bytes</pre>

            <br>
            
            <template v-if="fileDownloadKind == 'preview' && 'getResultObjectUrl' in caller">
              <img 
                v-if="caller.result.type.indexOf('image') >= 0" 
                :src="caller.getResultObjectUrl(this)" 
                :alt="caller.result.name"
                class="elevation-1"
                style="max-width: 100%"
              >
              <video 
                v-else-if="caller.result.type.indexOf('video') >= 0" 
                :src="caller.getResultObjectUrl(this)" 
                :alt="caller.result.name"
                class="elevation-1" controls
                style="max-width: 100%"
              />
              <pre v-else>Unable to show preview.</pre>
            </template>
          </div>

          <c-display 
            v-else-if="caller.result != null"
            element="pre"
            class="c-method--result-value"
            v-model="caller.result" 
            :for="methodMeta.return"
            :options="resultDisplayOptions"
          />

          <span
            v-else-if="caller.wasSuccessful != null && caller.result == null && methodMeta.return.type !== 'void'"
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
import {  ViewModel,  ListViewModel, DisplayOptions, AnyArgCaller, ItemApiState } from 'coalesce-vue';
import CInput from '../input/c-input'
import CDisplay from "../display/c-display";

const resultDisplayOptions = <DisplayOptions>{
  collection: {
    enumeratedItemsMax: Infinity,
    enumeratedItemsSeparator: "\n"
  }
}
@Component({
  name: 'c-method',
  components: {
    CInput,
    CDisplay
  }
})
export default class CAdminMethod extends MetadataComponent {
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
      .filter(p => !p.source)
  }

  get caller(): AnyArgCaller {
    const caller = (this.viewModel as any)[this.methodMeta.name];
    if (!caller) throw Error(`Method '${this.methodMeta.name}' doesn't exist on provided model.`);
    return caller;
  }

  get viewModel(): ViewModel | ListViewModel {
    if (this.model instanceof ViewModel) return this.model;
    if (this.model instanceof ListViewModel) return this.model;
    throw Error("c-method: prop `model` is required, and must be a ViewModel or ListViewModel.");
  }

  fileDownloadKind = "preview";

  async invoke() {
    this.fileDownloadKind = "preview";
    await this.caller.invokeWithArgs()
    if (this.autoReloadModel) {
      await this.viewModel.$load();
    }
  }

  async invokeAndDownload() {
    this.fileDownloadKind = "download";
    await this.caller.invokeWithArgs()
    
    if (this.autoReloadModel) {
      // Don't await. Just do this in the background while we setup the file download.
      this.viewModel.$load();
    }

    this.downloadFileResult();
  }

  downloadFileResult() {
    const caller = this.caller as ItemApiState<any, File>;
    const file = caller.result;
    if (!(file instanceof File)) return;

    const a = document.createElement('a');
    document.body.appendChild(a);
    a.href = caller.getResultObjectUrl(this)!;
    a.download = file.name;
    a.click();
    setTimeout(() => {
      document.body.removeChild(a);
    }, 1)
  }
}

</script>

<style lang="scss">
.c-method--section {
  > .col:first-child {
    font-size: 18px;
    flex-grow: 0;
    min-width: 170px;
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