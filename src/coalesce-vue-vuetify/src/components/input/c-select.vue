<template>
  <v-autocomplete
    class="c-select"
    :value="internalModelValue"
    @input="onInput"
    v-on="{...$listeners}"
    :loading="loading"
    :items="listItems"
    :search-input.sync="search"
    :item-value="modelObjectMeta.keyProp.name"
    :return-object="true"
    :clearable="effectiveClearable"
    :rules="effectiveRules"
    :error-messages="error"
    no-filter
    @keydown.native.delete.stop="!search ? onInput(null) : void 0"
    :open-on-clear="openOnClear"
    v-bind="inputBindAttrs"
  >
    <template slot="no-data">
      <!-- TODO: i18n -->
      <div class="grey--text px-4 my-3" v-if="!createItemLabel">No results found.</div>
      <!-- Must include an empty element so vuetify doesn't fall back to the slot's default -->
      <span v-else></span>
    </template>
    <template slot="prepend-item" v-if="createItemLabel">
      <v-list-item @click="createItem">
        <v-list-item-avatar class="mr-1 my-0">
          <v-icon color="success">fa fa-plus</v-icon>
        </v-list-item-avatar>
        <v-list-item-content>
          <v-list-item-title >
            <span class="grey--text">
              Create: 
            </span>
            {{createItemLabel}}
          </v-list-item-title>
        </v-list-item-content>
      </v-list-item>
    </template>
    <template slot="append-item">
      <!-- TODO: i18n -->
      <div class="grey--text px-4 my-3" v-if="listCaller.pageCount > 1">
        Max {{listCaller.pageSize}} items retrieved. Refine your search to view more.
      </div>
    </template>
    <template #item="{ item, attrs, on }">
      <!-- <c-display :model="item" /> -->
      <v-list-item v-bind="attrs" v-on="on">
        <v-list-item-content>
          <v-list-item-title >
            <slot name="item" :item="item">
              <c-display :model="item" />
            </slot>
          </v-list-item-title>
        </v-list-item-content>
      </v-list-item>
    </template>
    <template #selection="{ item }">
      <slot name="item" :item="item" >
        <c-display :model="item" />
      </slot>
    </template>

  </v-autocomplete>
</template>

<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator';

import CDisplay from '../display/c-display';
import MetadataComponent from '../c-metadata-component'
import { 
  ApiClient, ModelApiClient, ListApiState, ListResultPromise, ItemApiState, ItemResultPromise, 
  Model, ModelType, ForeignKeyProperty, ModelReferenceNavigationProperty, ListParameters, 
  mapParamsToDto, getMessageForError, 
  ViewModel, 
  mapValueToModel,
  ItemApiStateWithArgs
} from 'coalesce-vue'

@Component({
  name: 'c-select',
  components: {
    CDisplay
  }
})
export default class CSelect extends MetadataComponent {
  public search: string | null = null;

  @Prop({required: false, default: undefined, type: Boolean})
  public clearable!: boolean

  @Prop({required: false})
  public value?: any

  @Prop({required: false})
  public keyValue?: any

  @Prop({required: false})
  public objectValue?: any

  @Prop({ required: false, type: Boolean, default: false })
  public preselectFirst?: boolean;

  @Prop({ required: false, type: Boolean, default: false })
  public preselectSingle?: boolean;

  @Prop({ required: false, type: Boolean, default: true })
  public openOnClear?: boolean;

  @Prop({ required: false, type: Boolean, default: false })
  public reloadOnOpen?: boolean;

  @Prop({required: false})
  public params?: ListParameters

  @Prop({required: false})
  public create?: {
    getLabel: (search: string, items: Model<ModelType>[]) => string | false,
    getItem: (search: string, label: string) => Promise<Model<ModelType>> 
  }

  public error: string[] = []

  get loading() {
    return this.listCaller.isLoading || this.getCaller.isLoading;
  }

  /** The property on `this.model` which holds the foreign key being selected for, or `null` if there is no such property. */
  get modelKeyProp(): ForeignKeyProperty | null {
    const meta = this.valueMeta!;
    if (meta.role == "foreignKey" && "principalType" in meta) {
      return meta;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta.foreignKey;
    }
    return null;
  }

  /** The property on `this.model` which holds the reference navigation being selected for, or `null` if there is no such property. */
  get modelNavProp(): ModelReferenceNavigationProperty | null {
    const meta = this.valueMeta!;
    if (meta.role == "foreignKey" && "navigationProp" in meta) {
      return meta.navigationProp || null;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta;
    }
    return null;
  }

