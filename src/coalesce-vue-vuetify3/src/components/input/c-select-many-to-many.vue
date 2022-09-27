<template>
  <v-autocomplete
    class="c-select-many-to-many"
    :modelValue="internalValue"
    @update:modelValue="onInput"
    multiple
    chips
    small-chips
    :deletable-chips="canDelete"
    :loading="isLoading"
    :error-messages="error"
    :items="listItems"
    v-model:search="search"
    :item-title="itemText"
    :item-value="itemValue"
    :return-object="true"
    :disabled="!modelPkValue"
    no-filter
    v-bind="inputBindAttrs"
  >
  </v-autocomplete>
</template>

<script lang="ts">
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
} from "coalesce-vue";

import { defineComponent, PropType } from "vue";

export default defineComponent({
  name: "c-select-many-to-many",

  setup(props) {
    return { ...useMetadataProps(props) };
  },

  props: {
    ...makeMetadataProps(),
    modelValue: { required: false, type: Array },
    params: { required: false, type: Object as PropType<ListParameters> },
  },

  data() {
    return {
      search: undefined as string | undefined,
      listCaller: null! as ApiStateType<"list", [], Model<ModelType>>,
      currentLoaders: [] as ApiState<any, any>[],
    };
  },

  computed: {
    modelPkValue() {
      const model = this.model as Model<ModelType>;
      return model ? (model as any)[model.$metadata.keyProp.name] : null;
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
      // TODO: do this better to prevent duplicates?
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
      return this.modelValue || [];
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
      if (typeof this.$attrs["item-title"] === "function") {
        return this.$attrs["item-title"](item);
      }

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

      this.internalValue.forEach((vm) => {
        if (!newItems.has(vm)) {
          if (vm instanceof ViewModel && this.canDelete) {
            this.pushLoader(vm.$delete);
            vm.$delete()
              .then(() => {
                // Delete successful. No need to keep the state around.
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
          }
        } else {
          items.push(vm);
        }
      });

      // Handle many-to-many collections.
      // The items available for selection will be unsaved model
      // instances of the join table's type that contain the key values
      // for either side of the relationship.
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

          this.pushLoader(vm.$save);
          vm.$save()
            .then(() => {
              // Save successful. No need to keep the state around.
              this.currentLoaders = this.currentLoaders.filter(
                (l) => l != vm.$save
              );
            })
            .catch(() => {
              // Item failed to save. Remove it from the selected set of values.
              this.emitInput(this.internalValue.filter((i) => i != vm));
            });
        }
      });

      this.emitInput(items);
    },

    emitInput(items: any[]) {
      if (this.model) {
        return ((this.model as any)[this.collectionMeta.name] = items);
      }

      this.$emit("update:modelValue", items);
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
