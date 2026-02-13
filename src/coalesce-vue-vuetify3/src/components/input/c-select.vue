<template>
  <v-text-field
    ref="rootRef"
    v-model="mainValue"
    v-intersect="onIntersect"
    class="c-select"
    role="combobox"
    :class="{
      'c-select--is-menu-active': menuOpen,
      'c-select--multiple': effectiveMultiple,
    }"
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :validationValue="
      effectiveMultiple ? internalModelValue : internalModelValue[0]
    "
    :dirty="!!selectedKeysSet.size || menuOpen"
    :disabled="isDisabled"
    :readonly="isReadonly"
    :autofocus="autofocus"
    :clearable="isInteractive && isClearable"
    :placeholder="selectedKeysSet.size ? undefined : placeholder"
    append-inner-icon="$dropdown"
    @click:clear.stop.prevent="onInput(null, true)"
    @keydown="onInputKey($event)"
    @click:control.stop.prevent="openMenu()"
  >
    <template v-for="(_, slot) of passthroughSlots" #[slot]="scope">
      <slot :name="slot" v-bind="scope" />
    </template>

    <template #default>
      <!-- Selected items display -->
      <span
        v-for="(item, index) in internalModelValue"
        :key="item[modelObjectMeta.keyProp.name]"
        class="v-select__selection"
        :class="{
          'v-select__selection--selected':
            index === selectionIndex && effectiveMultiple,
        }"
      >
        <slot
          name="selected-item"
          :item="item"
          :search="search"
          :index
          :remove="() => onInput(item)"
        >
          <slot name="item" :item="item" :search="search">
            <v-chip v-if="effectiveMultiple" size="small">
              <template #append>
                <button
                  v-if="!!canDeselect && isInteractive"
                  class="v-chip__close"
                  type="button"
                  data-testid="close-chip"
                  aria-label="Remove Item"
                  tabindex="-1"
                  @click.stop.prevent="onInput(item)"
                >
                  <VIcon icon="$delete" size="x-small" />
                </button>
              </template>
              {{ itemTitle(item) }}
            </v-chip>
            <span v-else class="v-select__selection-text">
              {{ itemTitle(item) }}
            </span>
          </slot>
        </slot>
      </span>

      <v-menu
        :modelValue="menuOpen"
        activator="parent"
        :close-on-content-click="false"
        :open-on-click="false"
        contentClass="c-select__menu-content"
        :disabled="isInteractive"
        origin="top"
        location="bottom"
        v-bind="menuProps"
        @update:model-value="!$event ? closeMenu() : openMenu()"
      >
        <v-sheet
          ref="menuContentRef"
          @keydown.capture.down.stop.prevent="
            pendingSelection = Math.min(
              totalSelectableItems - 1,
              pendingSelection + 1,
            )
          "
          @keydown.capture.up.stop.prevent="
            pendingSelection = Math.max(
              createItemIndex == -1 ? -1 : 0,
              pendingSelection - 1,
            )
          "
          @keydown.capture.enter.stop.prevent="confirmPendingSelection"
          @keydown.capture.tab.stop.prevent="confirmPendingSelection"
          @blur.capture="onMenuContentBlur"
        >
          <v-text-field
            ref="searchRef"
            v-model="search"
            v-intersect="onSearchIntersect"
            hide-details="auto"
            prepend-inner-icon="fa fa-search"
            :loading="listCaller.isLoading"
            :error-messages="
              listCaller.wasSuccessful == false
                ? (listCaller.message ?? undefined)
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
            class="py-0 d-flex flex-column"
            max-height="302"
            density="compact"
            :aria-multiselectable="effectiveMultiple"
            role="listbox"
            v-bind="listProps"
          >
            <v-list-item
              v-if="createItemLabel"
              class="c-select__create-item"
              :class="{
                'c-select__create-item--end': props.create?.position === 'end',
                'pending-selection': pendingSelection === createItemIndex,
              }"
              :loading="createItemLoading"
              @click="createItem"
            >
              <template #prepend>
                <v-progress-circular
                  v-if="createItemLoading"
                  size="20"
                  indeterminate
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

            <template v-for="(item, i) in listItems" :key="item.key">
              <slot
                name="list-item-outer"
                :item="item.model"
                :search="search"
                :selected="item.selected"
                :props="{
                  value: item.key,
                  class: { 'pending-selection': pendingSelection === i },
                  active: item.selected,
                  role: 'option',
                  'aria-selected': item.selected,
                  onClick: () => onInput(item.model),
                }"
                :select="
                  (value: boolean) => {
                    if (value !== item.selected) onInput(item.model);
                  }
                "
              >
                <v-list-item
                  v-memo="[
                    pendingSelection === i,
                    item.selected,
                    hasCustomItemSlots ? search : false,
                  ]"
                  :value="item.key"
                  :class="{
                    'pending-selection': pendingSelection === i,
                  }"
                  :active="item.selected"
                  role="option"
                  :aria-selected="item.selected"
                  @click="onInput(item.model)"
                >
                  <template v-if="effectiveMultiple" #prepend>
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
              </slot>
            </template>

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
              Max {{ listCaller.pageSize }} items retrieved. Refine your search
              to view more.
            </v-list-item>
          </v-list>
        </v-sheet>
      </v-menu>
    </template>
  </v-text-field>
