<template>
  <div class="text-pre-wrap">
    {{
      JSON.stringify(
        {
          valueMeta: valueMeta?.name,
          valueMetaType: valueMeta?.type,
          valueMetaRole: valueMeta?.role,
          modelKeyProp,
          internalKeySet,
          internalKeyValue,
          modelMeta: modelMeta?.name,
        },
        null,
        2
      )
    }}
  </div>
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
    :disabled="isDisabled"
    :readonly="isReadonly"
    #default="{ isValid }"
  >
    <v-field
      :error="isValid.value === false"
      append-inner-icon="$dropdown"
      v-bind="fieldAttrs"
      :clearable="isInteractive && isClearable"
      :active="!!internalModelValue || focused || !!placeholder"
      :dirty="!!internalModelValue"
      :focused="focused"
      @click:clear.stop.prevent="onInput(null, true)"
      @keydown="onInputKey($event)"
    >
      <div class="v-field__input">
        <slot
          v-if="internalModelValue"
          name="selected-items"
          :item="internalModelValue"
          :search="search"
        >
          <span v-for="item in internalModelValue" class="v-select__selection">
            <slot name="item" :item="item" :search="search">
              <c-display class="v-select__selection-text" :model="item" />
            </slot>
          </span>
        </slot>

        <input
          type="text"
          ref="mainInputRef"
          v-model="mainValue"
          @mousedown.stop.prevent="
            // Intercept direct clicks on the input to short circuit `focused`
            // and v-menu's activator handler, which introduce some latency before the menu opens
            // if we allow the menu opening to be handled that way.
            // Mousedown is needed to prevent `focused` from happening.
            openMenu()
          "
          @click.stop.prevent="
            // Prevent v-menu's activator handler from running (which is a click handler, not mousedown).
            openMenu()
          "
          @focus="focused = true"
          @blur="focused = false"
          :autofocus="autofocus"
          :disabled="isDisabled"
          :readonly="isReadonly"
          :placeholder="internalModelValue ? undefined : placeholder"
        />
      </div>
    </v-field>

    <v-menu
      :modelValue="menuOpen"
      @update:modelValue="!$event ? closeMenu() : openMenu()"
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
          density="compact"
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
            :class="{ 'pending-selection': pendingSelection == i }"
            :active="
              pendingSelection == i ||
              internalKeySet.has(item[modelObjectMeta.keyProp.name])
            "
          >
            <template #prepend>
              <v-checkbox-btn
                tabindex="-1"
                :modelValue="
                  internalKeySet.has(item[modelObjectMeta.keyProp.name])
                "
              />
            </template>
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
            v-if="
              // When we do know an actual page count:
              (listCaller.pageCount && listCaller.pageCount > 1) ||
              // When `noCount` is used or counting is disabled on the server:
              (listCaller.pageCount == -1 &&
                listCaller.pageSize &&
                listItems.length >= listCaller.pageSize)
            "
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
  generic="
  TModel extends Model | AnyArgCaller | undefined, 
  TFor extends ForSpec<
    TModel,
    ForeignKeyProperty | ModelReferenceNavigationProperty | ModelValue
  > = any"
>
import {
  ComponentPublicInstance,
  ref,
  computed,
  nextTick,
  watch,
  camelize,
} from "vue";
import {
  useMetadataProps,
  ForSpec,
  useCustomInput,
} from "../c-metadata-component";
import { TypedValidationRule } from "../../util";
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
  ModelTypeLookup,
  EnumTypeLookup,
  TypeDiscriminatorToType,
  PrimaryKeyProperty,
  PropNames,
  ApiStateTypeWithArgs,
  EnumValue,
  ModelCollectionValue,
} from "coalesce-vue";
import { VField } from "vuetify/components";

