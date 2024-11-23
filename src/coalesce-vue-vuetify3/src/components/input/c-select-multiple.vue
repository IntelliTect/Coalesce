<template>
  <c-select-core
    ref="core"
    :listCaller
    :listItems
    :create
    :inputBindAttrs
    :hasSelection="!!internalModelValue?.length"
    :clearable="clearable ?? true"
    @input="onInput"
  >
    <template #selections="{ search }">
      <slot
        v-for="item in internalModelValue || []"
        name="selected-item"
        :item="item"
        :search="search"
      >
        <span class="v-select__selection">
          <slot name="item" :item="item" :search="search">
            <c-display class="v-select__selection-text" :model="item" />
          </slot>
        </span>
      </slot>
    </template>
    <template #items="{ search, pendingSelection }">
      <v-list-item
        v-for="(item, i) in listItems"
        :key="item[modelObjectMeta.keyProp.name]"
        @click="onInput(item)"
        :value="i"
        :class="{ 'pending-selection': pendingSelection == i }"
        :active="internalKeySet.has(item[modelObjectMeta.keyProp.name])"
      >
        <template #prepend>
          <v-checkbox-btn
            tabindex="-1"
            :modelValue="internalKeySet.has(item[modelObjectMeta.keyProp.name])"
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
    </template>
  </c-select-core>
</template>

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
  onMounted,
} from "vue";
import {
  useMetadataProps,
  ForSpec,
  useCustomInput,
} from "../c-metadata-component";
import { TypedValidationRule } from "../../util";
import cSelectCore from "./c-select-core.vue";
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

defineOptions({
  name: "c-select-multiple",
  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,
});

type ExtractValuesOfType<T, U> = {
  [K in keyof T]: T[K] extends U ? T[K] : never;
}[keyof T];

type FindPk<TModel extends Model> = ExtractValuesOfType<
  TModel["$metadata"]["props"],
  PrimaryKeyProperty
>;

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
  ["selected-item"]?(props: {
    item: SelectedModelType;
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

const core = ref<InstanceType<typeof cSelectCore>>();

const { inputBindAttrs, modelMeta, valueMeta, valueOwner } =
  useMetadataProps(props);

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

const items = computed(() => {
  return (listCaller?.result || []) as SelectedModelType[];
});

const listItems = computed((): Indexable<SelectedModelType>[] => {
  return items.value;
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
  emit("update:keyValues", internalKeyValue.value);
  core.value!.pendingSelection = value ? listItems.value.indexOf(value) : 0;
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
      search: core.value!.search || undefined,
    });
  })
  .onFulfilled(() => {
    core.value!.pendingSelection = 0;
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

// Load the initial contents of the list.
onMounted(() => listCaller());

// defineExpose({
//   menuOpen: menuOpen,
//   search: search,
// });
</script>
