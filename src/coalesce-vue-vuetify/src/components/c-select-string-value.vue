<template>
  <v-combobox
    class="c-select-string-value"
    :value="internalValue"
    @input="onInput"

    :loading="loading"
    :items="items"
    :search-input.sync="search"
    v-bind="inputBindAttrs"
  >
  </v-combobox>
    <!-- no-filter -->

</template>

<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { ListViewModel, ItemApiState, ModelApiClient, ItemResultPromise } from 'coalesce-vue';

    
@Component({
  name: 'c-select-string-value',
  components: {
  }
})
export default class CSelectStringValue extends MetadataComponent { 
  @Prop({required: false})
  public value?: any

  @Prop({required: true, type: String})
  public method!: string;

  @Prop({required: false, type: Object})
  public params?: {}

  @Prop({required: false, default: false})
  public listWhenEmpty!: boolean;

  caller!: ItemApiState<[number|null, string|null], string[]>;

  public search: string | null = null;

  created() {
    if (!this.modelMeta || this.modelMeta.type != 'model') {
      throw Error("c-select-string-value requires a model to be provided via the `model` prop.")
    }

    const methodMeta = this.modelMeta.methods[this.method];

    if (!methodMeta
      || !methodMeta.isStatic
      || methodMeta.transportType != "item"
      || methodMeta.return.type != "collection"
      || methodMeta.return.itemType.type != "string") {
      throw Error("c-select-string-value requires a static model method that returns an array of strings.")
    }

    this.caller = new ModelApiClient(this.modelMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller("item", function (this: CSelectStringValue, c, page: number, search: string) {
        return c.$invoke(methodMeta, {page, search, ...this.params}) as ItemResultPromise<string[]>
        // (c as any)[methodMeta.name](page, search) as ListResultPromise<string>
      })
      .setConcurrency("debounce");
  }

  mounted() {
    this.caller();
  }
  
  @Watch('search')
  searchChanged(newVal: any, oldVal: any) {
    if (!newVal && !this.listWhenEmpty) {
      return;
    }
    
    if (newVal != oldVal) {
      // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
      // since null == undefined, the transition from undefined to null will fail.
      this.caller(1, newVal);
    }
  }

  get loading() {
    return this.caller.isLoading;
  }

  get items() {
    if (!this.search && !this.listWhenEmpty) return [];
    return this.caller.result || [];
  }

  get internalValue(): string | null {
    if (this.model && this.valueMeta) {
      return (this.model as any)[this.valueMeta.name];
    }
    return this.value;
  }

  onInput(value: string) {
    if (this.model && this.valueMeta) {
      return (this.model as any)[this.valueMeta.name] = value;
    }
    
    this.$emit('input', value);
  }
}

</script>