  /** 
   * Whether the metadata provided to `for` and retrieved via `this.valueMeta` is for a key or an object.
   * Dictates whether v-model will attempt to bind a key or an object.
   */
  get primaryBindKind() {
    if (!this.valueMeta) {
      throw "c-select requires metadata"
    }

    if (this.valueMeta.role == "foreignKey") { 
      return "key";
    }
    if (this.valueMeta.type == "model") {
      return "model"
    }

    throw "The 'role' of the metadata provided for c-select must be 'foreignKey', or the type must be 'model' "
  }

  /** The metadata of the type being selected by the dropdown. */
  get modelObjectMeta() {
    var meta = this.valueMeta!;
    if (meta.role == "foreignKey" && "principalType" in meta) {
      return meta.principalType;
    } else if (meta.type == "model") {
      return meta.typeDef;
    } else {
      throw `Value ${meta.name} must be a foreignKey or model type to use c-select.`
    }
  }

  /** The model representing the current selected item 
   * in the case that only the PK was provided to the component.
   * This is maintained in a variable to prevent its reference from 
   * changing unexpectedly, which causes Vuetify to annoying things.
   */
  keyFetchedModel: any = null;

  /** The effective object (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
  get internalModelValue() {
    if (this.objectValue) {
      return this.objectValue;
    }
    if (this.model && this.modelNavProp && (this.model as any)[this.modelNavProp.name]) {
      return (this.model as any)[this.modelNavProp.name];
    }
    if (this.value && this.primaryBindKind == "model") {
      return this.value;
    }
    
    if (this.internalKeyValue) {

      // See if we already have a model that we're using to represent a key-only binding.
      // Storing this object prevents it from flipping between different instances
      // obtained from either this.getCaller or this.listCaller,
      // which causes vuetify to reset its search when the object passed to v-select's `value` prop changes.
      if (
        this.keyFetchedModel && 
        this.internalKeyValue === this.keyFetchedModel[this.modelObjectMeta.keyProp.name]) {
        
        return this.keyFetchedModel;
      }

      // All we have is the PK. First, check if it is already in our item array.
      // If so, capture it. If not, request the object from the server.if (this.keyValue) {
      const item = this.items.filter(i => this.internalKeyValue === (i as any)[this.modelObjectMeta.keyProp.name])[0];
      if (item) {
        this.keyFetchedModel = item;
        return item;
      }
      
      // See if we obtained the item via getCaller.
      const singleItem = this.getCaller.result;
      if (singleItem && this.internalKeyValue === (singleItem as any)[this.modelObjectMeta.keyProp.name]) {
        this.keyFetchedModel = singleItem;
        return singleItem;
      }

      if (!this.listCaller.isLoading && this.getCaller.args.id != this.internalKeyValue) {
        // Only request the single item if the list isn't currently loading,
        // and if the last requested key is not the key we're looking for.
        // (this prevents an infinite loop of invokes if the call to the server fails.)
        // The single item may end up coming back from a pending list call.
        this.getCaller.args.id = this.internalKeyValue
        this.getCaller.invokeWithArgs()
      }
    }
    return null;
  }

  /** The effective key (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
  get internalKeyValue() {
    let value: any;
    if (this.keyValue) {
      value = this.keyValue;
    } else if (this.model && this.modelKeyProp) {
      value = (this.model as any)[this.modelKeyProp.name];
    } else if (this.value && this.primaryBindKind == "key") {
      value = this.value;
    } else {
      value = null;
    }

    if (value != null) {
      // Parse the value in case we were given a string instead of a number, or something like that, via the `keyValue` prop.
      // This prevents `internalModelValue` from getting confused and infinitely calling the `getCaller`.
      return mapValueToModel(value, this.modelObjectMeta.keyProp);
    }

    return null;
  }

  /** The effective set of validation rules to pass to the v-select. */
  get effectiveRules() {
    // If we were explicitly given rules, use those.
    if (this.inputBindAttrs.rules) return this.inputBindAttrs.rules;

    // We're binding to a ViewModel instance.
    // Grab the rules from the instance, because it may contain custom rules
    // and/or other rule changes that have been customized in userland beyond what the metadata provides.
    if (this.model instanceof ViewModel && this.modelKeyProp) {
      return this.model.$getRules(this.modelKeyProp)
    }
    
    // Look for validation rules from the metadata on the key prop.
    // The foreign key is always the one that provides validation rules
    // for navigation properties - never the navigation property itself.
    if (this.modelKeyProp?.rules) {
      return Object.values(this.modelKeyProp.rules)
    }

    return [];
  }

  /** The effective clearability state of the dropdown. */
  get effectiveClearable(): boolean {
    // If explicitly given a value, use that value.
    if (typeof this.clearable == "boolean") return this.clearable;
    
    // Check to see if the foreign key is nullable (i.e. doesn't have a 'required' rule).
    return !!(this.modelKeyProp && !this.modelKeyProp.rules?.required)
  }

