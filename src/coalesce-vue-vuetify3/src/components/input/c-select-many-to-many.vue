<template>
  <v-autocomplete
    class="c-select-many-to-many"
    :modelValue="internalValue"
    @update:modelValue="onInput"
    multiple
    chips
    small-chips
    :closable-chips="canDelete"
    :loading="isLoading"
    :error-messages="error"
    :items="listItems"
    v-model:search="search"
    :item-title="itemText"
    :item-value="itemValue"
    :return-object="true"
    :disabled="isDisabled"
    no-filter
    v-bind="inputBindAttrs"
  >
  </v-autocomplete>
</template>

<script lang="ts" setup generic="TModel extends Model">
import { ForSpec, useMetadataProps } from "../c-metadata-component";
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
  ApiState,
  Indexable,
  ModelCollectionNavigationProperty,
  ResponseCachingConfiguration,
} from "coalesce-vue";

import { computed, ref, watch } from "vue";

defineOptions({
  name: "c-select-many-to-many",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

const modelValue = defineModel<Model[] | null>();
const props = defineProps<{
  /** An object owning the value to be edited that is specified by the `for` prop. */
  model: TModel;

  /** A metadata specifier for the value being bound. One of:
   * * A string with the name of the value belonging to `model`. E.g. `"caseProducts"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.props.caseProducts`.
   * * A string in dot-notation that starts with a type name. E.g. `"Case.caseProducts"`.
   */
  for: ForSpec<TModel, ModelCollectionNavigationProperty & { manyToMany: {} }>;

  params?: ListParameters;

  /** Response caching configuration for the `/get` and `/list` API calls made by the component.
   * See https://intellitect.github.io/Coalesce/stacks/vue/layers/api-clients.html#response-caching. */
  cache?: ResponseCachingConfiguration | boolean;

  itemTitle?: (item: any) => string;
}>();

const emit = defineEmits<{
  (e: "deleting", vm: ViewModel): void;
  (e: "deleted", vm: ViewModel): void;
  (e: "adding", vm: ViewModel): void;
  (e: "added", vm: ViewModel): void;
}>();

const { inputBindAttrs, valueMeta } = useMetadataProps(props);

const search = ref<string>();
const currentLoaders = ref([] as ApiState<any, any>[]);

const modelPkValue = computed(() => {
  const model = props.model as Model<ModelType>;
  return model ? (model as any)[model.$metadata.keyProp.name] : null;
});

const isDisabled = computed(() => {
  if (props.model instanceof ViewModel && props.model.$isAutoSaveEnabled) {
    // If autosave is enabled (and therefore we're going to be calling
    // APIs on the viewmodel automatically, we can only do so if we're able
    // to populate the near-side foreign key on the join entity
    // using the PK of the model we're bound to.

    // This is not as precise a check as it could be, since if deep autosaves are enabled,
    // once `props.model` gets saved then FKs will be fixed up and then the active unsaved
    // will become valid and get saved too.
    return modelPkValue.value == null;
  } else {
    // If not autosaving then we shouldn't force disabled
    // since if bulkSaves are being used, bulkSaves can handle
    // missing FK values as long as navigation properties are populated
    // (which we do - see the `items` computed below.)
    return false;
  }
});

const items = computed(() => {
  const items = listCaller.result || [];
  const manyToMany = manyToManyMeta.value;

  const model = props.model as Model<ModelType>;

  // Map the items into fake instances of the join entity.
  return items.map((i) =>
    convertToModel(
      {
        [manyToMany.farForeignKey.name]: (i as any)[
          foreignItemKeyPropName.value!
        ],
        [manyToMany.farNavigationProp.name]: i,
        [manyToMany.nearForeignKey.name]: modelPkValue.value,
        [manyToMany.nearNavigationProp.name]: model,
      },
      collectionMeta.value.itemType.typeDef
    )
  );
});

const listItems = computed(() => {
  const added = new Set();
  const ret = [];

  // Make a lookup of all the candidate items
  // so we can use this to determine if a selected item
  // matches the search criteria that the server is using.
  const candidateItems = new Map<any, any>();
  for (const item of items.value) {
    const key = itemValue(item);
    candidateItems.set(key, item);
  }

  for (const item of internalValue.value) {
    // Put the selected values first
    const key = itemValue(item);
    added.add(key);

    // If we're not searching, put all the selected items at the start of the list.
    // If we are searching, add the selected item if the server returned it as a candidate for the current search query,
    // or if it matches locally by name (to avoid weird/unexpected behavior if server search criteria is weird).
    if (
      !search.value ||
      candidateItems.has(key) ||
      itemText(item)?.toLowerCase().includes(search.value.toLowerCase())
    ) {
      ret.push(item);
    }
  }

  for (const item of items.value) {
    // Add in all the non-selected values after the selected values,
    // excluding any items previously listed.
    // Vuetify2 would deduplicate for us; Vuetify3 does not.
    const key = itemValue(item);
    if (added.has(key)) continue;
    added.add(key);
    ret.push(item);
  }

  return ret;
});

const listParams = computed((): ListParameters => {
  return {
    pageSize: 100,
    ...props.params,
    search: search.value || undefined,
  };
});

const isLoading = computed((): boolean => {
  return currentLoaders.value.some((l) => l.isLoading);
});

const error = computed((): string[] => {
  for (const loader of currentLoaders.value) {
    if (!loader.wasSuccessful && loader.message) {
      return [loader.message];
    }
  }
  return [];
});

const canDelete = computed((): boolean => {
  return (
    (collectionMeta.value.itemType.typeDef.behaviorFlags &
      BehaviorFlags.Delete) !=
    0
  );
});

const collectionMeta = computed((): ModelCollectionNavigationProperty => {
  const meta = valueMeta.value;
  if (meta?.type == "collection" && "manyToMany" in meta) {
    return meta;
  } else {
    throw Error(
      "c-select-many-to-many requires value metadata for a many-to-many collection. Specify it with the 'for' prop'"
    );
  }
});

const manyToManyMeta = computed(() => {
  if ("manyToMany" in collectionMeta.value) {
    return collectionMeta.value.manyToMany!;
  }
  throw Error(
    "c-select-many-to-many requires value metadata for a many-to-many collection. Specify it with the 'for' prop'"
  );
});

const internalValue = computed((): any[] => {
  if (props.model && collectionMeta.value) {
    return (props.model as any)[collectionMeta.value.name] || [];
  }
  return modelValue.value || [];
});

const foreignItemModelType = computed((): ModelType => {
  return manyToManyMeta.value.typeDef;
});

const foreignItemKeyPropName = computed((): string => {
  return foreignItemModelType.value.keyProp.name;
});

function pushLoader(loader: ApiState<any, any>) {
  // User performed an action to make this happen.
  // Remove all loaders that aren't loading so that if there are any that are in an error state,
  // the old error state is cleared out to make room for any future error states.
  var newArray = currentLoaders.value.filter((l) => l.isLoading);

  // Reset the error message of the loader. If the attempt previously failed,
  // we don't want to suddenly flash an old error message to the user.
  loader.message = null;

  newArray.push(loader);
  currentLoaders.value = newArray;
}

function itemText(item: any): string | null {
  if (typeof props.itemTitle === "function") {
    return props.itemTitle(item);
  }

  const foreignItem = foreignItemOf(item);
  if (!foreignItem) {
    const itemFarFk = manyToManyMeta.value.farForeignKey;
    const itemFarNav = manyToManyMeta.value.farNavigationProp;
    console.warn(
      `c-select-many-to-many: Unable to display the name of %o because '${itemFarNav.name}' is not loaded.`,
      item
    );

    return item[itemFarFk.name] || modelDisplay(item);
  }

  return modelDisplay(foreignItem);
}

function itemValue(item: any) {
  if (Array.isArray(item)) {
    // Workaround for https://github.com/vuetifyjs/vuetify/issues/8793
    return null;
  }

  const foreignItem = foreignItemOf(item);
  if (foreignItem) {
    return foreignItem[foreignItemKeyPropName.value];
  }
  return item[manyToManyMeta.value.farForeignKey.name];
}

function onInput(value: any[]) {
  const existingItems = new Set(internalValue.value);
  const newItems = new Set(value);

  const items: any[] = [];

  // Check for deleted existing items
  internalValue.value.forEach((vm) => {
    if (!newItems.has(vm)) {
      if (vm instanceof ViewModel && canDelete.value) {
        if (
          props.model instanceof ViewModel &&
          props.model.$isAutoSaveEnabled
        ) {
          pushLoader(vm.$delete);
          emit("deleting", vm);
          vm.$delete()
            .then(() => {
              // Delete successful. No need to keep the state around.
              emit("deleted", vm);
              currentLoaders.value = currentLoaders.value.filter(
                (l) => l != vm.$delete
              );
            })
            .catch(() => {
              // Item failed to delete. Re-add it to the selected set of values.
              // Note that the item will be re-added to the end because we don't
              // know where for sure which index to re-insert it at.
              // Because this is an error case, this is probably acceptable.
              emitInput([...internalValue.value, vm]);
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

      if (props.model instanceof ViewModel && props.model.$isAutoSaveEnabled) {
        // Only perform a save if the model we're bound to is autosaving.
        // Otherwise, its possible that bulk saves are being used and
        // that an automatic save here would be undesirable.

        pushLoader(vm.$save);
        emit("adding", vm);
        vm.$save()
          .then(() => {
            // Save successful. No need to keep the state around.
            emit("added", vm);
            currentLoaders.value = currentLoaders.value.filter(
              (l) => l != vm.$save
            );
          })
          .catch(() => {
            // Item failed to save. Remove it from the selected set of values.
            emitInput(internalValue.value.filter((i) => i != vm));
          });
      }
    }
  });

  emitInput(items);
}

function emitInput(items: any[]) {
  if (props.model) {
    return ((props.model as any)[collectionMeta.value.name] = items);
  }

  modelValue.value = items;
}

function foreignItemOf(value: any): Indexable<Model> | null | undefined {
  return value[manyToManyMeta.value!.farNavigationProp.name];
}

// This needs to be late initialized so we have the correct "this" reference.
const listCaller = new ModelApiClient(foreignItemModelType.value)
  .$withSimultaneousRequestCaching()
  .$makeCaller("list", (c) => {
    return c.list(listParams.value);
  })
  .setConcurrency("debounce");

watch(
  () => props.cache,
  () => {
    listCaller.useResponseCaching(props.cache === true ? {} : props.cache);
  },
  { immediate: true }
);

watch(
  () => JSON.stringify(mapParamsToDto(listParams.value)),
  () => {
    pushLoader(listCaller);
    listCaller!();
  }
);

// Access this so it will throw an error if the meta props aren't in order.
manyToManyMeta.value;
listCaller();
</script>