</template>

<script lang="ts">
// These types are declared outside the `setup` script so that vue-tsc doesn't have to inline
// every single type declaration, which spans thousands of lines and ultimately exceeds
// the limit on what tsc is even capable of emitting

type CSelectModelSpec = Model | AnyArgCaller | undefined;

type CSelectForSpec<TModel extends CSelectModelSpec> = ForSpec<
  TModel,
  | ForeignKeyProperty
  | ModelReferenceNavigationProperty
  | ModelValue
  | ModelCollectionValue
  | ModelType
>;

type _SelectedModelType<
  TModel extends CSelectModelSpec,
  TFor extends CSelectForSpec<TModel>,
  TMultiple extends boolean,
> = TFor extends string & keyof ModelTypeLookup
  ? // `for="TypeName"`
    TMultiple extends true
    ? Array<ModelTypeLookup[TFor]>
    : ModelTypeLookup[TFor]
  : TFor extends
        | ModelReferenceNavigationProperty
        | ModelValue
        | ForeignKeyProperty
    ? ValueOrFkToModelType<TFor>
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
          ? ValueOrFkToModelType<TModel["$metadata"]["props"][TFor]>
          : any
        : Model<ModelType>;

type _Single<T> = T extends Array<infer U> ? U : T;

type ExtractValuesOfType<T, U> = {
  [K in keyof T]: T[K] extends U ? T[K] : never;
}[keyof T];

type FindPk<TModel extends Model> = ExtractValuesOfType<
  TModel["$metadata"]["props"],
  PrimaryKeyProperty
>;

type ModelToPkType<TModel extends Model> = MetadataToModelType<FindPk<TModel>>;

type InheritedProps = Omit<
  VTextField["$props"],
  | InheritExcludePropNames
  | "readonly"
  | "disabled"
  | "hint"
  | "direction"
  | "rules"
  | "clearable"
  | "dirty"
  | "active"
  | "modelValue"
>;

type _InheritedSlots = Omit<VTextField["$slots"], "default">;
// This extra mapped type prevents vue-tsc from getting confused
// and failing to emit any types at all. When it encountered the mapped type,
// it doesn't know how to handle it and so leaves it un-transformed.
type InheritedSlots = {
  [Property in keyof _InheritedSlots]?: _InheritedSlots[Property];
};
</script>

<script
  lang="ts"
  setup
  generic="
    TModel extends CSelectModelSpec,
    TFor extends CSelectForSpec<TModel> = any,
    TMultiple extends boolean = false
  "
>
import {
  ref,
  computed,
  nextTick,
  watch,
  onBeforeUnmount,
  useTemplateRef,
} from "vue";
import {
  useMetadataProps,
  ForSpec,
  useCustomInput,
  InheritExcludePropNames,
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
  PrimaryKeyProperty,
  PropNames,
  ApiStateTypeWithArgs,
  ModelCollectionValue,
  modelDisplay,
  MetadataToModelType,
  ValueOrFkToModelType,
  ViewModelFactory,
  ViewModelCollection,
  ModelCollectionNavigationProperty,
} from "coalesce-vue";
import { VList, VListItem, VMenu, VTextField } from "vuetify/components";
import { Intersect } from "vuetify/directives";

