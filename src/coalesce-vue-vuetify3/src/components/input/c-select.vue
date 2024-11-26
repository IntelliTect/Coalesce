<template>
  <v-input
    class="c-select"
    :class="{
      'c-select--is-menu-active': menuOpen,
      'c-select--multiple': effectiveMultiple,
    }"
    :focused="focused"
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :modelValue="effectiveMultiple ? internalModelValue : internalModelValue[0]"
    :disabled="isDisabled"
    :readonly="isReadonly"
    #default="{ isValid }"
  >
    <v-field
      :error="isValid.value === false"
      append-inner-icon="$dropdown"
      v-bind="fieldAttrs"
      :clearable="isInteractive && isClearable"
      :active="!!selectedKeysSet.size || focused || !!placeholder"
      :dirty="!!selectedKeysSet.size"
      :focused="focused"
      @click:clear.stop.prevent="onInput(null, true)"
      @keydown="onInputKey($event)"
    >
      <div class="v-field__input">
        <span
          class="v-autocomplete__selection"
          v-for="item in internalModelValue"
          :key="item[modelObjectMeta.keyProp.name]"
        >
          <slot name="selected-item" :item="item" :search="search">
            <slot name="item" :item="item" :search="search">
              <v-chip
                v-if="effectiveMultiple"
                size="small"
                :closable="canDeselect"
                @click:close="onInput(item)"
              >
                {{ itemTitle(item) }}
              </v-chip>
              <span v-else class="v-select__selection-text">
                {{ itemTitle(item) }}
              </span>
            </slot>
          </slot>
        </span>

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
          :placeholder="selectedKeysSet.size ? undefined : placeholder"
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
      location="bottom"
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
        <v-list
          class="py-0"
          max-height="302"
          ref="listRef"
          density="compact"
          :aria-multiselectable="effectiveMultiple"
          role="listbox"
        >
          <v-list-item
            v-if="createItemLabel"
            class="c-select__create-item"
            @click="createItem"
            :loading="createItemLoading"
          >
            <template #prepend>
              <v-progress-circular
                size="20"
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
            :key="item.key"
            @click="onInput(item.model)"
            :value="i"
            :class="{ 'pending-selection': pendingSelection == i }"
            :active="item.selected"
            role="option"
            :aria-selected="item.selected"
          >
            <template #prepend v-if="effectiveMultiple">
              <v-checkbox-btn tabindex="-1" :modelValue="item.selected" />
            </template>
            <v-list-item-title>
              <slot
                name="list-item"
                :item="item.model"
                :search="search"
                :selected="item.selected"
              >
                <slot name="item" :item="item.model" :search="search">
                  {{ itemTitle(item.model) }}
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
      // flex-wrap: nowrap;
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

