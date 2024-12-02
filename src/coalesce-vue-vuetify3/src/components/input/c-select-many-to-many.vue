<template>
  <c-select
    class="c-select-many-to-many"
    v-bind="inputBindAttrs"
    :for="farItemModelType"
    multiple
    :modelValue="farItems"
    @selectionChanged="selectionChanged"
    :clearable="
      !canDelete ? false : clearable === undefined ? false : clearable
    "
    :can-deselect="canDelete"
    :loading="isLoading"
    :error-messages="error"
    :itemTitle="itemText"
    :disabled="disabled || forceDisabled"
  />
</template>

<script lang="ts" setup generic="TModel extends Model">
import { ForSpec, useMetadataProps } from "../c-metadata-component";
import {
  Model,
  ModelType,
  convertToModel,
  modelDisplay,
  ViewModel,
  ViewModelFactory,
  ApiState,
  Indexable,
  ModelCollectionNavigationProperty,
  BehaviorFlags,
} from "coalesce-vue";

import { computed, ref } from "vue";

defineOptions({
  name: "c-select-many-to-many",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

const modelValue = defineModel<Model[] | null>();
const props = withDefaults(
  defineProps<{
    /** An object owning the value to be edited that is specified by the `for` prop. */
    model: TModel;

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"caseProducts"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.caseProducts`.
     * * A string in dot-notation that starts with a type name. E.g. `"Case.caseProducts"`.
     */
    for: ForSpec<
      TModel,
      ModelCollectionNavigationProperty & { manyToMany: {} }
    >;

    itemTitle?: (item: any) => string;

    clearable?: boolean;
    disabled?: boolean;
  }>(),
  {
    clearable: undefined,
  }
);

const emit = defineEmits<{
  (e: "deleting", vm: ViewModel): void;
  (e: "deleted", vm: ViewModel): void;
  (e: "adding", vm: ViewModel): void;
  (e: "added", vm: ViewModel): void;
}>();

const fakeItemMarker = Symbol();

const { inputBindAttrs, valueMeta } = useMetadataProps(props);

const currentLoaders = ref([] as ApiState<any, any>[]);

const modelPkValue = computed(() => {
  const model = props.model as Model<ModelType>;
  return model ? (model as any)[model.$metadata.keyProp.name] : null;
});

const forceDisabled = computed(() => {
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

function mapFarItemToMiddleItem(farItem: any) {
  const manyToMany = manyToManyMeta.value;
  const model = props.model as Model<ModelType>;
  return convertToModel(
    {
      [manyToMany.farForeignKey.name]: (farItem as any)[
        farItemKeyPropName.value!
      ],
      [manyToMany.farNavigationProp.name]: farItem,
      [manyToMany.nearForeignKey.name]: modelPkValue.value,
      [manyToMany.nearNavigationProp.name]: model,
    },
    collectionMeta.value.itemType.typeDef
  );
}

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

const farItems = computed((): any[] => {
  return internalValue.value.map((x) => {
    let ret = farItemOf(x);
    if (!ret) {
      ret = convertToModel(
        {
          [farItemKeyPropName.value]:
            x[manyToManyMeta.value.farForeignKey.name],
        },
        farItemModelType.value
      );
      (ret as any)[fakeItemMarker] = true;
    }
    return ret;
  });
});

const farItemModelType = computed((): ModelType => {
  return manyToManyMeta.value.typeDef;
});

const farItemKeyPropName = computed((): string => {
  return farItemModelType.value.keyProp.name;
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

function itemText(farItem: any): string | null {
  if (typeof props.itemTitle === "function") {
    // This mapping of the foreign item back to the middle item
    // exists for backwards compatibility with the implementation of
    // c-select-many-to-many before it became based upon c-select[multiple].
    // https://github.com/IntelliTect/Coalesce/issues/497
    const middleItem = mapFarItemToMiddleItem(farItem);
    return props.itemTitle(middleItem);
  }

  if (farItem[fakeItemMarker]) {
    // The foreign item is a "fake" item, indicating that the far side navigation property
    // was missing from the middle model. Fall back on displaying the PK.
    return modelDisplay(farItem) ?? farItem[farItemKeyPropName.value];
  } else {
    // Don't fall back to displaying the FK if the foreign item is "real",
    // since the display property of the model might really be null.
    return modelDisplay(farItem);
  }
}

function selectionChanged(farItems: any[], selected: boolean) {
  const manyToMany = manyToManyMeta.value;

  let newItems = [...internalValue.value];

  for (const farItem of farItems) {
    if (!selected) {
      const vm = newItems.find(
        (x) =>
          (x as any)[manyToMany.farForeignKey.name] ==
          farItem[farItemKeyPropName.value]
      );

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

          vm._isRemoved = true;
          if (vm.$parent instanceof ViewModel) {
            (vm.$parent.$removedItems ??= []).push(vm);
          }
        }
      }

      newItems = newItems.filter((x) => x !== vm);
    } else {
      // item selected
      let vm: ViewModel | undefined = undefined;

      if (props.model instanceof ViewModel) {
        // Look for a matching removed item (pending bulk save deletion):
        const removedItems = props.model.$removedItems;
        if (removedItems) {
          const itemIndex = removedItems.findIndex(
            (x) =>
              x.$metadata == collectionMeta.value.itemType.typeDef &&
              (x as any)[manyToMany.farForeignKey.name] ==
                farItem[farItemKeyPropName.value]
          );

          if (itemIndex >= 0) {
            // We found a matching item that was previously flagged for removal
            // (i.e. the user unchecked it moments ago). Restore it to prevent an unnecessary delete+create.
            const item = removedItems[itemIndex];
            removedItems.splice(itemIndex, 1);
            item._isRemoved = false;
            vm = item;
          }
        }
      }

      if (!vm) {
        const middleItem = mapFarItemToMiddleItem(farItem);
        vm = ViewModelFactory.get(
          middleItem.$metadata.name,
          middleItem,
          // All of the data of `middleItem`, recursing into any nested objects, IS clean.
          // However, `vm` itself is dirty because it is a brand new object with no PK.
          true
        );
        vm.$isDirty = true;
      }

      if (props.model instanceof ViewModel && props.model.$isAutoSaveEnabled) {
        // Only perform a save if the model we're bound to is autosaving.
        // Otherwise, its possible that bulk saves are being used and
        // that an automatic save here would be undesirable.

        pushLoader(vm.$save);
        emit("adding", vm);
        vm.$save()
          .then(() => {
            // Save successful. No need to keep the state around.
            emit("added", vm!);
            currentLoaders.value = currentLoaders.value.filter(
              (l) => l != vm!.$save
            );
          })
          .catch(() => {
            // Item failed to save. Remove it from the selected set of values.
            emitInput(internalValue.value.filter((i) => i != vm));
          });
      }
      newItems = [...internalValue.value, vm];
    }
  }
  emitInput(newItems);
}

function emitInput(items: any[]) {
  if (props.model) {
    return ((props.model as any)[collectionMeta.value.name] = items);
  }

  modelValue.value = items;
}

function farItemOf(value: any): Indexable<Model> | null | undefined {
  return value[manyToManyMeta.value.farNavigationProp.name];
}
</script>
