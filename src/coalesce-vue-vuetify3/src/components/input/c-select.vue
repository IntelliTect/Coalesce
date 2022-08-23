<template>
  <v-autocomplete
    class="c-select"
    v-bind="inputBindAttrs"
    :model-value="internalModelValue"
    @update:modelValue="onInput"
    :loading="loading"
    :items="listItems"
    v-model:search="search"
    :item-value="modelObjectMeta.keyProp.name"
    :item-title="$attrs['item-title'] || modelDisplay"
    :return-object="true"
    :clearable="effectiveClearable"
    :error-messages="error"
    no-filter
    @keydown.delete.stop="!search ? onInput(null) : void 0"
    @update:menu="onMenuUpdate"
    :open-on-clear="openOnClear"
    :rules="effectiveRules"
    ref="vAutocomplete"
  >
    <template #no-data>
      <!-- TODO: i18n -->
      <div class="grey--text px-4 my-3" v-if="!createItemLabel">
        No results found.
      </div>
      <!-- Must include an empty element so vuetify doesn't fall back to the slot's default -->
      <span v-else></span>
    </template>
    <template #prepend-item v-if="createItemLabel">
      <!-- TODO: This does not work because `prepend-item` currently doesn't exist. -->
      <v-list-item @click="createItem">
        <v-list-item-media class="mr-1 my-0">
          <v-icon color="success">fa fa-plus</v-icon>
        </v-list-item-media>
        <v-list-item-title>
          <span class="grey--text"> Create: </span>
          {{ createItemLabel }}
        </v-list-item-title>
      </v-list-item>
    </template>
    <template #append-item>
      <!-- TODO: i18n -->
      <div
        class="grey--text px-4 my-3"
        v-if="listCaller.pageCount && listCaller.pageCount > 1"
      >
        Max {{ listCaller.pageSize }} items retrieved. Refine your search to
        view more.
      </div>
    </template>
    <template #selection="{ props, item }">
      <slot name="item" :item="item.raw">
        <c-display :model="item.raw" v-bind="props" />
      </slot>
    </template>
  </v-autocomplete>
</template>

<script lang="ts">
import { defineComponent, Prop, PropType } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";
import {
  ModelApiClient,
  ListApiState,
  ListResultPromise,
  Model,
  ModelType,
  ForeignKeyProperty,
  ModelReferenceNavigationProperty,
  ListParameters,
  mapParamsToDto,
  getMessageForError,
  mapValueToModel,
  ItemApiStateWithArgs,
  ViewModel,
  modelDisplay,
} from "coalesce-vue";

