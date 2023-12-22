<template>
  <v-autocomplete
    class="c-select-many-to-many"
    :value="internalValue"
    @input="onInput"
    multiple
    chips
    small-chips
    :deletable-chips="canDelete"
    :loading="isLoading"
    :error-messages="error"
    :items="listItems"
    :search-input.sync="search"
    :item-text="$attrs['item-text'] || itemText"
    :item-value="itemValue"
    :return-object="true"
    :disabled="isDisabled"
    no-filter
    v-bind="inputBindAttrs"
  >
  </v-autocomplete>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";
import {
  ListParameters,
  Model,
  ModelType,
  convertToModel,
  ModelApiClient,
  mapParamsToDto,
  modelDisplay,
  ViewModel,
  ViewModelFactory,
  BehaviorFlags,
  ApiStateType,
  ApiState,
  Indexable,
  ModelCollectionNavigationProperty,
  ResponseCachingConfiguration,
} from "coalesce-vue";

export default defineComponent({
  name: "c-select-many-to-many",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in the autocomplete.
  inheritAttrs: false,

  setup(props) {
    return { ...useMetadataProps(props) };
  },

  props: {
    ...makeMetadataProps(),
    value: { required: false, type: Array },
    params: { required: false, type: Object as PropType<ListParameters> },

    /** Response caching configuration for the `/get` and `/list` API calls made by the component.
     * See https://intellitect.github.io/Coalesce/stacks/vue/layers/api-clients.html#response-caching. */
    cache: {
      required: false,
      type: [Object, Boolean] as PropType<
        ResponseCachingConfiguration | boolean
      >,
      default: false as any,
    },
  },

  data() {
    return {
      search: null as string | null,
      listCaller: null! as ApiStateType<"list", [], Model<ModelType>>,
      currentLoaders: [] as ApiState<any, any>[],
    };
  },

  computed: {
    modelPkValue() {
      const model = this.model as Model<ModelType>;
      return model ? (model as any)[model.$metadata.keyProp.name] : null;
    },

    isDisabled() {
      if (this.model instanceof ViewModel && this.model.$isAutoSaveEnabled) {
        // If autosave is enabled (and therefore we're going to be calling
        // APIs on the viewmodel automatically, we can only do so if we're able
        // to populate the near-side foreign key on the join entity
        // using the PK of the model we're bound to.

        // This is not as precise a check as it could be, since if deep autosaves are enabled,
        // once `this.model` gets saved then FKs will be fixed up and then the active unsaved
        // will become valid and get saved too.
        return this.modelPkValue == null;
      } else {
        // If not autosaving then we shouldn't force disabled
        // since if bulkSaves are being used, bulkSaves can handle
        // missing FK values as long as navigation properties are populated
        // (which we do - see the `items` computed below.)
        return false;
      }
    },

    items() {
      const items = this.listCaller.result || [];
      const manyToManyMeta = this.manyToManyMeta;

      const model = this.model as Model<ModelType>;

      // Map the items into fake instances of the join entity.
      return items.map((i) =>
        convertToModel(
          {
            [manyToManyMeta.farForeignKey.name]: (i as any)[
              this.foreignItemKeyPropName!
            ],
            [manyToManyMeta.farNavigationProp.name]: i,
            [manyToManyMeta.nearForeignKey.name]: this.modelPkValue,
            [manyToManyMeta.nearNavigationProp.name]: model,
          },
          this.collectionMeta.itemType.typeDef
        )
      );
    },

    listItems() {
      const added = new Set();
      const ret = [];

      for (const item of this.internalValue) {
        // Put the selected values first
        added.add(this.itemValue(item));
        ret.push(item);
      }

      for (const item of this.items) {
        // Add in all the non-selected values after the selected values,
        // excluding any items previously listed.
        // Vuetify2 would deduplicate for us; Vuetify3 does not.
        const key = this.itemValue(item);
        if (added.has(key)) continue;
        added.add(key);
        ret.push(item);
      }

      return ret;
    },

    listParams(): ListParameters {
      return {
        pageSize: 100,
        ...this.params,
        search: this.search || undefined,
      };
    },

    isLoading(): boolean {
      return this.currentLoaders.some((l) => l.isLoading);
    },

    error(): string[] {
      for (const loader of this.currentLoaders) {
        if (!loader.wasSuccessful && loader.message) {
          return [loader.message];
        }
      }
      return [];
    },

    canDelete(): boolean {
      return (
        (this.collectionMeta.itemType.typeDef.behaviorFlags &
          BehaviorFlags.Delete) !=
        0
      );
    },

    collectionMeta(): ModelCollectionNavigationProperty {
      const valueMeta = this.valueMeta;
      if (
        valueMeta &&
        valueMeta.type == "collection" &&
        "manyToMany" in valueMeta
      ) {
        return valueMeta;
      } else {
        throw Error(
          "c-select-many-to-many requires value metadata for a many-to-many collection. Specify it with the 'for' prop'"
        );
      }
    },

    manyToManyMeta() {
      const collectionMeta = this.collectionMeta;
      if ("manyToMany" in collectionMeta) {
        return collectionMeta.manyToMany!;
      }
      throw Error(
        "c-select-many-to-many requires value metadata for a many-to-many collection. Specify it with the 'for' prop'"
      );
    },

    internalValue(): any[] {
      if (this.model && this.collectionMeta) {
        return (this.model as any)[this.collectionMeta.name] || [];
      }
      return this.value || [];
    },

    foreignItemModelType(): ModelType {
      return this.manyToManyMeta.typeDef;
    },

    foreignItemKeyPropName(): string {
      return this.foreignItemModelType.keyProp.name;
    },
  },

  methods: {
    pushLoader(loader: ApiState<any, any>) {
      // User performed an action to make this happen.
      // Remove all loaders that aren't loading so that if there are any that are in an error state,
      // the old error state is cleared out to make room for any future error states.
      var newArray = this.currentLoaders.filter((l) => l.isLoading);

      // Reset the error message of the loader. If the attempt previously failed,
      // we don't want to suddenly flash an old error message to the user.
      loader.message = null;

      newArray.push(loader);
      this.currentLoaders = newArray;
    },

    itemText(item: any) {
      const foreignItem = this.foreignItemOf(item);
      if (!foreignItem) {
        const itemFarFk = this.manyToManyMeta.farForeignKey;
        const itemFarNav = this.manyToManyMeta.farNavigationProp;
        console.warn(
          `${this.$options.name}: Unable to display the name of %o because '${itemFarNav.name}' is not loaded.`,
          item
        );

        return item[itemFarFk.name] || modelDisplay(item);
      }

      return modelDisplay(foreignItem);
    },

    itemValue(item: any) {
      if (Array.isArray(item)) {
        // Workaround for https://github.com/vuetifyjs/vuetify/issues/8793
        return null;
      }

      const foreignItem = this.foreignItemOf(item);
      if (foreignItem) {
        return foreignItem[this.foreignItemKeyPropName];
      }
      return item[this.manyToManyMeta.farForeignKey.name];
    },

    onInput(value: any[]) {
      const existingItems = new Set(this.internalValue);
      const newItems = new Set(value);

      const items: any[] = [];

      // Check for deleted existing items
      this.internalValue.forEach((vm) => {
        if (!newItems.has(vm)) {
          if (vm instanceof ViewModel && this.canDelete) {
            if (
              this.model instanceof ViewModel &&
              this.model.$isAutoSaveEnabled
            ) {
              this.$emit("deleting", vm);
              this.pushLoader(vm.$delete);
              vm.$delete()
                .then(() => {
                  // Delete successful. No need to keep the state around.
                  this.$emit("deleted", vm);
                  this.currentLoaders = this.currentLoaders.filter(
                    (l) => l != vm.$delete
                  );
                })
                .catch(() => {
                  // Item failed to delete. Re-add it to the selected set of values.
                  // Note that the item will be re-added to the end because we don't
                  // know where for sure which index to re-insert it at.
                  // Because this is an error case, this is probably acceptable.
                  this.emitInput([...this.internalValue, vm]);
                });
            } else {
              // Autosave disabled. Remove (which supports bulk saves).
              // This is a bit hacky - we don't want to call $remove
              // because that will modify the collection we're currently iterating.
              // We need to mark the item as removed, but allow `emitInput` to handle
              // actually updating the collection bound to the input.

              // @ts-expect-error internal state
              vm._isRemoved = true;
              // @ts-expect-error internal state
              if (vm.$parent) {
                // @ts-expect-error internal state
                (vm.$parent.$removedItems ??= []).push(vm);
              }
            }
          }
        } else {
          items.push(vm);
        }
      });

      // Check for added new items
      newItems.forEach((i) => {
        if (!existingItems.has(i)) {
          const vm = ViewModelFactory.get(
            i.$metadata.name,
            i,
            // All of the data of `i`, recursing into any nested objects, IS clean.
            // However, `vm` itself is dirty because it is a brand new object with no PK.
            true
          );
          vm.$isDirty = true;
          items.push(vm);

          if (
            this.model instanceof ViewModel &&
            this.model.$isAutoSaveEnabled
          ) {
            // Only perform a save if the model we're bound to is autosaving.
            // Otherwise, its possible that bulk saves are being used and
            // that an automatic save here would be undesirable.

            this.pushLoader(vm.$save);
            this.$emit("adding", vm);
            vm.$save()
              .then(() => {
                // Save successful. No need to keep the state around.
                this.$emit("added", vm);
                this.currentLoaders = this.currentLoaders.filter(
                  (l) => l != vm.$save
                );
              })
              .catch(() => {
                // Item failed to save. Remove it from the selected set of values.
                this.emitInput(this.internalValue.filter((i) => i != vm));
              });
          }
        }
      });

      this.emitInput(items);
    },

    emitInput(items: any[]) {
      if (this.model) {
        return ((this.model as any)[this.collectionMeta.name] = items);
      }

      this.$emit("input", items);
    },

    foreignItemOf(value: any): Indexable<Model> | null | undefined {
      return value[this.manyToManyMeta!.farNavigationProp.name];
    },
  },

  created() {
    // This needs to be late initialized so we have the correct "this" reference.
    this.listCaller = new ModelApiClient(this.foreignItemModelType)
      .$withSimultaneousRequestCaching()
      .$makeCaller("list", (c) => {
        return c.list(this.listParams);
      })
      .setConcurrency("debounce");

    this.$watch(
      () => this.cache,
      () => {
        this.listCaller.useResponseCaching(
          this.cache === true ? {} : this.cache
        );
      },
      { immediate: true }
    );

    this.$watch(
      () => JSON.stringify(mapParamsToDto(this.listParams)),
      () => {
        this.pushLoader(this.listCaller);
        this.listCaller!();
      }
    );
  },

  mounted() {
    // Access this so it will throw an error if the meta props aren't in order.
    this.manyToManyMeta;
    this.listCaller();
  },
});
</script>