/* DEV NOTES:
 * - Experimented with moving this back to be based on v-autocomplete on 2024 Nov 22. Found issues:
 *    - When the menu opens, a janky scroll to the selected item occurs,
 *      with pre-scroll content flashing into view for a few frames.
 *    - Scroll of the list doesn't reset when searching, no easy way to do this manually
 *    - Arrow key navigation relies on tab focus, which fundamentally doesn't work with virtual scrolling.
 *      Specifically, hitting "UpArrow" when the top item is selected wraps around to the bottom of the list,
 *      but the bottom of the rendered list is really only somewhere in the middle of the true list.
 *    - Menu isn't animated when it opens for some reason
 *    - Have to use direct DOM manipulation to intercept focus events on the search input inside the v-autocomplete.
 *      Very hard to get this to behave consistently.
 */

defineOptions({
  name: "c-select",
  directives: { Intersect },
  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

type SelectedModelType = _SelectedModelType<TModel, TFor, TMultiple>;

type SelectedModelTypeSingle = _Single<SelectedModelType>;

type SelectedPkTypeSingle = ModelToPkType<SelectedModelTypeSingle>;

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

type SlotTypes = {
  ["item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
  }): any;
  ["selected-item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
    index: number;
    /** Remove/unselect the item. Only applicable for multiselect/multiple mode. */
    remove: () => void;
  }): any;
  ["list-item"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
    selected: boolean;
  }): any;
  ["list-item-outer"]?(props: {
    item: SelectedModelTypeSingle;
    search: string | null;
    selected: boolean;
    /** Props to bind to v-list-item for full control over the list item rendering.
     * Includes value, class, active, and click handler. */
    props: Partial<VListItem["$props"]> & {
      onClick: (e: MouseEvent) => void;
      class: Record<string, boolean>;
      role: string;
      "aria-selected": boolean;
    };
    /** Function to toggle selection of the item. */
    select: (value: boolean) => void;
  }): any;
} & InheritedSlots;
const slots = defineSlots<SlotTypes>();

const props = withDefaults(
  defineProps<
    {
      /** An object owning the value to be edited that is specified by the `for` prop. */
      model?: TModel | null;

      /** A metadata specifier for the value being bound. One of:
       * * A string with the name of type to select. E.g. `"Person"`.
       * * A string with the name of the value belonging to `model`. E.g. `"supervisor"`.
       * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
       */
      for: TFor;

      multiple?: TMultiple & boolean; // `& boolean`: https://github.com/vuejs/core/issues/9877
      canDeselect?: boolean | null;

      readonly?: boolean | null;
      disabled?: boolean | null;
      autofocus?: boolean | null;
      clearable?: boolean | null;
      placeholder?: string | null;
      preselectFirst?: boolean | null;
      preselectSingle?: boolean | null;
      openOnClear?: boolean | null;
      reloadOnOpen?: boolean | null;
      params?: Partial<ListParameters>;

      // DONT use defineModel for these. We don't want to capture local state if the parent isn't binding it
      // since we have 4 different binding sources in this component, we'll get stuck on the values of the ones that aren't used.
      keyValue?: SelectedPkType | null;
      objectValue?: SelectedModelType | null;
      modelValue?: PrimaryBindType | null;

      /** Response caching configuration for the `/get` and `/list` API calls made by the component.
       * See https://coalesce.intellitect.com/stacks/vue/layers/api-clients.html#response-caching. */
      cache?: ResponseCachingConfiguration | boolean;

      rules?: Array<TypedValidationRule<SelectedPkType>>;

      itemTitle?: (item: SelectedModelTypeSingle) => string | null;

      /** Props to pass to the underlying v-menu component */
      menuProps?: VMenu["$props"] & { [s: string]: any };

      /** Props to pass to the underlying v-list component */
      listProps?: VList["$props"] & { [s: string]: any };

      create?: {
        getLabel: (
          search: string,
          items: SelectedModelTypeSingle[],
        ) => string | false | null | undefined;
        getItem: (
          search: string,
          label: string,
        ) => Promise<SelectedModelTypeSingle>;
        /** Position of the create item in the list. Defaults to 'start'. */
        position?: "start" | "end";
      };
    } & /* @vue-ignore */ InheritedProps
  >(),
  {
    openOnClear: false,
    canDeselect: true,
    clearable: undefined,
    multiple: undefined,
    itemTitle: modelDisplay,
  },
);

