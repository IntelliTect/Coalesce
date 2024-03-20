<template>
  <v-input
    class="c-select"
    :class="{
      'c-select--is-menu-active': menuOpen,
    }"
    :error-messages="error"
    :focused="focused"
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :modelValue="internalModelValue"
    #default="{ isDisabled, isReadonly, isValid }"
  >
    <v-field
      persistent-clear
      :error="isValid.value === false"
      append-inner-icon="$dropdown"
      v-bind="inputBindAttrs"
      :clearable="!isDisabled.value && !isReadonly.value && isClearable"
      :active="!!internalModelValue || focused || !!placeholder"
      :dirty="!!internalModelValue"
      :focused="focused"
      @click:clear.stop.prevent="onInput(null, true)"
      @keydown="!isDisabled.value && !isReadonly.value && onInputKey($event)"
    >
      <div class="v-field__input">
        <slot
          v-if="internalModelValue"
          name="selected-item"
          :item="internalModelValue"
          :search="search"
        >
          <span class="v-select__selection">
            <slot name="item" :item="internalModelValue" :search="search">
              <c-display
                class="v-select__selection-text"
                :model="internalModelValue"
              />
            </slot>
          </span>
        </slot>

        <input
          type="text"
          ref="mainInputRef"
          v-model="mainValue"
          @focus="focused = true"
          @blur="focused = false"
          :disabled="isDisabled.value"
          :readonly="isReadonly.value"
          :placeholder="internalModelValue ? undefined : placeholder"
        />
      </div>
    </v-field>

    <v-menu
      :modelValue="menuOpen"
      @update:modelValue="
        !$event
          ? closeMenu()
          : !isDisabled.value && !isReadonly.value && openMenu()
      "
      activator="parent"
      :close-on-content-click="false"
      contentClass="c-select__menu-content"
      origin="top"
      location="top"
    >
      <v-sheet
        @keydown.capture.down.stop.prevent="
          pendingSelection = Math.min(
            listItems.length - 1,
            pendingSelection + 1
          )
        "
        @keydown.capture.up.stop.prevent="
          pendingSelection = Math.max(0, pendingSelection - 1)
        "
        @keydown.capture.enter.stop.prevent="confirmPendingSelection"
        @keydown.capture.tab.stop.prevent="confirmPendingSelection"
      >
        <v-text-field
          v-model="search"
          ref="searchRef"
          hide-details="auto"
          prepend-inner-icon="fa fa-search"
          :loading="listCaller.isLoading"
          :error-messages="
            listCaller.wasSuccessful == false
              ? listCaller.message ?? undefined
              : ''
          "
          clearable
          placeholder="Search"
          variant="filled"
        >
        </v-text-field>

        <!-- TODO: i18n -->
        <div
          v-if="!createItemLabel && !listItems.length"
          class="grey--text px-4 my-3 font-italic"
        >
          <v-fade-transition mode="out-in">
            <span v-if="listCaller.isLoading">Loading...</span>
            <span v-else>No results found.</span>
          </v-fade-transition>
        </div>

        <!-- This height shows 7 full items, with a final item partially out 
        of the scroll area to improve visual hints to the user that the can scroll the list. -->
        <v-list class="py-0" max-height="302" ref="listRef" density="compact">
          <v-list-item
            v-if="createItemLabel"
            class="c-select__create-item"
            @click="createItem"
            :loading="createItemLoading"
          >
            <template #prepend>
              <v-progress-circular
                class="mr-6"
                indeterminate
                v-if="createItemLoading"
              ></v-progress-circular>
              <v-icon v-else>$plus</v-icon>
            </template>
            <v-list-item-title>
              {{ createItemLabel }}
            </v-list-item-title>
            <v-list-item-subtitle
              v-if="createItemError"
              class="text-error font-weight-bold"
            >
              {{ createItemError }}
            </v-list-item-subtitle>
          </v-list-item>

          <v-list-item
            v-for="(item, i) in listItems"
            :key="item[modelObjectMeta.keyProp.name]"
            @click="onInput(item)"
            :value="i"
            :active="pendingSelection == i"
          >
            <v-list-item-title>
              <slot name="list-item" :item="item" :search="search">
                <slot name="item" :item="item" :search="search">
                  <c-display :model="item" />
                </slot>
              </slot>
            </v-list-item-title>
          </v-list-item>

          <!-- TODO: With this version of c-select (versus the v2 one),
        we can implement infinite scroll much easier. Consider doing this instead of having this message. -->
          <v-list-item
            v-if="listCaller.pageCount && listCaller.pageCount > 1"
            class="text-grey font-italic"
          >
            Max {{ listCaller.pageSize }} items retrieved. Refine your search to
            view more.
          </v-list-item>
        </v-list>
      </v-sheet>
    </v-menu>
  </v-input>
