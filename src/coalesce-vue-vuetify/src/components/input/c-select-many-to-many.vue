<template>
  <v-autocomplete
    class="c-select-many-to-many"
    :value="internalValue"
    @input="onInput"
    multiple
    chips
    small-chips
    :deletable-chips="canDelete"
    :loading="loading"
    :items="listItems"
    :search-input.sync="search"
    :item-text="$attrs['item-text'] || itemText"
    :item-value="itemValue"
    :return-object="true"
    :disabled="!modelPkValue"
    no-filter
    v-bind="inputBindAttrs"
  >
  </v-autocomplete>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from "vue-property-decorator";
import MetadataComponent from "../c-metadata-component";
import CDisplay from "../display/c-display";
import {
  ListParameters,
  Model,
  ModelType,
  ModelValue,
  convertToModel,
  ListApiState,
  ListResultPromise,
  ModelApiClient,
  mapParamsToDto,
  modelDisplay,
  ViewModel,
  ViewModelFactory,
  BehaviorFlags
} from "coalesce-vue";

@Component({
  name: "c-select-many-to-many",
  components: {
    CDisplay
  }
})
export default class CSelectManyToMany extends MetadataComponent {
  public search: string | null = null;

  @Prop({ required: false })
  public value?: any;

  @Prop({ required: false })
  public params?: ListParameters;

  get loading() {
    return this.listCaller && this.listCaller.isLoading;
  }

  get modelPkValue() {
    const model = this.model as Model<ModelType>;
    return model ? (model as any)[model.$metadata.keyProp.name] : null;
  }

  get items() {
    const items = this.listCaller.result || [];
    const manyToManyMeta = this.manyToManyMeta;

    const model = this.model as Model<ModelType>;

    // Map the items into fake instances of the join entity.
    return items.map(i =>
      convertToModel(
        {
          [manyToManyMeta.farForeignKey.name]: (i as any)[
            this.foreignItemKeyPropName!
          ],
          [manyToManyMeta.farNavigationProp.name]: i,
          [manyToManyMeta.nearForeignKey.name]: this.modelPkValue,
          [manyToManyMeta.nearNavigationProp.name]: model
        },
        (this.collectionMeta.itemType as ModelValue).typeDef
      )
    );
  }

  get listItems() {
    // TODO: do this better to prevent duplicates?
    return [...this.internalValue, ...this.items];
  }

  listCaller: ListApiState<[], Model<ModelType>> &
    (() => ListResultPromise<Model<ModelType>>) = null as any;

  get listParams() {
    return { pageSize: 100, ...this.params, search: this.search || undefined };
  }

  created() {
    // This needs to be late initialized so we have the correct "this" reference.
    this.listCaller = new ModelApiClient(this.foreignItemModelType)
      .$makeCaller("list", function(this: CSelectManyToMany, c) {
        return c.list(this.listParams);
      })
      .setConcurrency("debounce");

    this.$watch(
      () => JSON.stringify(mapParamsToDto(this.listParams)),
      () => this.listCaller!()
    );
  }

  mounted() {
    // Access this so it will throw an error if the meta props aren't in order.
    this.manyToManyMeta;
    this.listCaller();
  }

  itemText(item: any) {
    if (Array.isArray(item)) {
      // Workaround for https://github.com/vuetifyjs/vuetify/issues/8793
      return null;
    }

    const display = modelDisplay(this.foreignItemOf(item) || item);
    return display;
  }

  itemValue(item: any) {
    if (Array.isArray(item)) {
      // Workaround for https://github.com/vuetifyjs/vuetify/issues/8793
      return null;
    }

    item = this.foreignItemOf(item);
    return item[this.foreignItemKeyPropName];
  }

  onInput(value: any[]) {
    const existingItems = new Set(this.internalValue);
    const newItems = new Set(value);

    const items: any[] = [];

    this.internalValue.forEach(i => {
      if (!newItems.has(i)) {
        if (i instanceof ViewModel && this.canDelete) {
          // TODO: Display the status of this somewhere.
          // TODO: Add it back into the selected items if the delete fails?
          i.$delete();
        }
      } else {
        items.push(i);
      }
    });

    // Handle many-to-many collections.
    // The items available for selection will be unsaved model
    // instances of the join table's type that contain the key values
    // for either side of the relationship.
    newItems.forEach(i => {
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
        // TODO: Display the status of this somewhere.
        // TODO: Remove it from the selected items if the save fails?
        vm.$save();
      }
    });

    if (this.model) {
      return ((this.model as any)[this.collectionMeta.name] = items);
    }

    this.$emit("input", items);
  }

  get canDelete() {
    return (
      ((this.collectionMeta.itemType as ModelValue).typeDef.behaviorFlags &
        BehaviorFlags.Delete) !=
      0
    );
  }

  get collectionMeta() {
    const valueMeta = this.valueMeta;
    if (valueMeta && valueMeta.type == "collection") {
      return valueMeta;
    } else {
      throw Error(
        "c-select-many-to-many requires value metadata for a collection. Specify it with the 'for' prop'"
      );
    }
  }

  get manyToManyMeta() {
    const collectionMeta = this.collectionMeta;
    if ("manyToMany" in collectionMeta) {
      return collectionMeta.manyToMany!;
    }
    throw Error(
      "c-select-many-to-many requires value metadata for a many-to-many collection. Specify it with the 'for' prop'"
    );
  }

  get internalValue(): any[] {
    if (this.model && this.collectionMeta) {
      return (this.model as any)[this.collectionMeta.name] || [];
    }
    return this.value || [];
  }

  foreignItemOf(value: any) {
    return value[this.manyToManyMeta!.farNavigationProp.name];
  }

  get foreignItemModelType() {
    return this.manyToManyMeta.typeDef;
  }

  get foreignItemKeyPropName() {
    return this.foreignItemModelType.keyProp.name;
  }
}
</script>