.c-select__menu-content {
  .v-list-item.pending-selection {
    &::after {
      opacity: calc(0.15 * var(--v-theme-overlay-multiplier));
    }
    &:not(.v-list-item--active) > .v-list-item__overlay {
      opacity: calc(0.05 * var(--v-theme-overlay-multiplier));
    }
  }
}
.c-select__create-item {
  .v-list-item__prepend {
    width: 40px;
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
    ForeignKeyProperty | ModelReferenceNavigationProperty | ModelValue | (ModelCollectionValue & {manyToMany: never})
  > = any,
  TMultiple extends boolean = false"
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
  modelDisplay,
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
  ? // `for="TypeName"`
    TMultiple extends true
    ? Array<ModelTypeLookup[TFor]>
    : ModelTypeLookup[TFor]
  : TFor extends ModelReferenceNavigationProperty | ModelValue
  ? TFor["typeDef"]["name"] extends keyof ModelTypeLookup
    ? ModelTypeLookup[TFor["typeDef"]["name"]]
    : any
  : TFor extends ForeignKeyProperty
  ? TFor["principalType"]["name"] extends keyof ModelTypeLookup
    ? ModelTypeLookup[TFor["principalType"]["name"]]
    : any
  : TModel extends ApiStateTypeWithArgs<any, any, infer TArgsObj, any>
  ? TFor extends keyof TArgsObj
    ? TMultiple extends true
      ? TArgsObj[TFor] extends Array<any> | null | undefined
        ? TArgsObj[TFor]
        : never // Ban use of `multiple` prop when binding to something we know isn't an array
      : TArgsObj[TFor]
    : any
  : TModel extends Model
  ? TFor extends PropNames<TModel["$metadata"]>
    ? TModel["$metadata"]["props"][TFor] extends
        | ModelReferenceNavigationProperty
        | ModelValue
      ? TModel["$metadata"]["props"][TFor]["typeDef"]["name"] extends keyof ModelTypeLookup
        ? ModelTypeLookup[TModel["$metadata"]["props"][TFor]["typeDef"]["name"]]
        : any
      : TModel["$metadata"]["props"][TFor] extends ForeignKeyProperty
      ? TModel["$metadata"]["props"][TFor]["principalType"]["name"] extends keyof ModelTypeLookup
        ? ModelTypeLookup[TModel["$metadata"]["props"][TFor]["principalType"]["name"]]
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

type SelectedModelTypeSingle = SelectedModelType extends Array<
  infer U extends Model<ModelType>
>
  ? U
  : SelectedModelType;

type SelectedPkTypeSingle = FindPk<SelectedModelTypeSingle> extends EnumValue
  ? FindPk<SelectedModelTypeSingle>["typeDef"]["name"] extends keyof EnumTypeLookup
    ? EnumTypeLookup[FindPk<SelectedModelTypeSingle>["typeDef"]["name"]]
    : any
  : TypeDiscriminatorToType<FindPk<SelectedModelTypeSingle>["type"]>;

type SelectedPkType = TMultiple extends true
  ? SelectedPkTypeSingle[]
  : SelectedPkTypeSingle;

type PrimaryBindType = TFor extends ForeignKeyProperty
  ? SelectedPkType
  : TModel extends Model
  ? TFor extends PropNames<TModel["$metadata"]>
    ? TModel["$metadata"]["props"][TFor] extends ForeignKeyProperty
      ? SelectedPkType
      : SelectedModelType
    : SelectedModelType
  : SelectedModelType;

defineSlots<{
  ["item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
  }): any;
  ["selected-item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
  }): any;
  ["list-item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
    selected: boolean;
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

    multiple?: TMultiple & boolean; // `& boolean`: https://github.com/vuejs/core/issues/9877
    canDeselect?: boolean;

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
    keyValue?: SelectedPkType | null;
    objectValue?: SelectedModelType | null;
    modelValue?: PrimaryBindType | null;

    /** Response caching configuration for the `/get` and `/list` API calls made by the component.
     * See https://intellitect.github.io/Coalesce/stacks/vue/layers/api-clients.html#response-caching. */
    cache?: ResponseCachingConfiguration | boolean;

    rules?: Array<TypedValidationRule<SelectedPkType>>;

    itemTitle?: (item: SelectedModelTypeSingle) => string | null;
    create?: {
      getLabel: (
        search: string,
        items: SelectedModelTypeSingle[]
      ) => string | false;
      getItem: (
        search: string,
        label: string
      ) => Promise<SelectedModelTypeSingle>;
    };
  }>(),
  {
    openOnClear: true,
    canDeselect: true,
    clearable: undefined,
    multiple: undefined,
    itemTitle: modelDisplay,
  }
);