</template>

<style lang="scss">
.c-select {
  .v-field {
    align-items: center;
    min-height: var(--v-input-control-height, 56px);
  }
  .v-field__field {
    align-items: center;
    .v-field__input {
      flex-wrap: nowrap;
      input {
        min-width: 0;
        flex: 1 1;
        flex-basis: 1px;
        &:focus {
          outline: none;
        }
      }
    }
  }
  .v-input__details {
    padding-inline-start: 16px;
    padding-inline-end: 16px;
  }
  .v-field__clearable,
  .v-field__append-inner {
    padding-top: 0;
    .v-icon {
      transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    }
  }

  &.c-select--is-menu-active .v-field__append-inner > .v-icon {
    transform: rotate(180deg);
  }
}
</style>

<script
  lang="ts"
  setup
  generic="TModel extends Model | AnyArgCaller | undefined"
>
import { ComponentPublicInstance, ref, computed, nextTick, watch } from "vue";
import { useMetadataProps, ForSpec } from "../c-metadata-component";
import {
  ModelApiClient,
  Model,
  ModelType,
  ForeignKeyProperty,
  ModelReferenceNavigationProperty,
  ListParameters,
  mapParamsToDto,
  getMessageForError,
  mapValueToModel,
  ViewModel,
  Indexable,
  ModelValue,
  AnyArgCaller,
  ResponseCachingConfiguration,
} from "coalesce-vue";