  onInput(value: Model<ModelType> | null | undefined, dontFocus = false) {
    // Clear any manual errors
    this.error = [];

    value = value ? value : null;
    const key = value ? (value as any)[this.modelObjectMeta.keyProp.name] : null;
    this.keyFetchedModel = value;

    if (this.model) {
      if (this.modelKeyProp){
        (this.model as any)[this.modelKeyProp.name] = key;
      }
      if (this.modelNavProp){
        (this.model as any)[this.modelNavProp.name] = value;
      }
    }

    this.$emit('input', this.primaryBindKind == "key" ? key : value);
    this.$emit('update:object-value', value);
    this.$emit('update:key-value', key);

    // When the input value is cleared, re-focus the dropdown
    // to allow the user to enter new search input.
    // Without this, pressing backspace to delete the current value
    // will cause the search field to lose focus and further keyboard input will do nothing.
    if (!dontFocus) {
      if (!value) {
        this.focus()
      } else {
        this.blur();
      }
    }
  }

  get vAutocomplete() {
    return this.$children[0] as any | undefined
  }

  focus() {
    this.$nextTick(() => this.vAutocomplete?.onClick?.(new Event('fake')))
  }

  blur() {
    this.$nextTick(() => {
      this.vAutocomplete?.onClick?.(new Event('fake')); 
      this.vAutocomplete?.onEscDown?.(new Event('fake'))
    })
  }

  get items() {
    return this.listCaller.result || [];
  }

  get listItems() {
    const items = this.items.slice();
    const selected = this.internalModelValue;
    
    // Appending this to the bottom is intentional - chances are, if a person opens a dropdown that already has a value selected, they don't want to re-select the value that's already selected.
    if (selected) items.push(selected);
    
    return items;
  }
  
  @Watch('search')
  searchChanged(newVal: any, oldVal: any) {
    if (newVal != oldVal) {
      // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
      // since null == undefined, the transition from undefined to null will fail.
      this.listCaller();
    }
  }

  listCaller: ListApiState<[], Model<ModelType>> & (() => ListResultPromise<Model<ModelType>>) = null as any

  /**
   * A caller that will be used to resolve the full object when the only thing
   * that has been provided to c-select is a primary key value.
   */
  getCaller: ItemApiStateWithArgs<[], {id: any}, Model<ModelType>> = null as any

  get createItemLabel() {
    if (!this.create || !this.search) return null;

    const result = this.create.getLabel(this.search, this.items);
    if (result) {
      return result;
    }
    return null;
  }

  async createItem() {
    if (!this.createItemLabel) return;
    try {
      const item = await this.create!.getItem(this.search!, this.createItemLabel);
      if (!item) return;
      this.onInput(item);
    } catch (e: unknown) {
      this.error = [getMessageForError(e)]
    }
  }

  created() {
    const propMeta = this.valueMeta;
    if (!propMeta) {
      throw "c-select requires value metadata. Specify it with the 'for' prop'";
    }
    
    // This needs to be late initialized so we have the correct "this" reference.
    
    
    this.getCaller = new ModelApiClient(this.modelObjectMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller(
        "item", 
        function (this: CSelect, c) {
          throw 'expected calls to be made with invokeWithArgs'
        }, 
        () => ({ id: null as any }),
        (c, args) => c.get(args.id)
      )
      .setConcurrency("debounce");

    this.listCaller = new ModelApiClient(this.modelObjectMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller("list", function (this: CSelect, c) {
        return c.list({pageSize: 100, ...this.params, search: this.search || undefined})
      })
      .onRejected(function(this: CSelect, state) {
        this.error = [state.message || "Unknown Error"]
      })
      .setConcurrency("debounce");

    this.$watch(
      () => JSON.stringify(mapParamsToDto(this.params)), 
      () => this.listCaller()
    );

    // Load the initial contents of the list.
    this.listCaller().then(() => {
      if (this.internalModelValue || this.internalKeyValue) {
        // Don't preselect if there's already a value selected.
        return;
      }
      
      const first = this.items[0];
      if (first && (this.preselectFirst || (this.preselectSingle && this.items.length === 1))) {
        this.onInput(first, true);
      }
    });
  }

  mounted() {
    // This is dependent upon $children, and so cannot be done until after mount
    // ($children and $refs are not reactive).
    this.$watch(
      () => this.vAutocomplete?.isMenuActive, 
      (v) => { if (v && this.reloadOnOpen) this.listCaller() }
    );
  }
}
</script>


<style lang="scss">
  .c-select.v-input--is-focused input[type=text]:not(:first-child) {
    padding-left: 10px;
    color: rgba(0,0,0,.54) !important;
  }
</style>