defineOptions({
  name: "c-select",
  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

type SelectedModelType = TFor extends string & keyof ModelTypeLookup
  ? ModelTypeLookup[TFor]
  : TFor extends ModelCollectionValue
  ? TFor["itemType"]["typeDef"]["name"] extends keyof ModelTypeLookup
    ? ModelTypeLookup[TFor["itemType"]["typeDef"]["name"]]
    : any
  : TModel extends ApiStateTypeWithArgs<any, any, infer TArgsObj, any>
  ? TFor extends keyof TArgsObj
    ? TArgsObj[TFor]
    : any
  : TModel extends Model
  ? TFor extends PropNames<TModel["$metadata"]>
    ? TModel["$metadata"]["props"][TFor] extends ModelCollectionValue
      ? TModel["$metadata"]["props"][TFor]["itemType"]["typeDef"]["name"] extends keyof ModelTypeLookup
        ? ModelTypeLookup[TModel["$metadata"]["props"][TFor]["itemType"]["typeDef"]["name"]]
        : any
      : any
    : any
  : Model<ModelType>;

type ExtractValuesOfType<T, U> = {
  [K in keyof T]: T[K] extends U ? T[K] : never;
}[keyof T];

type FindPk<TModel extends Model> = ExtractValuesOfType<
  TModel["$metadata"]["props"],
  PrimaryKeyProperty
>;

type SelectedPkType = FindPk<SelectedModelType> extends EnumValue
  ? FindPk<SelectedModelType>["typeDef"]["name"] extends keyof EnumTypeLookup
    ? EnumTypeLookup[FindPk<SelectedModelType>["typeDef"]["name"]]
    : any
  : TypeDiscriminatorToType<FindPk<SelectedModelType>["type"]>;

type PrimaryBindType = TModel extends Model[]
  ? SelectedModelType
  : SelectedPkType;

defineSlots<{
  ["item"]?(props: { item: SelectedModelType; search: string | null }): any;
  ["selected-items"]?(props: {
    item: SelectedModelType[];
    search: string | null;
  }): any;
  ["list-item"]?(props: {
    item: SelectedModelType;
    search: string | null;
  }): any;
}>();

const props = withDefaults(
  defineProps<{
    /** An object owning the value to be edited that is specified by the `for` prop. */
    model?: TModel | null;

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"firstName"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
     * * A string in dot-notation that starts with a type name. E.g. `"Person.firstName"`.
     */
    for: TFor;

    readonly?: boolean | null;
    disabled?: boolean | null;
    autofocus?: boolean;
    clearable?: boolean;
    placeholder?: string;
    preselectFirst?: boolean;
    preselectSingle?: boolean;
    openOnClear?: boolean;
    reloadOnOpen?: boolean;
    params?: Partial<ListParameters>;

    // DONT use defineModel for these. We don't want to capture local state if the parent isn't binding it
    // since we have 4 different binding sources in this component, we'll get stuck on the values of the ones that aren't used.
    keyValues?: SelectedPkType[] | null;
    objectValues?: SelectedModelType[] | null;
    modelValue?: SelectedModelType[] | null;

    /** Response caching configuration for the `/get` and `/list` API calls made by the component.
     * See https://intellitect.github.io/Coalesce/stacks/vue/layers/api-clients.html#response-caching. */
    cache?: ResponseCachingConfiguration | boolean;

    rules?: Array<TypedValidationRule<SelectedPkType[]>>;

    create?: {
      getLabel: (search: string, items: SelectedModelType[]) => string | false;
      getItem: (search: string, label: string) => Promise<SelectedModelType>;
    };
  }>(),
  { openOnClear: true, clearable: undefined }
);

const emit = defineEmits<{
  "update:keyValues": [value: SelectedPkType[] | null];
  "update:objectValues": [value: SelectedModelType[] | null];
  "update:modelValue": [value: PrimaryBindType[] | null];
}>();

const mainInputRef = ref<HTMLInputElement>();
const listRef = ref<ComponentPublicInstance>();
const searchRef = ref<ComponentPublicInstance>();

const fieldAttrs = computed(() =>
  VField.filterProps(
    Object.fromEntries(
      // We have to perform prop name normalization ourselves here
      // because vuetify's filterProps doesn't support the non-camelized names.
      Object.entries(inputBindAttrs.value).map(([k, v]) => [camelize(k), v])
    )
  )
);

const { isDisabled, isReadonly, isInteractive } = useCustomInput(props);

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

/** The effective clearability state of the dropdown. */
const isClearable = computed((): boolean => {
  if (typeof props.clearable == "boolean")
    // If explicitly given a value, use that value.
    return props.clearable;

  return true;
});

/** The property on `valueOwner` which holds the model object being selected for, or `null` if there is no such property. */
const modelObjectProp = computed((): ModelCollectionValue | null => {
  const meta = valueMeta.value!;
  if (meta.type == "collection" && meta.itemType.type == "model") {
    return meta as ModelCollectionValue;
  }
  return null;
});

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
const internalModelValue = computed((): SelectedModelType[] | null => {
  if (
    valueOwner.value &&
    modelObjectProp.value &&
    valueOwner.value[modelObjectProp.value.name]
  ) {
    return valueOwner.value[modelObjectProp.value.name];
  }

  if (props.objectValues) {
    return props.objectValues;
  }

  if (props.modelValue && primaryBindKind.value == "model") {
    return props.modelValue;
  }

  return null;
});

/** The effective keys (whose type is described by `modelObjectMeta`) that has been provided to the component. */
const internalKeyValue = computed((): SelectedPkType[] | null => {
  let value: any;
  if (props.keyValues) {
    value = props.keyValues;
  } else if (props.modelValue && primaryBindKind.value == "key") {
    value = props.modelValue;
  } else {
    value = internalModelValue.value?.map(
      (x) => x[modelObjectMeta.value.keyProp.name]
    );
  }

  return value;
});

const internalKeySet = computed(() => new Set(internalKeyValue.value));

/** The effective set of validation rules to pass to the v-select. */
const effectiveRules = computed((): TypedValidationRule<SelectedPkType>[] => {
  // If we were explicitly given rules, use those.
  if (props.rules) return props.rules;

  return [];
});

const items = computed(() => {
  return (listCaller?.result || []) as SelectedModelType[];
});

const listItems = computed((): Indexable<SelectedModelType>[] => {
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

function onInput(value: SelectedModelType | null, dontFocus = false) {
  value = value ?? null;
  if (value == null) {
    return;
  }

  const pkPropName = modelObjectMeta.value.keyProp.name;
  const key = value ? (value as any)[pkPropName] : null;

  const idx = internalKeyValue.value?.findIndex((x) => x == key);
  const isSelect = idx === -1 || idx == undefined;

  // Clear any manual errors
  error.value = [];

  let newModelValues;
  if (valueOwner.value && modelObjectProp.value) {
    const items: Array<SelectedModelType> = (valueOwner.value[
      modelObjectProp.value.name
    ] ??= []);
    // When bound by valueOwner, mutate the existing collection

    newModelValues = items;
  } else {
    newModelValues = internalModelValue.value || [];
    newModelValues = [...newModelValues];
  }
  if (isSelect) {
    newModelValues.push(value);
  } else {
    if (idx !== -1) {
      newModelValues.splice(idx, 1);
    }
  }

  emit(
    "update:modelValue",
    primaryBindKind.value == "key" ? internalKeyValue.value : newModelValues
  );
  emit("update:objectValues", newModelValues);
  emit("update:keyValue", internalKeyValue.value);
  pendingSelection.value = value ? listItems.value.indexOf(value) : 0;
}

/** When a key is pressed on the top level input */
function onInputKey(event: KeyboardEvent) {
  if (!isInteractive.value) return;

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
  if (!isInteractive.value) return;

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

const listCaller = new ModelApiClient<SelectedModelType>(modelObjectMeta.value)
  .$useSimultaneousRequestCaching()
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
    listCaller.useResponseCaching(c === true ? {} : c);
  },
  { immediate: true }
);

watch(pendingSelection, async () => {
  await nextTick();
  await nextTick();
  var listDiv = listRef.value?.$el as HTMLElement;
  var selectedItem = listDiv?.querySelector(".pending-selection");
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