const emit = defineEmits<{
  "update:keyValue": [value: SelectedPkType | null];
  "update:objectValue": [value: SelectedModelType | null];
  "update:modelValue": [value: PrimaryBindType | null];
  /** Fired when an item is selected or deselected in `multiple` mode. */
  selectionChanged: [values: SelectedModelTypeSingle[], selected: boolean];
}>();

const passthroughSlots = computed(() => {
  const ret = { ...slots };
  //@ts-expect-error Runtime safety - already forbidden by types.
  delete ret.default;
  return ret;
});

const hasCustomItemSlots = computed(
  () => !!slots["list-item"] || !!slots["item"],
);

const rootRef = useTemplateRef("rootRef");
const menuContentRef = useTemplateRef("menuContentRef");
const searchRef = useTemplateRef("searchRef");

// For VTextField, we need to access the internal input element
const mainInputRef = computed(() => {
  return rootRef.value?.$el?.querySelector("input") as
    | HTMLInputElement
    | undefined;
});

watch(mainInputRef, (el) => {
  el?.addEventListener("mousedown", (e) => {
    // Intercept direct clicks on the input to short circuit `focused`
    // and v-menu's activator handler, which introduce some latency before the menu opens
    // if we allow the menu opening to be handled that way.
    // Mousedown is needed to prevent `focused` from happening.
    e.stopPropagation();
    e.preventDefault();
    openMenu();
  });

  el?.addEventListener("click", (e) => {
    // Prevent v-text-field's click handler from running, which focuses mainInputRef.
    // We always want this to open our menu, rather than momentarily focusing mainInputRef
    // before we then re-focus searchRef.
    e.stopPropagation();
    e.preventDefault();
    openMenu();
  });
});

const { isDisabled, isReadonly, isInteractive } = useCustomInput(props);

const { inputBindAttrs, valueMeta, valueOwner } = useMetadataProps(props, (v) =>
  // Use the navigation metadata (if exists) to drive the logic in here,
  // as it will be a better source to pull label, hint, etc. from.
  v.role == "foreignKey" && "navigationProp" in v ? v.navigationProp! : v,
);

const search = ref(null as string | null);
const menuOpen = ref(false);
const searchChanged = ref(new Date());
const mainValue = ref("");
const createItemLoading = ref(false);
const createItemError = ref("" as string | null);
const pendingSelection = ref(0);
const selectionIndex = ref(-1);
const pendingSearchSelect = ref(false);

/** The models representing the current selected item(s)
 * in the case that only the PK was provided to the component.
 * Uses normalized keys to handle Date objects.
 */
const internallyFetchedModels = new Map<
  any,
  WeakRef<SelectedModelTypeSingle>
>();

function toArray<T>(x: T | T[] | null | undefined) {
  return Array.isArray(x) ? x : x == null ? [] : [x];
}

/** Normalizes a key value for use in equality comparisons.
 * Converts Date objects to ISO strings to enable proper equality checks.
 */
function normalizeKey(key: any): any {
  if (key instanceof Date) return key.toISOString();
  return key;
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
const modelKeyProp = computed(
  (): ForeignKeyProperty | PrimaryKeyProperty | null => {
    const meta = valueMeta.value!;
    if (meta.role == "foreignKey" && "principalType" in meta) {
      return meta;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta.foreignKey;
    }
    return null;
  },
);

/** The property on `valueOwner` which holds the model object being selected for, or `null` if there is no such property. */
const modelObjectProp = computed(
  ():
    | ModelReferenceNavigationProperty
    | ModelValue
    | ModelCollectionValue
    | ModelCollectionNavigationProperty
    | null => {
    const meta = valueMeta.value!;
    if (meta.role == "foreignKey" && "navigationProp" in meta) {
      return meta.navigationProp || null;
    }
    if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
      return meta;
    }
    if (meta.type == "model") {
      return meta;
    }
    if (meta.type == "collection" && meta.itemType.type == "model") {
      return meta as ModelCollectionValue | ModelCollectionNavigationProperty;
    }
    return null;
  },
);

/**
 * Whether the metadata provided to `for` and retrieved via `valueMeta` is for a key or an object.
 * Dictates whether v-model will attempt to bind a key or an object.
 */