defineOptions({
  name: "c-select",
  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

defineSlots<{
  // FUTURE: strongly type `item` based on the forspec?
  ["item"]?(props: { item: any; search: string | null }): any;
  ["selected-item"]?(props: { item: any; search: string | null }): any;
  ["list-item"]?(props: { item: any; search: string | null }): any;
}>();

const props = withDefaults(
  defineProps<{
    /** An object owning the value to be edited that is specified by the `for` prop. */
    model?: TModel;

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"firstName"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
     * * A string in dot-notation that starts with a type name. E.g. `"Person.firstName"`.
     */
    for: ForSpec<
      TModel,
      ForeignKeyProperty | ModelReferenceNavigationProperty | ModelValue
    >;

    clearable?: boolean;
    placeholder?: string;
    preselectFirst?: boolean;
    preselectSingle?: boolean;
    openOnClear?: boolean;
    reloadOnOpen?: boolean;
    params?: ListParameters;

    // DONT use defineModel for these. We don't want to capture local state if the parent isn't binding it
    // since we have 4 different binding sources in this component, we'll get stuck on the values of the ones that aren't used.
    keyValue?: any;
    objectValue?: Model;
    modelValue?: any;

    /** Response caching configuration for the `/get` and `/list` API calls made by the component.
     * See https://intellitect.github.io/Coalesce/stacks/vue/layers/api-clients.html#response-caching. */
    cache?: ResponseCachingConfiguration | boolean;

    create?: {
      getLabel: (search: string, items: Model<ModelType>[]) => string | false;
      getItem: (search: string, label: string) => Promise<Model<ModelType>>;
    };
  }>(),
  { openOnClear: true }
);

const emit = defineEmits<{
  "update:keyValue": [value: any];
  "update:objectValue": [value: any];
  "update:modelValue": [value: any];
}>();

const mainInputRef = ref<HTMLInputElement>();
const listRef = ref<ComponentPublicInstance>();
const searchRef = ref<ComponentPublicInstance>();

const { inputBindAttrs, modelMeta, valueMeta, valueOwner } = useMetadataProps(
  props,
  (v) =>
    // Use the navigation metadata (if exists) to drive the logic in here,
    // as it will be a better source to pull label, hint, etc. from.
    v.role == "foreignKey" && "navigationProp" in v ? v.navigationProp! : v
);

const search = ref(null as string | null);
const error = ref([] as string[]);
const focused = ref(false);
const menuOpen = ref(false);
const menuOpenForced = ref(false);
const searchChanged = ref(new Date());
const mainValue = ref("");
const createItemLoading = ref(false);
const createItemError = ref("" as string | null);
const pendingSelection = ref(0);

/** The model representing the current selected item
 * in the case that only the PK was provided to the component.
 * This is maintained in a variable to prevent its reference from
 * changing unexpectedly, which causes Vuetify to annoying things.
 */
const keyFetchedModel = ref(null as any);

/** The effective clearability state of the dropdown. */
const isClearable = computed((): boolean => {
  if (typeof props.clearable == "boolean")
    // If explicitly given a value, use that value.
    return props.clearable;

  // Check to see if the foreign key is nullable (i.e. doesn't have a 'required' rule).
  return !!(modelKeyProp.value && !modelKeyProp.value.rules?.required);
});

/** The property on `valueOwner` which holds the foreign key being selected for, or `null` if there is no such property. */
const modelKeyProp = computed((): ForeignKeyProperty | null => {
  const meta = valueMeta.value!;
  if (meta.role == "foreignKey" && "principalType" in meta) {
    return meta;
  }
  if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
    return meta.foreignKey;
  }
  return null;
});

/** The property on `valueOwner` which holds the model object being selected for, or `null` if there is no such property. */
const modelObjectProp = computed(
  (): ModelReferenceNavigationProperty | ModelValue | null => {
    const meta = valueMeta.value!;
    if (meta.role == "foreignKey" && "navigationProp" in meta) {
      return meta.navigationProp || null;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta;
    }
    if (meta.role == "value" && meta.type == "model") {
      return meta;
    }
    return null;
  }
);

/**
 * Whether the metadata provided to `for` and retrieved via `valueMeta` is for a key or an object.
 * Dictates whether v-model will attempt to bind a key or an object.
 */
const primaryBindKind = computed(() => {
  if (!valueMeta.value) {
    throw "c-select requires metadata";
  }

  if (valueMeta.value.role == "foreignKey") {
    return "key";
  }
  if (valueMeta.value.type == "model") {
    if (typeof props.modelValue != "object" && props.modelValue !== undefined) {
      throw (
        "Expected a model object to be bound to modelValue, but received a " +
        typeof props.modelValue
      );
    }
    return "model";
  }

  throw "The 'role' of the metadata provided for c-select must be 'foreignKey', or the type must be 'model' ";
});

/** The metadata of the type being selected by the dropdown. */
const modelObjectMeta = computed(() => {
  var meta = valueMeta.value!;
  if (meta.role == "foreignKey" && "principalType" in meta) {
    return meta.principalType;
  } else if (meta.type == "model") {
    return meta.typeDef;
  } else {
    throw `Value ${meta.name} must be a foreignKey or model type to use c-select.`;
  }
});

/** The effective object (whose type is described by `modelObjectMeta`) that has been provided to the component. */
const internalModelValue = computed(() => {
  if (props.objectValue) {
    return props.objectValue;
  }
  if (
    valueOwner.value &&
    modelObjectProp.value &&
    valueOwner.value[modelObjectProp.value.name]
  ) {
    return valueOwner.value[modelObjectProp.value.name];
  }

  if (props.modelValue && primaryBindKind.value == "model") {
    return props.modelValue;
  }

  if (internalKeyValue.value) {
    // See if we already have a model that we're using to represent a key-only binding.
    // Storing this object prevents it from flipping between different instances
    // obtained from either getCaller or listCaller,
    // which causes vuetify to reset its search when the object passed to v-select's `modelValue` prop changes.
    if (
      keyFetchedModel.value &&
      internalKeyValue.value ===
        keyFetchedModel.value[modelObjectMeta.value.keyProp.name]
    ) {
      return keyFetchedModel.value;
    }

    // All we have is the PK. First, check if it is already in our item array.
    // If so, capture it. If not, request the object from the server.
    const item = items.value.filter(
      (i) =>
        internalKeyValue.value ===
        (i as any)[modelObjectMeta.value.keyProp.name]
    )[0];
    if (item) {
      // eslint-disable-next-line vue/no-side-effects-in-computed-properties
      keyFetchedModel.value = item;
      return item;
    }

    // See if we obtained the item via getCaller.
    const singleItem = getCaller.result;
    if (
      singleItem &&
      internalKeyValue.value ===
        (singleItem as any)[modelObjectMeta.value.keyProp.name]
    ) {
      // eslint-disable-next-line vue/no-side-effects-in-computed-properties
      keyFetchedModel.value = singleItem;
      return singleItem;
    }

    if (!listCaller.isLoading && getCaller.args.id != internalKeyValue.value) {
      // Only request the single item if the list isn't currently loading,
      // and if the last requested key is not the key we're looking for.
      // (this prevents an infinite loop of invokes if the call to the server fails.)
      // The single item may end up coming back from a pending list call.
      // eslint-disable-next-line vue/no-side-effects-in-computed-properties
      getCaller.args.id = internalKeyValue.value;
      getCaller.invokeWithArgs();
    }
  }
  return null;
});

/** The effective key (whose type is described by `modelObjectMeta`) that has been provided to the component. */
const internalKeyValue = computed(() => {
  let value: any;
  if (props.keyValue) {
    value = props.keyValue;
  } else if (valueOwner.value && modelKeyProp.value) {
    value = valueOwner.value[modelKeyProp.value.name];
  } else if (props.modelValue && primaryBindKind.value == "key") {
    value = props.modelValue;
  } else {
    value = null;
  }

  if (value != null) {
    // Parse the value in case we were given a string instead of a number, or something like that, via the `keyValue` prop.
    // This prevents `internalModelValue` from getting confused and infinitely calling the `getCaller`.
    return mapValueToModel(value, modelObjectMeta.value.keyProp);
  }

  return null;
});

/** The effective set of validation rules to pass to the v-select. */
const effectiveRules = computed(() => {
  // If we were explicitly given rules, use those.
  if (inputBindAttrs.value.rules) return inputBindAttrs.value.rules;

  if (valueOwner.value instanceof ViewModel && modelKeyProp.value) {
    // We're binding to a ViewModel instance.
    // Grab the rules from the instance, because it may contain custom rules
    // and/or other rule changes that have been customized in userland beyond what the metadata provides.

    // Validate using the key, not the navigation. The FK
    // is the actual scalar value that gets sent to the server,
    // and is the prop that we generate things like `required` onto.
    // We need to translate the rule functions to pass the selected FK instead
    // of the selected model object.
    return valueOwner.value
      .$getRules(modelKeyProp.value)
      ?.map((rule) => () => rule(internalKeyValue.value));
  }

  // Look for validation rules from the metadata on the key prop.
  // The foreign key is always the one that provides validation rules
  // for navigation properties - never the navigation property itself.
  if (modelKeyProp.value?.rules) {
    return Object.values(modelKeyProp.value.rules);
  }

  return [];
});

const items = computed(() => {
  return listCaller?.result || [];
});

const listItems = computed((): Indexable<Model<ModelType>>[] => {
  return items.value;
});

const createItemLabel = computed(() => {
  if (!props.create || !search.value) return null;

  const result = props.create.getLabel(search.value, items.value);
  if (result) {
    return result;
  }
  return null;
});

function onInput(
  value: Model<ModelType> | null | undefined,
  dontFocus = false
) {
  value = value ?? null;
  if (value == null && !isClearable.value) {
    return;
  }

  // Clear any manual errors
  error.value = [];

  const key = value ? (value as any)[modelObjectMeta.value.keyProp.name] : null;
  keyFetchedModel.value = value;

  if (valueOwner.value) {
    if (modelKeyProp.value) {
      valueOwner.value[modelKeyProp.value.name] = key;
    }
    if (modelObjectProp.value) {
      valueOwner.value[modelObjectProp.value.name] = value;
    }
  }

  emit("update:modelValue", primaryBindKind.value == "key" ? key : value);
  emit("update:objectValue", value);
  emit("update:keyValue", key);
  pendingSelection.value = value ? listItems.value.indexOf(value) : 0;

  // When the input value is cleared, re-focus the dropdown
  // to allow the user to enter new search input.
  // Without this, pressing backspace to delete the current value
  // will cause the search field to lose focus and further keyboard input will do nothing.
  if (!dontFocus) {
    if (!value) {
      openMenu();
    } else {
      closeMenu(true);
    }
  }
}

/** When a key is pressed on the top level input */
function onInputKey(event: KeyboardEvent) {
  switch (event.key.toLowerCase()) {
    case "delete":
    case "backspace":
      if (!menuOpen.value) {
        onInput(null, true);
        event.stopPropagation();
        event.preventDefault();
      }
      return;
    case "esc":
    case "escape":
      event.stopPropagation();
      event.preventDefault();
      closeMenu(true);
      return;
    case " ":
    case "enter":
    case "up":
    case "arrowup":
    case "down":
    case "arrowdown":
    case "spacebar":
    case "space":
      event.stopPropagation();
      event.preventDefault();
      openMenu();
      return;
  }
}

function confirmPendingSelection() {
  var item = listItems.value[pendingSelection.value];
  if (!item) return;
  onInput(item);
}

async function createItem() {
  if (!createItemLabel.value) return;
  try {
    createItemLoading.value = true;
    const item = await props.create!.getItem(
      search.value!,
      createItemLabel.value
    );
    if (!item) return;
    onInput(item);
    listCaller(); // Refresh the list, because the new item is probably now in the results.
  } catch (e: unknown) {
    createItemError.value = getMessageForError(e);
  } finally {
    createItemLoading.value = false;
  }
}

async function openMenu(select?: boolean) {
  if (select == undefined) {
    // Select the whole search input if it hasn't changed recently.
    // If it /has/ changed recently, it means the user is actively typing and probably
    // doesn't want to use what they're typing.
    select = new Date().valueOf() - searchChanged.value.valueOf() > 1000;
  }

  if (menuOpen.value) return;
  menuOpen.value = true;

  if (props.reloadOnOpen) listCaller();

  await nextTick();
  const input = searchRef.value?.$el.querySelector("input") as HTMLInputElement;

  // Wait for the menu fade-in animation to unhide the content root
  // before we try to focus the search input, because otherwise it wont work.
  // https://stackoverflow.com/questions/19669786/check-if-element-is-visible-in-dom
  const start = performance.now();

  // Force the menu open while we wait, because otherwise if a user clicks and then rapidly types a character,
  // the typed character will process before the click, resulting in the click toggling the menu closed
  // after the typed character opened the menu.
  menuOpenForced.value = true;

  while (
    // cap waiting at 100ms
    start + 100 > performance.now() &&
    (!input.offsetParent || input != document.activeElement)
  ) {
    input.focus();
    await new Promise((resolve) => setTimeout(resolve, 1));
  }

  menuOpenForced.value = false;

  if (select) {
    input.select();
  }
}

function closeMenu(force = false) {
  if (!menuOpen.value) return;
  if (menuOpenForced.value && !force) return;

  menuOpenForced.value = false;
  menuOpen.value = false;
  mainInputRef.value?.focus();
}

function toggleMenu() {
  if (menuOpen.value) closeMenu();
  else openMenu();
}

const propMeta = valueMeta.value;
if (!propMeta) {
  throw "c-select requires value metadata. Specify it with the 'for' prop'";
}

/**
 * A caller that will be used to resolve the full object when the only thing
 * that has been provided to c-select is a primary key value.
 */
const getCaller = new ModelApiClient(modelObjectMeta.value)
  .$withSimultaneousRequestCaching()
  .$makeCaller(
    "item",
    function () {
      throw "expected calls to be made with invokeWithArgs";
    },
    () => ({ id: null as any }),
    (c, args) => c.get(args.id)
  )
  .setConcurrency("debounce");

const listCaller = new ModelApiClient(modelObjectMeta.value)
  .$withSimultaneousRequestCaching()
  .$makeCaller("list", (c) => {
    return c.list({
      pageSize: 100,
      ...props.params,
      search: search.value || undefined,
    });
  })
  .onFulfilled(() => {
    pendingSelection.value = 0;
  })
  .onRejected((state) => {
    error.value = [state.message || "Unknown Error"];
  })
  .setConcurrency("debounce");

watch(
  () => JSON.stringify(mapParamsToDto(props.params)),
  () => listCaller()
);

watch(
  () => props.cache,
  (c) => {
    getCaller.useResponseCaching(c === true ? {} : c);
    listCaller.useResponseCaching(c === true ? {} : c);
  },
  { immediate: true }
);

watch(pendingSelection, async () => {
  await nextTick();
  await nextTick();
  var listDiv = listRef.value?.$el as HTMLElement;
  var selectedItem = listDiv?.querySelector(".v-list-item--active");
  selectedItem?.scrollIntoView?.({
    behavior: "auto",
    block: "nearest",
    inline: "nearest",
  });
});

watch(search, (newVal: any, oldVal: any) => {
  searchChanged.value = new Date();
  if (newVal != oldVal) {
    listCaller();
  }
});

watch(mainValue, (val) => {
  if (val) {
    nextTick(() => (mainValue.value = ""));
    searchChanged.value = new Date();
    if (!menuOpen.value) {
      search.value = val;
      openMenu(false);
    } else {
      search.value ||= "";
      search.value += val;
    }
  }
});

watch(createItemLabel, () => {
  createItemError.value = null;
});

// Load the initial contents of the list.
listCaller().then(() => {
  if (internalModelValue.value || internalKeyValue.value) {
    // Don't preselect if there's already a value selected.
    return;
  }

  const first = items.value[0];
  if (
    first &&
    (props.preselectFirst ||
      (props.preselectSingle && items.value.length === 1))
  ) {
    onInput(first, true);
  }
});

defineExpose({
  menuOpen: menuOpen,
  search: search,
});
</script>