const emit = defineEmits<{
  "update:keyValue": [value: SelectedPkType | null];
  "update:objectValue": [value: SelectedModelType | null];
  "update:modelValue": [value: PrimaryBindType | null];
  /** Fired when an item is selected or deselected in `multiple` mode. */
  selectionChanged: [values: SelectedModelTypeSingle[], selected: boolean];
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
const focused = ref(false);
const menuOpen = ref(false);
const menuOpenForced = ref(false);
const searchChanged = ref(new Date());
const mainValue = ref("");
const createItemLoading = ref(false);
const createItemError = ref("" as string | null);
const pendingSelection = ref(0);

/** The models representing the current selected item(s)
 * in the case that only the PK was provided to the component.
 */
const internallyFetchedModels = new Map<
  any,
  WeakRef<SelectedModelTypeSingle>
>();

function toArray<T>(x: T | T[] | null | undefined) {
  return Array.isArray(x) ? x : x == null ? [] : [x];
}

/** The effective clearability state of the dropdown. */
const isClearable = computed((): boolean => {
  if (typeof props.clearable == "boolean")
    // If explicitly given a value, use that value.
    return props.clearable;

  if (effectiveMultiple.value) {
    return true;
  }

  // Check to see if the foreign key is nullable (i.e. doesn't have a 'required' rule).
  if (modelKeyProp.value) {
    return !modelKeyProp.value.rules?.required;
  }

  if (valueMeta.value && "rules" in valueMeta.value) {
    return !valueMeta.value.rules?.required;
  }

  // Only default true when we're bound to a specific prop/param on some parent owner.
  // This preserves longstanding behavior where a <c-select for=Person /> is non-clearable by default.
  if (valueOwner.value) return true;

  return false;
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
  ():
    | ModelReferenceNavigationProperty
    | ModelValue
    | ModelCollectionValue
    | null => {
    const meta = valueMeta.value!;
    if (meta.role == "foreignKey" && "navigationProp" in meta) {
      return meta.navigationProp || null;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta;
    }
    if (meta.role == "value") {
      if (meta.type == "model") {
        return meta;
      }
      if (meta.type == "collection" && meta.itemType.type == "model") {
        return meta as ModelCollectionValue;
      }
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
  if (valueMeta.value.type == "model" || valueMeta.value.type == "collection") {
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
  } else if (meta.type == "collection" && meta.itemType.type == "model") {
    return meta.itemType.typeDef;
  } else {
    throw `Value ${meta.name} must be a foreignKey or model type to use c-select.`;
  }
});

/** The effective object (whose type is described by `modelObjectMeta`) that has been provided to the component. */
const internalModelValue = computed((): SelectedModelTypeSingle[] => {
  if (props.objectValue) {
    return toArray(props.objectValue);
  }
  if (
    valueOwner.value &&
    modelObjectProp.value &&
    valueOwner.value[modelObjectProp.value.name]
  ) {
    return toArray(valueOwner.value[modelObjectProp.value.name]);
  }

  if (props.modelValue && primaryBindKind.value == "model") {
    return toArray(props.modelValue);
  }

  let ret = [];
  let needsLoad = [];
  for (const key of internalKeyValue.value) {
    // See if we already have a model that we're using to represent a key-only binding.
    // Storing this object prevents it from flipping between different instances
    // obtained from either getCaller or listCaller,
    // which causes vuetify to reset its search when the object passed to v-select's `modelValue` prop changes.
    const keyFetchedModel = internallyFetchedModels.get(key)?.deref();
    if (keyFetchedModel) {
      ret.push(keyFetchedModel);
      continue;
    }

    // All we have is the PK. First, check if it is already in our item array.
    // If so, capture it. If not, request the object from the server.
    const item = items.value.filter(
      (i) => key === (i as any)[modelObjectMeta.value.keyProp.name]
    )[0];
    if (item) {
      // eslint-disable-next-line vue/no-side-effects-in-computed-properties
      internallyFetchedModels.set(key, new WeakRef(item));
      ret.push(item);
      continue;
    }

    // See if we obtained the item via getCaller.
    const singleItem = getCaller.result?.find(
      (x) => key === x[modelObjectMeta.value.keyProp.name]
    );
    if (singleItem) {
      // eslint-disable-next-line vue/no-side-effects-in-computed-properties
      internallyFetchedModels.set(key, new WeakRef(singleItem));
      ret.push(singleItem);
      continue;
    }

    needsLoad.push(key);
  }

  if (
    !listCaller.isLoading &&
    !getCaller.isLoading &&
    needsLoad.some((needed) => !getCaller.args.ids.includes(needed))
  ) {
    // Only request the item if the list isn't currently loading,
    // since the item may end up coming back from a pending list call.
    // Also only load if any of the needed keys are missing from the last requested server call.
    // (this prevents an infinite loop of invokes if the call to the server fails.)
    // eslint-disable-next-line vue/no-side-effects-in-computed-properties
    getCaller.args.ids = needsLoad;
    getCaller.invokeWithArgs();
  }

  return ret;
});

/** The effective key (whose type is described by `modelObjectMeta`) that has been provided to the component. */
const internalKeyValue = computed((): SelectedPkType[] => {
  let value: any;
  if (props.keyValue) {
    value = toArray(props.keyValue);
  } else if (valueOwner.value && modelKeyProp.value) {
    value = toArray(valueOwner.value[modelKeyProp.value.name]);
  } else if (props.modelValue && primaryBindKind.value == "key") {
    value = toArray(props.modelValue);
  } else {
    value = [];
  }

  // Parse the values in case we were given a string instead of a number, or something like that, via the `keyValue` prop.
  // This prevents `internalModelValue` from getting confused and infinitely calling the `getCaller`.
  return value.map((v: any) =>
    mapValueToModel(v, modelObjectMeta.value.keyProp)
  );
});

const selectedKeysSet = computed(
  () =>
    new Set([
      ...internalKeyValue.value,
      ...internalModelValue.value.map(
        (x) => x[modelObjectMeta.value.keyProp.name]
      ),
    ])
);

/** The effective set of validation rules to pass to the v-select. */
const effectiveRules = computed((): TypedValidationRule<SelectedPkType>[] => {
  // If we were explicitly given rules, use those.
  if (props.rules) return props.rules;

  if (valueOwner.value instanceof ViewModel && modelKeyProp.value) {
    // We're binding to a ViewModel instance.
    // Grab the rules from the instance, because it may contain custom rules
    // and/or other rule changes that have been customized in userland beyond what the metadata provides.

    // Validate using the key, not the navigation. The FK
    // is the actual scalar value that gets sent to the server,
    // and is the prop that we generate things like `required` onto.
    // We need to translate the rule functions to pass the selected FK instead
    // of the selected model object.
    return (
      valueOwner.value
        .$getRules(modelKeyProp.value)
        ?.map(
          (rule) => () =>
            effectiveMultiple.value
              ? rule(internalKeyValue.value)
              : rule(internalKeyValue.value[0])
        ) ?? []
    );
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
  return (listCaller?.result || []) as SelectedModelTypeSingle[];
});

const listItems = computed(() => {
  const pkName = modelObjectMeta.value.keyProp.name;
  return items.value.map((item) => ({
    model: item,
    key: item[pkName],
    selected: selectedKeysSet.value.has(item[pkName]),
  }));
});

const createItemLabel = computed(() => {
  if (!props.create || !search.value) return null;

  const result = props.create.getLabel(search.value, items.value);
  if (result) {
    return result;
  }
  return null;
});

const effectiveMultiple = computed(() => {
  let multiple: boolean = props.multiple ?? false;
  if (valueMeta.value?.type == "collection") {
    if ("manyToMany" in valueMeta.value) {
      throw new Error(
        `c-select cannot be used with the many-to-many value '${valueMeta.value.name}'. Use c-select-many-to-many instead.`
      );
    }
    multiple = true;
  } else if (valueOwner.value && valueMeta.value && multiple) {
    throw new Error(
      `The 'multiple' prop cannot be used with the non-collection value '${valueMeta.value.name}'.`
    );
  }

  return multiple;
});

function onInput(value: SelectedModelTypeSingle | null, dontFocus = false) {
  value = value ?? null;

  if (value === null && !props.canDeselect) {
    return;
  }

  const key = value ? (value as any)[modelObjectMeta.value.keyProp.name] : null;
  let newKey, newObjectValue: any;

  if (effectiveMultiple.value) {
    if (value == null) {
      newObjectValue = [];
      newKey = [];
      emit("selectionChanged", [...internalModelValue.value], false);
    } else {
      const selectedKeys = [...selectedKeysSet.value];
      const selectedModels = [...internalModelValue.value];
      if (key != null) {
        const idx = selectedKeys.indexOf(key);
        if (idx === -1) {
          selectedKeys.push(key);
          selectedModels.push(value);
          internallyFetchedModels.set(key, new WeakRef(value));
          emit("selectionChanged", [value], true);
        } else {
          if (!props.canDeselect) return;
          selectedKeys.splice(idx, 1);
          const modelIdx = selectedModels.indexOf(value);
          if (modelIdx !== -1) {
            selectedModels.splice(idx, 1);
          }
          emit("selectionChanged", [value], false);
        }
      } else {
        // Key may be null if the item came from `props.create` and isn't saved yet.
        const idx = selectedModels.indexOf(value);
        if (idx === -1) {
          selectedModels.push(value);
          emit("selectionChanged", [value], true);
        } else {
          if (!props.canDeselect) return;
          selectedModels.splice(idx, 1);
          emit("selectionChanged", [value], false);
        }
      }

      newObjectValue = selectedModels;
      newKey = selectedKeys;
    }
  } else {
    if (value == null && !isClearable.value) {
      return;
    }
    newObjectValue = value;
    newKey = key;
    if (value) {
      internallyFetchedModels.set(key, new WeakRef(value));
    }
  }

  if (valueOwner.value) {
    if (modelKeyProp.value) {
      valueOwner.value[modelKeyProp.value.name] = newKey;
    }
    if (modelObjectProp.value) {
      valueOwner.value[modelObjectProp.value.name] = newObjectValue;
    }
  }

  emit(
    "update:modelValue",
    primaryBindKind.value == "key" ? newKey : newObjectValue
  );
  emit("update:objectValue", newObjectValue);
  emit("update:keyValue", newKey);

  pendingSelection.value = value ? items.value.indexOf(value) : 0;

  // When the input value is cleared, re-focus the dropdown
  // to allow the user to enter new search input.
  // Without this, pressing backspace to delete the current value
  // will cause the search field to lose focus and further keyboard input will do nothing.
  if (!dontFocus && !effectiveMultiple.value) {
    if (!value) {
      openMenu();
    } else {
      closeMenu(true);
    }
  }
}

/** When a key is pressed on the top level input */
function onInputKey(event: KeyboardEvent) {
  if (!isInteractive.value) return;

  switch (event.key.toLowerCase()) {
    case "delete":
    case "backspace":
      if (!menuOpen.value) {
        if (effectiveMultiple.value) {
          // Delete only the last item when deleting items with multi-select
          const lastItem = internalModelValue.value.at(-1);
          if (lastItem) {
            onInput(lastItem, true);
          }
        } else {
          onInput(null, true);
        }
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
  var item = items.value[pendingSelection.value];
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
    // cap waiting
    start + 500 > performance.now() &&
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

if (!valueMeta.value) {
  throw "c-select requires value metadata. Specify it with the 'for' prop'";
}

/**
 * A caller that will be used to resolve the full object when the only thing
 * that has been provided to c-select is a primary key value.
 */
const getCaller = new ModelApiClient<SelectedModelTypeSingle>(
  modelObjectMeta.value
)
  .$useSimultaneousRequestCaching()
  .$makeCaller(
    "list",
    function () {
      throw "expected calls to be made with invokeWithArgs";
    },
    () => ({ ids: [] as any[] }),
    (c, args) => {
      if (!args.ids.length) return;

      if (args.ids.length == 1) {
        return c.get(args.ids[0], props.params).then((res) => ({
          ...res,
          data: {
            ...res.data,
            // Convert ItemResult to ListResult
            list: res.data.object ? [res.data.object] : [],
          },
        }));
      } else if (
        args.ids.length > 1 &&
        modelObjectMeta.value.keyProp.type == "number"
      ) {
        return c.list({
          ...props.params,
          filter: {
            ...props.params?.filter,
            [modelObjectMeta.value.keyProp.name]: args.ids.join(","),
          },
          pageSize: args.ids.length,
          noCount: true,
        });
      } else {
        // Multiple values that we can't query with a CSV-filtered list call.
        const promises = args.ids.map((id) => c.get(id, props.params));
        return Promise.allSettled(promises).then((res) => {
          const items = res
            .filter((res) => res.status == "fulfilled")
            .map((res) => res.value);
          return {
            ...items[0],
            data: {
              wasSuccessful: items.length > 0,
              list: items
                .filter((r) => r.data.object)
                .map((r) => r.data.object!),
            },
          };
        });
      }
    }
  )
  .setConcurrency("debounce");

const listCaller = new ModelApiClient<SelectedModelTypeSingle>(
  modelObjectMeta.value
)
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
  if (selectedKeysSet.value.size) {
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