const primaryBindKind = computed((): "model" | "key" => {
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
const modelObjectMeta = computed((): ModelType => {
  const meta = valueMeta.value!;
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

  const ret = [];
  const needsLoad = [];
  for (const key of internalKeyValue.value) {
    // See if we already have a model that we're using to represent a key-only binding.
    // Storing this object prevents it from flipping between different instances
    // obtained from either getCaller or listCaller,
    // which causes vuetify to reset its search when the object passed to v-select's `modelValue` prop changes.
    const normalizedKey = normalizeKey(key);
    const keyFetchedModel = internallyFetchedModels.get(normalizedKey)?.deref();
    if (keyFetchedModel) {
      ret.push(keyFetchedModel);
      continue;
    }

    // All we have is the PK. First, check if it is already in our item array.
    // If so, capture it. If not, request the object from the server.
    const item = items.value.find(
      (i) =>
        normalizeKey(i[modelObjectMeta.value.keyProp.name]) === normalizedKey,
    );
    if (item) {
      internallyFetchedModels.set(normalizedKey, new WeakRef(item));
      ret.push(item);
      continue;
    }

    // See if we obtained the item via getCaller.
    const singleItem = getCaller.result?.find(
      (x) =>
        normalizeKey(x[modelObjectMeta.value.keyProp.name]) === normalizedKey,
    );
    if (singleItem) {
      internallyFetchedModels.set(normalizedKey, new WeakRef(singleItem));
      ret.push(singleItem);
      continue;
    }

    needsLoad.push(key);
  }

  if (
    !listCaller.isLoading &&
    !getCaller.isLoading &&
    needsLoad.some(
      (needed) =>
        !getCaller.args.ids.some(
          (id) => normalizeKey(id) === normalizeKey(needed),
        ),
    )
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
const internalKeyValue = computed((): SelectedPkTypeSingle[] => {
  let value: SelectedPkTypeSingle[];
  if (props.keyValue) {
    value = toArray(props.keyValue) as SelectedPkTypeSingle[];
  } else if (valueOwner.value && modelKeyProp.value) {
    value = toArray(valueOwner.value[modelKeyProp.value.name]);
  } else if (props.modelValue && primaryBindKind.value == "key") {
    value = toArray(props.modelValue);
  } else {
    value = [];
  }

  // Parse the values in case we were given a string instead of a number, or something like that, via the `keyValue` prop.
  // This prevents `internalModelValue` from getting confused and infinitely calling the `getCaller`.
  return value.map((v) => mapValueToModel(v, modelObjectMeta.value.keyProp));
});

/** A Set of normalized primary keys representing all currently selected items.
 * Keys are normalized via `normalizeKey()` to handle Date objects and ensure proper equality comparisons.
 */
const selectedKeysSet = computed(
  (): Set<any> =>
    new Set([
      ...internalKeyValue.value.map(normalizeKey),
      ...internalModelValue.value.map((x) =>
        normalizeKey(x[modelObjectMeta.value.keyProp.name]),
      ),
    ]),
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
              : rule(internalKeyValue.value[0]),
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

const items = computed((): SelectedModelTypeSingle[] => {
  return listCaller?.result || [];
});

const listItems = computed(() => {
  const pkName = modelObjectMeta.value.keyProp.name;
  return items.value.map((item) => ({
    model: item,
    key: item[pkName],
    selected: selectedKeysSet.value.has(normalizeKey(item[pkName])),
  }));
});

const totalSelectableItems = computed(() => {
  let count = listItems.value.length;
  if (createItemLabel.value) count += 1;
  return count;
});

const createItemIndex = computed(() => {
  if (!createItemLabel.value) return -2;
  return props.create?.position === "end" ? listItems.value.length : -1;
});

const createItemLabel = computed((): string | null => {
  if (!props.create || !search.value) return null;

  const result = props.create.getLabel(search.value, items.value);
  if (result) {
    return result;
  }
  return null;
});

const effectiveMultiple = computed((): boolean => {
  let multiple: boolean = props.multiple ?? false;
  const _valueMeta = valueMeta.value;

  if (_valueMeta?.type == "collection") {
    if ("manyToMany" in _valueMeta) {
      throw new Error(
        `c-select cannot be used with the many-to-many value '${_valueMeta.name}'. Use c-select-many-to-many instead.`,
      );
    }
    if (_valueMeta.role == "collectionNavigation") {
      if (!("inverseNavigation" in _valueMeta)) {
        throw new Error(
          "c-select requires that bound collection navigation properties have a defined inverse property.",
        );
      }
    }
    multiple = true;
  } else if (valueOwner.value && _valueMeta && multiple) {
    throw new Error(
      `The 'multiple' prop cannot be used with the non-collection value '${_valueMeta.name}'.`,
    );
  }

  return multiple;
});

/** 
  Converts the provided single item into a ViewModel instance 
  if we're going to be assigning the item into a ViewModel or ViewModelCollection.

  We do this preemptively so that the object instance
  that we emit from events will be identical to the instance that lands on valueOwner.

  Implements https://github.com/IntelliTect/Coalesce/issues/543
*/
function convertValue<T extends SelectedModelTypeSingle | null>(value: T): T {
  if (!value) return value;

  if (
    !((value as any) instanceof ViewModel) &&
    ((valueOwner.value instanceof ViewModel && modelObjectProp.value) ||
      internalModelValue.value instanceof ViewModelCollection)
  ) {
    return ViewModelFactory.get(
      value.$metadata.name,
      value,
      // Assume the data is clean if it has a PK.
      // This mirrors the behavior of model prop setters on ViewModel instances,
      // which is the code that we're preempting here.
      !!value[value.$metadata.keyProp.name],
    ) as any;
  }

  return value;
}

function selectionChanged(
  values: SelectedModelTypeSingle[],
  selected: boolean,
) {
  const owner = valueOwner.value;
  const prop = modelObjectProp.value;
  if (
    owner &&
    prop &&
    prop.type == "collection" &&
    "inverseNavigation" in prop
  ) {
    for (const item of values) {
      if (selected) {
        // Assign the selected item's parent through the navigation to be `valueOwner`.
        // This will also populate the FK on `item` through viewmodel setters.
        item[prop.inverseNavigation!.name] = owner;
        if ((item as any) instanceof ViewModel) {
          // Force dirty the FK in case the cached item loaded by our list
          // was stale and already has the FK value we're after.
          // This avoids breakage that happens if you deselect an existing selection,
          // save, open the menu, and re-select the item, and save again.
          (item as ViewModel).$setPropDirty(prop.foreignKey.name, true);
        }
      } else {
        // Item unselected. Sever the relationship through the navigation.
        item[prop.foreignKey.name] = null;
        item[prop.inverseNavigation!.name] = null;

        // Ensure the severed item will be discovered by bulk saves
        // after it is imminently removed from its current collection.
        // Note that this won't outright delete `item` since we're not setting `_isRemoved`.
        if (owner instanceof ViewModel) {
          (owner.$removedItems ??= []).push(item);
        }
      }
    }
  }

  emit("selectionChanged", values, selected);
}

function onInput(
  value: SelectedModelTypeSingle | null,
  dontFocus = false,
): void {
  if (!isInteractive.value) return;

  value = value ?? null;

  if (value === null && !props.canDeselect) {
    return;
  }

  const pkName = modelObjectMeta.value.keyProp.name;
  const key = value ? value[pkName] : null;
  let newKey, newObjectValue: any;

  if (effectiveMultiple.value) {
    if (value == null) {
      // Clear button clicked on a multi-select
      newObjectValue = [];
      newKey = [];
      selectionChanged([...internalModelValue.value], false);
    } else {
      // Multi-select selection changed.

      const selectedKeys = [...selectedKeysSet.value];
      const selectedModels = [...internalModelValue.value];

      if (key != null) {
        const normalizedKey = normalizeKey(key);
        const idx = selectedKeys.indexOf(normalizedKey);
        if (idx === -1) {
          const newValue = convertValue(value)!;

          selectedKeys.push(normalizedKey);
          selectedModels.push(newValue);
          internallyFetchedModels.set(normalizedKey, new WeakRef(newValue));
          selectionChanged([newValue], true);
        } else {
          if (!props.canDeselect) return;
          selectedKeys.splice(idx, 1);
          const modelIdx = selectedModels.findIndex(
            (x) => normalizeKey(x[pkName]) === normalizedKey,
          );
          if (modelIdx !== -1) {
            value = selectedModels[modelIdx] ?? value;
            selectedModels.splice(idx, 1);
          }
          selectionChanged([value!], false);
        }
      } else {
        // Key may be null if the item came from `props.create` and isn't saved yet.
        const modelIdx = selectedModels.indexOf(value);
        if (modelIdx === -1) {
          const newValue = convertValue(value);
          selectedModels.push(newValue);
          selectionChanged([newValue], true);
        } else {
          if (!props.canDeselect) return;
          value = selectedModels[modelIdx] ?? value;
          selectedModels.splice(modelIdx, 1);
          selectionChanged([value!], false);
        }
      }

      newObjectValue = selectedModels;
      newKey = selectedKeys;
    }
  } else {
    if (value == null && !isClearable.value) {
      return;
    }
    const newValue = convertValue(value);
    newObjectValue = newValue;
    newKey = key;
    if (newValue && key != null) {
      internallyFetchedModels.set(normalizeKey(key), new WeakRef(newValue));
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
    primaryBindKind.value == "key" ? newKey : newObjectValue,
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
      return;
    } else {
      closeMenu(true);
      return;
    }
  }

  if (value == null && props.openOnClear) {
    openMenu();
  }
}

function onMenuContentBlur(event: FocusEvent): void {
  if (
    !menuContentRef.value?.$el.contains(event.relatedTarget as HTMLElement) &&
    !rootRef.value?.$el.contains(event.target as HTMLElement)
  ) {
    closeMenu(true);
  }
}

/** When a key is pressed on the top level input */
function onInputKey(event: KeyboardEvent): void {
  if (!isInteractive.value) return;
  const key = event.key?.toLowerCase();

  const input = mainInputRef.value;
  const selectionStart = input?.selectionStart;
  const value = internalModelValue.value;
  const length = value.length;

  switch (key) {
    case "delete":
    case "backspace":
      if (!menuOpen.value) {
        if (effectiveMultiple.value) {
          if (length == 1) {
            onInput(value[0], true);
          } else if (selectionIndex.value >= 0) {
            // If we have a selection index, remove that specific item
            const itemToRemove = value[selectionIndex.value];
            if (itemToRemove) {
              onInput(itemToRemove, true);
              // Adjust selection index after removal
              const originalSelectionIndex = selectionIndex.value;
              selectionIndex.value =
                originalSelectionIndex >= length - 1
                  ? length - 2
                  : originalSelectionIndex;
            }
          } else if (key === "backspace") {
            selectionIndex.value = length - 1;
          }
        } else {
          onInput(null, true);
        }
        event.stopPropagation();
        event.preventDefault();
      }
      return;
    case "arrowleft":
    case "left":
      if (!effectiveMultiple.value || menuOpen.value) return;

      if (
        selectionIndex.value < 0 &&
        selectionStart != null &&
        selectionStart > 0
      )
        return;

      const prev =
        selectionIndex.value > -1 ? selectionIndex.value - 1 : length - 1;

      if (internalModelValue.value[prev]) {
        selectionIndex.value = prev;
      } else {
        const searchLength = search.value?.length ?? 0;
        selectionIndex.value = -1;
        input?.setSelectionRange(searchLength, searchLength);
      }
      event.stopPropagation();
      event.preventDefault();
      return;
    case "arrowright":
    case "right":
      if (!effectiveMultiple.value || menuOpen.value) return;

      if (selectionIndex.value < 0) return;

      const next = selectionIndex.value + 1;

      if (internalModelValue.value[next]) {
        selectionIndex.value = next;
      } else {
        selectionIndex.value = -1;
        input?.setSelectionRange(0, 0);
      }
      event.stopPropagation();
      event.preventDefault();
      return;
    case "esc":
    case "escape":
      if (!menuOpen.value && selectionIndex.value >= 0) {
        selectionIndex.value = -1;
        mainInputRef.value?.focus();
      } else {
        closeMenu(true);
      }
      event.stopPropagation();
      event.preventDefault();
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

function confirmPendingSelection(): void {
  // Check if the pending selection is the create item
  if (
    createItemLabel.value &&
    pendingSelection.value === createItemIndex.value
  ) {
    createItem();
    return;
  }

  // For regular items, pendingSelection directly corresponds to the item index
  const item = items.value[pendingSelection.value];
  if (!item) return;
  onInput(item);
}

const teardownQueue: Array<() => void> = [];
onBeforeUnmount(() => teardownQueue.forEach((fn) => fn()));

async function createItem(): Promise<void> {
  if (!createItemLabel.value) return;
  try {
    createItemLoading.value = true;
    const item = await props.create!.getItem(
      search.value!,
      createItemLabel.value,
    );
    if (!item) return;

    // If the newly created item doesn't have a PK when we call onInput
    // and emit the selected value, Vuetify's validation won't re-run on its own
    // once the item gains a primary key. This could happen if selecting the item
    // opens a dialog, for example, and the item is eventually saved by that dialog.
    // This is tested by test case "create with deferred save"
    if (!item[modelObjectMeta.value.keyProp.name]) {
      const watcher = watch(
        () => item[modelObjectMeta.value.keyProp.name],
        (pk) => pk && rootRef.value?.validate(),
      );
      teardownQueue.push(watcher);
    }

    onInput(item);

    listCaller(); // Refresh the list, because the new item is probably now in the results.
  } catch (e: unknown) {
    createItemError.value = getMessageForError(e);
  } finally {
    createItemLoading.value = false;
  }
}

function onIntersect(isIntersecting: boolean) {
  if (!props.autofocus || !isIntersecting) return;
  // Doesn't work reliably without a small delay
  setTimeout(() => {
    mainInputRef.value?.focus();
  }, 50);
}

async function onSearchIntersect(isIntersecting: boolean) {
  if (!isIntersecting) return;

  const input = searchRef.value?.$el.querySelector("input") as HTMLInputElement;
  if (!input) return;

  await nextTick();
  await new Promise((resolve) => setTimeout(resolve, 25));

  input.focus();

  if (pendingSearchSelect.value) {
    input.select();
    pendingSearchSelect.value = false;
  }
}

async function openMenu(select?: boolean): Promise<void> {
  if (!isInteractive.value || forceClosed) return;

  if (menuOpen.value) return;
  menuOpen.value = true;
  selectionIndex.value = -1; // Reset selection index when menu opens

  if (select == undefined) {
    // Select the whole search input if it hasn't changed recently.
    // If it /has/ changed recently, it means the user is actively typing and probably
    // doesn't want to use what they're typing.
    select = new Date().valueOf() - searchChanged.value.valueOf() > 1000;
  }

  if (props.reloadOnOpen) listCaller();

  // Store the select preference for when the input becomes visible
  pendingSearchSelect.value = select ?? false;
}

let forceClosed = false;
function closeMenu(force = false): void {
  if (force) {
    forceClosed = true;
    setTimeout(() => (forceClosed = false), 300);
  }

  if (!menuOpen.value) return;

  menuOpen.value = false;
  selectionIndex.value = -1;
  mainInputRef.value?.focus();
}

if (!valueMeta.value) {
  throw "c-select requires value metadata. Specify it with the 'for' prop'";
}

/**
 * A caller that will be used to resolve the full object when the only thing
 * that has been provided to c-select is a primary key value.
 */
const getCaller = new ModelApiClient<SelectedModelTypeSingle>(
  modelObjectMeta.value,
)
  .$useSimultaneousRequestCaching()
  .$makeCaller(
    "list",
    function () {
      throw "expected calls to be made with invokeWithArgs";
    },
    () => ({ ids: [] as SelectedPkTypeSingle[] }),
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
    },
  )
  .setConcurrency("debounce");

const listCaller = new ModelApiClient<SelectedModelTypeSingle>(
  modelObjectMeta.value,
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
  () => listCaller(),
);

watch(
  () => props.cache,
  (c) => {
    getCaller.useResponseCaching(c === true ? {} : c);
    listCaller.useResponseCaching(c === true ? {} : c);
  },
  { immediate: true },
);

watch(pendingSelection, async () => {
  await nextTick();
  await nextTick();
  const listDiv = menuContentRef.value?.$el as HTMLElement;
  const selectedItem = listDiv?.querySelector(".pending-selection");
  selectedItem?.scrollIntoView?.({
    behavior: "auto",
    block: "nearest",
    inline: "nearest",
  });
});

watch(search, (newVal, oldVal) => {
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

  .v-field__append-inner > .v-icon {
    cursor: pointer;
  }
  &.c-select--is-menu-active .v-field__append-inner > .v-icon {
    transform: rotate(180deg);
  }

  :has(.v-select__selection--selected) {
    .v-field__input {
      caret-color: transparent;
    }
    .v-select__selection:not(.v-select__selection--selected) {
      opacity: var(--v-medium-emphasis-opacity);
    }
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

  order: -1; // Default: position at start

  &.c-select__create-item--end {
    order: 999999; // Position at end
  }
}
</style>