export default defineComponent({
  name: "c-select",

  setup(props) {
    return {
      ...useMetadataProps(props),
    };
  },

  props: {
    ...makeMetadataProps(),
    clearable: { required: false, default: undefined, type: Boolean },
    value: <Prop<any>>{ required: false },
    keyValue: <Prop<any>>{ required: false },
    objectValue: <Prop<any>>{ required: false },
    preselectFirst: { required: false, type: Boolean, default: false },
    preselectSingle: { required: false, type: Boolean, default: false },
    openOnClear: { required: false, type: Boolean, default: true },
    reloadOnOpen: { required: false, type: Boolean, default: false },
    params: { required: false, type: Object as PropType<ListParameters> },
    create: {
      required: false,
      type: Object as PropType<{
        getLabel: (search: string, items: Model<ModelType>[]) => string | false;
        getItem: (search: string, label: string) => Promise<Model<ModelType>>;
      }>,
    },
  },

  data() {
    return {
      modelDisplay: modelDisplay,

      search: null as string | null,
      error: [] as string[],

      /** The model representing the current selected item
       * in the case that only the PK was provided to the component.
       * This is maintained in a variable to prevent its reference from
       * changing unexpectedly, which causes Vuetify to annoying things.
       */
      keyFetchedModel: null as any,

      listCaller: null! as ListApiState<[], Model<ModelType>> &
        (() => ListResultPromise<Model<ModelType>>),

      /**
       * A caller that will be used to resolve the full object when the only thing
       * that has been provided to c-select is a primary key value.
       */
      getCaller: null! as ItemApiStateWithArgs<
        [],
        { id: any },
        Model<ModelType>
      >,
    };
  },

  watch: {
    search(newVal: any, oldVal: any) {
      if (newVal != oldVal) {
        // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
        // since null == undefined, the transition from undefined to null will fail.
        this.listCaller();
      }
    },
  },

  computed: {
    loading() {
      return this.listCaller.isLoading || this.getCaller.isLoading;
    },

    /** The property on `this.model` which holds the foreign key being selected for, or `null` if there is no such property. */
    modelKeyProp(): ForeignKeyProperty | null {
      const meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "principalType" in meta) {
        return meta;
      }
      if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
        return meta.foreignKey;
      }
      return null;
    },

    /** The property on `this.model` which holds the reference navigation being selected for, or `null` if there is no such property. */
    modelNavProp(): ModelReferenceNavigationProperty | null {
      const meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "navigationProp" in meta) {
        return meta.navigationProp || null;
      }
      if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
        return meta;
      }
      return null;
    },

    /**
     * Whether the metadata provided to `for` and retrieved via `this.valueMeta` is for a key or an object.
     * Dictates whether v-model will attempt to bind a key or an object.
     */
    primaryBindKind() {
      if (!this.valueMeta) {
        throw "c-select requires metadata";
      }

      if (this.valueMeta.role == "foreignKey") {
        return "key";
      }
      if (this.valueMeta.type == "model") {
        return "model";
      }

      throw "The 'role' of the metadata provided for c-select must be 'foreignKey', or the type must be 'model' ";
    },

    /** The metadata of the type being selected by the dropdown. */
    modelObjectMeta() {
      var meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "principalType" in meta) {
        return meta.principalType;
      } else if (meta.type == "model") {
        return meta.typeDef;
      } else {
        throw `Value ${meta.name} must be a foreignKey or model type to use c-select.`;
      }
    },

    /** The effective object (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
    internalModelValue() {
      if (this.objectValue) {
        return this.objectValue;
      }
      if (
        this.model &&
        this.modelNavProp &&
        (this.model as any)[this.modelNavProp.name]
      ) {
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
          this.internalKeyValue ===
            this.keyFetchedModel[this.modelObjectMeta.keyProp.name]
        ) {
          return this.keyFetchedModel;
        }

        // All we have is the PK. First, check if it is already in our item array.
        // If so, capture it. If not, request the object from the server.if (this.keyValue) {
        const item = this.items.filter(
          (i) =>
            this.internalKeyValue ===
            (i as any)[this.modelObjectMeta.keyProp.name]
        )[0];
        if (item) {
          this.keyFetchedModel = item;
          return item;
        }

        // See if we obtained the item via getCaller.
        const singleItem = this.getCaller.result;
        if (
          singleItem &&
          this.internalKeyValue ===
            (singleItem as any)[this.modelObjectMeta.keyProp.name]
        ) {
          this.keyFetchedModel = singleItem;
          return singleItem;
        }

        if (
          !this.listCaller.isLoading &&
          this.getCaller.args.id != this.internalKeyValue
        ) {
          // Only request the single item if the list isn't currently loading,
          // and if the last requested key is not the key we're looking for.
          // (this prevents an infinite loop of invokes if the call to the server fails.)
          // The single item may end up coming back from a pending list call.
          this.getCaller.args.id = this.internalKeyValue;
          this.getCaller.invokeWithArgs();
        }
      }
      return null;
    },

    /** The effective key (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
    internalKeyValue() {
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
    },

    /** The effective set of validation rules to pass to the v-select. */
    effectiveRules() {
      // If we were explicitly given rules, use those.
      if (this.inputBindAttrs.rules) return this.inputBindAttrs.rules;

      if (this.model instanceof ViewModel && this.modelKeyProp) {
        // We're binding to a ViewModel instance.
        // Grab the rules from the instance, because it may contain custom rules
        // and/or other rule changes that have been customized in userland beyond what the metadata provides.

        // Since the v-autocomplete is always bound to a model instance,
        // vuetify will invoke the rules using the model object as the arg to the rule function.
        // However, we use the validation rules of the foreign key since the FK
        // is the actual scalar value that gets sent to the server,
        // and is the prop that we generate things like `required` onto.
        // We need to translate the rule functions to pass the selected FK instead
        // of the selected model object.
        return this.model
          .$getRules(this.modelKeyProp)
          ?.map((rule) => () => rule(this.internalKeyValue));
      }

      // Look for validation rules from the metadata on the key prop.
      // The foreign key is always the one that provides validation rules
      // for navigation properties - never the navigation property itself.
      if (this.modelKeyProp?.rules) {
        return Object.values(this.modelKeyProp.rules);
      }

      return [];
    },

    /** The effective clearability state of the dropdown. */
    effectiveClearable(): boolean {
      // If explicitly given a value, use that value.
      if (typeof this.clearable == "boolean") return this.clearable;

      // Check to see if the foreign key is nullable (i.e. doesn't have a 'required' rule).
      return !!(this.modelKeyProp && !this.modelKeyProp.rules?.required);
    },

    items() {
      return this.listCaller.result || [];
    },

    listItems() {
      const items = this.items.slice();
      const selected = this.internalModelValue;
      const selectedKey = this.internalKeyValue;

      // Appending this to the bottom is intentional - chances are, if a person opens a dropdown that already has a value selected, they don't want to re-select the value that's already selected.
      if (
        selected &&
        !items.some(
          (i) => selectedKey == (i as any)[this.modelObjectMeta.keyProp.name]
        )
      ) {
        items.push(selected);
      }

      return items;
    },

    createItemLabel() {
      if (!this.create || !this.search) return null;

      const result = this.create.getLabel(this.search, this.items);
      if (result) {
        return result;
      }
      return null;
    },
  },

  methods: {
    onInput(value: Model<ModelType> | null | undefined, dontFocus = false) {
      // Clear any manual errors
      this.error = [];

      value = value ? value : null;
      const key = value
        ? (value as any)[this.modelObjectMeta.keyProp.name]
        : null;
      this.keyFetchedModel = value;

      if (this.model) {
        if (this.modelKeyProp) {
          (this.model as any)[this.modelKeyProp.name] = key;
        }
        if (this.modelNavProp) {
          (this.model as any)[this.modelNavProp.name] = value;
        }
      }

      this.$emit("input", this.primaryBindKind == "key" ? key : value);
      this.$emit("update:objectValue", value);
      this.$emit("update:keyValue", key);
    },

    async createItem() {
      if (!this.createItemLabel) return;
      try {
        const item = await this.create!.getItem(
          this.search!,
          this.createItemLabel
        );
        if (!item) return;
        this.onInput(item);
      } catch (e: unknown) {
        this.error = [getMessageForError(e)];
      }
    },

    onMenuUpdate(menuOpen: boolean) {
      if (menuOpen && this.reloadOnOpen) this.listCaller();
    },
  },

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
        function (c) {
          throw "expected calls to be made with invokeWithArgs";
        },
        () => ({ id: null as any }),
        (c, args) => c.get(args.id)
      )
      .setConcurrency("debounce");

    this.listCaller = new ModelApiClient(this.modelObjectMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller("list", (c) => {
        return c.list({
          pageSize: 100,
          ...this.params,
          search: this.search || undefined,
        });
      })
      .onRejected((state) => {
        this.error = [state.message || "Unknown Error"];
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
      if (
        first &&
        (this.preselectFirst ||
          (this.preselectSingle && this.items.length === 1))
      ) {
        this.onInput(first, true);
      }
    });
  },
});
</script>
