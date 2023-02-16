<template>
  <v-input
    class="c-select"
    :class="{
      'c-select--is-menu-active': menuOpen,
    }"
    :error-messages="error"
    :messages="
      focused ||
      inputBindAttrs['persistent-hint'] ||
      inputBindAttrs['persistentHint']
        ? inputBindAttrs.hint
        : ''
    "
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :modelValue="internalModelValue"
    #default="{ isDisabled, isReadonly, isValid }"
  >
    <v-field
      :active="!!internalModelValue || focused || !!placeholder"
      :dirty="!!internalModelValue"
      :clearable="!isDisabled.value && !isReadonly.value && isClearable"
      persistent-clear
      append-inner-icon="$dropdown"
      :error="isValid.value === false"
      v-bind="inputBindAttrs"
      :label="$attrs.label ?? modelNavProp?.displayName ?? inputBindAttrs.label"
      @click:clear.stop.prevent="onInput(null, true)"
      @keydown="!isDisabled.value && !isReadonly.value && onInputKey($event)"
      :focused="focused"
    >
      <div class="v-field__input">
        <slot
          v-if="internalModelValue"
          name="selected-item"
          :item="internalModelValue"
          :search="search"
        >
          <slot name="item" :item="internalModelValue" :search="search">
            <span style="overflow: hidden">
              <c-display :model="internalModelValue" />
            </span>
          </slot>
        </slot>

        <input
          type="text"
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
        menuOpen
          ? toggleMenu()
          : !isDisabled.value && !isReadonly.value && toggleMenu()
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
        >
        </v-text-field>

        <!-- TODO: i18n -->
        <div
          v-if="!createItemLabel && !listItems.length"
          class="grey--text px-4 my-3 font-italic"
        >
          No results found.
        </div>

        <!-- This height shows 7 full items, with a final item partially out 
        of the scroll area to improve visual hints to the user that the can scroll the list. -->
        <v-list class="py-0" max-height="302" ref="listRef" density="compact">
          <v-list-item
            v-if="createItemLabel"
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

<script lang="ts">
import { ComponentPublicInstance, defineComponent, PropType, ref } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";
import {
  ModelApiClient,
  ListApiState,
  ListResultPromise,
  Model,
  ModelType,
  ForeignKeyProperty,
  ModelReferenceNavigationProperty,
  ListParameters,
  mapParamsToDto,
  getMessageForError,
  mapValueToModel,
  ItemApiStateWithArgs,
  ViewModel,
  modelDisplay,
  Indexable,
} from "coalesce-vue";

export default defineComponent({
  name: "c-select",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,

  setup(props) {
    return {
      listRef: ref<ComponentPublicInstance>(),
      searchRef: ref<ComponentPublicInstance>(),
      ...useMetadataProps(props),
    };
  },

  props: {
    ...makeMetadataProps(),
    clearable: { required: false, default: undefined, type: Boolean },
    modelValue: { required: false },
    keyValue: { required: false },
    objectValue: { required: false },
    placeholder: { type: String, required: false },
    preselectFirst: { required: false, type: Boolean, default: false },
    preselectSingle: { required: false, type: Boolean, default: false },
    openOnClear: { required: false, type: Boolean, default: true },
    reloadOnOpen: { required: false, type: Boolean, default: false },
    params: { required: false, type: Object as PropType<ListParameters> },
    create: {
      required: false,
      type: Object as PropType<{
        getLabel: (search: string, items: Model<ModelType>[]) => string | false;
        getItem: (search: string, label: string) => Promise<Model<ModelType>>;
      }>,
    },
  },

  data() {
    return {
      modelDisplay: modelDisplay,

      search: null as string | null,
      error: [] as string[],
      focused: false,
      menuOpen: false,
      mainValue: "",
      createItemLoading: false,
      createItemError: "" as string | null,
      pendingSelection: 0,

      /** The model representing the current selected item
       * in the case that only the PK was provided to the component.
       * This is maintained in a variable to prevent its reference from
       * changing unexpectedly, which causes Vuetify to annoying things.
       */
      keyFetchedModel: null as any,

      listCaller: null! as ListApiState<[], Model<ModelType>> &
        (() => ListResultPromise<Model<ModelType>>),

      /**
       * A caller that will be used to resolve the full object when the only thing
       * that has been provided to c-select is a primary key value.
       */
      getCaller: null! as ItemApiStateWithArgs<
        [],
        { id: any },
        Model<ModelType>
      >,
    };
  },

  computed: {
    /** The effective clearability state of the dropdown. */
    isClearable(): boolean {
      if (typeof this.clearable == "boolean")
        // If explicitly given a value, use that value.
        return this.clearable;

      // Check to see if the foreign key is nullable (i.e. doesn't have a 'required' rule).
      return !!(this.modelKeyProp && !this.modelKeyProp.rules?.required);
    },

    /** The property on `this.model` which holds the foreign key being selected for, or `null` if there is no such property. */
    modelKeyProp(): ForeignKeyProperty | null {
      const meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "principalType" in meta) {
        return meta;
      }
      if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
        return meta.foreignKey;
      }
      return null;
    },

    /** The property on `this.model` which holds the reference navigation being selected for, or `null` if there is no such property. */
    modelNavProp(): ModelReferenceNavigationProperty | null {
      const meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "navigationProp" in meta) {
        return meta.navigationProp || null;
      }
      if (meta.role == "referenceNavigation" && "foreignKey" in meta) {
        return meta;
      }
      return null;
    },

    /**
     * Whether the metadata provided to `for` and retrieved via `this.valueMeta` is for a key or an object.
     * Dictates whether v-model will attempt to bind a key or an object.
     */
    primaryBindKind() {
      if (!this.valueMeta) {
        throw "c-select requires metadata";
      }

      if (this.valueMeta.role == "foreignKey") {
        return "key";
      }
      if (this.valueMeta.type == "model") {
        if (
          typeof this.modelValue != "object" &&
          this.modelValue !== undefined
        ) {
          throw (
            "Expected a model object to be bound to modelValue, but received a " +
            typeof this.modelValue
          );
        }
        return "model";
      }

      throw "The 'role' of the metadata provided for c-select must be 'foreignKey', or the type must be 'model' ";
    },

    /** The metadata of the type being selected by the dropdown. */
    modelObjectMeta() {
      var meta = this.valueMeta!;
      if (meta.role == "foreignKey" && "principalType" in meta) {
        return meta.principalType;
      } else if (meta.type == "model") {
        return meta.typeDef;
      } else {
        throw `Value ${meta.name} must be a foreignKey or model type to use c-select.`;
      }
    },

    /** The effective object (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
    internalModelValue() {
      if (this.objectValue) {
        return this.objectValue;
      }
      if (
        this.model &&
        this.modelNavProp &&
        (this.model as any)[this.modelNavProp.name]
      ) {
        return (this.model as any)[this.modelNavProp.name];
      }

      if (this.modelValue && this.primaryBindKind == "model") {
        return this.modelValue;
      }

      if (this.internalKeyValue) {
        // See if we already have a model that we're using to represent a key-only binding.
        // Storing this object prevents it from flipping between different instances
        // obtained from either this.getCaller or this.listCaller,
        // which causes vuetify to reset its search when the object passed to v-select's `modelValue` prop changes.
        if (
          this.keyFetchedModel &&
          this.internalKeyValue ===
            this.keyFetchedModel[this.modelObjectMeta.keyProp.name]
        ) {
          return this.keyFetchedModel;
        }

        // All we have is the PK. First, check if it is already in our item array.
        // If so, capture it. If not, request the object from the server.if (this.keyValue) {
        const item = this.items.filter(
          (i) =>
            this.internalKeyValue ===
            (i as any)[this.modelObjectMeta.keyProp.name]
        )[0];
        if (item) {
          this.keyFetchedModel = item;
          return item;
        }

        // See if we obtained the item via getCaller.
        const singleItem = this.getCaller.result;
        if (
          singleItem &&
          this.internalKeyValue ===
            (singleItem as any)[this.modelObjectMeta.keyProp.name]
        ) {
          this.keyFetchedModel = singleItem;
          return singleItem;
        }

        if (
          !this.listCaller.isLoading &&
          this.getCaller.args.id != this.internalKeyValue
        ) {
          // Only request the single item if the list isn't currently loading,
          // and if the last requested key is not the key we're looking for.
          // (this prevents an infinite loop of invokes if the call to the server fails.)
          // The single item may end up coming back from a pending list call.
          this.getCaller.args.id = this.internalKeyValue;
          this.getCaller.invokeWithArgs();
        }
      }
      return null;
    },

    /** The effective key (whose type is described by `this.modelObjectMeta`) that has been provided to the component. */
    internalKeyValue() {
      let value: any;
      if (this.keyValue) {
        value = this.keyValue;
      } else if (this.model && this.modelKeyProp) {
        value = (this.model as any)[this.modelKeyProp.name];
      } else if (this.modelValue && this.primaryBindKind == "key") {
        value = this.modelValue;
      } else {
        value = null;
      }

      if (value != null) {
        // Parse the value in case we were given a string instead of a number, or something like that, via the `keyValue` prop.
        // This prevents `internalModelValue` from getting confused and infinitely calling the `getCaller`.
        return mapValueToModel(value, this.modelObjectMeta.keyProp);
      }

      return null;
    },

    /** The effective set of validation rules to pass to the v-select. */
    effectiveRules() {
      // If we were explicitly given rules, use those.
      if (this.inputBindAttrs.rules) return this.inputBindAttrs.rules;

      if (this.model instanceof ViewModel && this.modelKeyProp) {
        // We're binding to a ViewModel instance.
        // Grab the rules from the instance, because it may contain custom rules
        // and/or other rule changes that have been customized in userland beyond what the metadata provides.

        // Since the v-autocomplete is always bound to a model instance,
        // vuetify will invoke the rules using the model object as the arg to the rule function.
        // However, we use the validation rules of the foreign key since the FK
        // is the actual scalar value that gets sent to the server,
        // and is the prop that we generate things like `required` onto.
        // We need to translate the rule functions to pass the selected FK instead
        // of the selected model object.
        return this.model
          .$getRules(this.modelKeyProp)
          ?.map((rule) => () => rule(this.internalKeyValue));
      }

      // Look for validation rules from the metadata on the key prop.
      // The foreign key is always the one that provides validation rules
      // for navigation properties - never the navigation property itself.
      if (this.modelKeyProp?.rules) {
        return Object.values(this.modelKeyProp.rules);
      }

      return [];
    },

    items() {
      return this.listCaller?.result || [];
    },

    listItems(): Indexable<Model<ModelType>>[] {
      return this.items;
    },

    createItemLabel() {
      if (!this.create || !this.search) return null;

      const result = this.create.getLabel(this.search, this.items);
      if (result) {
        return result;
      }
      return null;
    },
  },

  watch: {
    search(newVal: any, oldVal: any) {
      if (newVal != oldVal) {
        // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
        // since null == undefined, the transition from undefined to null will fail.
        this.listCaller();
      }
    },
    mainValue(val) {
      // Transfer any input typed into the main input to the search input,
      // and then open the menu. This has several jobs:
      // - If the user starts typing while the main input is focused, we take it as a search query
      // - We don't have to disable/readonly the main input
      if (val) {
        this.$nextTick(() => (this.mainValue = ""));
        if (!this.menuOpen) {
          this.search = val;
          this.openMenu(false);
        } else {
          this.search += val;
        }
      }
    },
    createItemLabel() {
      this.createItemError = null;
    },
  },

  methods: {
    onInput(value: Model<ModelType> | null | undefined, dontFocus = false) {
      value = value ?? null;
      if (value == null && !this.isClearable) {
        return;
      }

      // Clear any manual errors
      this.error = [];

      const key = value
        ? (value as any)[this.modelObjectMeta.keyProp.name]
        : null;
      this.keyFetchedModel = value;

      if (this.model) {
        if (this.modelKeyProp) {
          (this.model as any)[this.modelKeyProp.name] = key;
        }
        if (this.modelNavProp) {
          (this.model as any)[this.modelNavProp.name] = value;
        }
      }

      this.$emit(
        "update:modelValue",
        this.primaryBindKind == "key" ? key : value
      );
      this.$emit("update:objectValue", value);
      this.$emit("update:keyValue", key);
      this.pendingSelection = value ? this.listItems.indexOf(value) : 0;

      // When the input value is cleared, re-focus the dropdown
      // to allow the user to enter new search input.
      // Without this, pressing backspace to delete the current value
      // will cause the search field to lose focus and further keyboard input will do nothing.
      if (!dontFocus) {
        if (!value) {
          this.openMenu();
        } else {
          this.closeMenu();
        }
      }
    },

    /** When a key is pressed on the top level input */
    onInputKey(event: KeyboardEvent) {
      switch (event.key) {
        case "Delete":
        case "Backspace":
          if (!this.menuOpen) {
            this.onInput(null, true);
            event.stopPropagation();
            event.preventDefault();
            return;
          }
        case "Esc":
        case "Escape":
          event.stopPropagation();
          event.preventDefault();
          this.closeMenu();
          return;
        case " ":
        case "Enter":
        case "Up":
        case "ArrowUp":
        case "Down":
        case "ArrowDown":
        case "Spacebar":
          event.stopPropagation();
          event.preventDefault();
          this.openMenu();
          return;
      }
    },

    confirmPendingSelection() {
      var item = this.listItems[this.pendingSelection];
      if (!item) return;
      this.onInput(item);
    },

    async createItem() {
      if (!this.createItemLabel) return;
      try {
        this.createItemLoading = true;
        const item = await this.create!.getItem(
          this.search!,
          this.createItemLabel
        );
        if (!item) return;
        this.onInput(item);
        this.listCaller(); // Refresh the list, because the new item is probably now in the results.
      } catch (e: unknown) {
        this.createItemError = getMessageForError(e);
      } finally {
        this.createItemLoading = false;
      }
    },

    async openMenu(select = true) {
      if (this.menuOpen) return;
      this.menuOpen = true;

      if (this.reloadOnOpen) this.listCaller();

      await this.$nextTick();
      const input = this.searchRef?.$el.querySelector(
        "input"
      ) as HTMLInputElement;

      // Wait for the menu fade-in animation to unhide the content root
      // before we try to focus the search input, because otherwise it wont work.
      // https://stackoverflow.com/questions/19669786/check-if-element-is-visible-in-dom
      const start = performance.now();
      while (
        // cap waiting at 100ms
        start + 100 > performance.now() &&
        (!input.offsetParent || input != document.activeElement)
      ) {
        input.focus();
        await new Promise((resolve) => setTimeout(resolve, 1));
      }

      if (select) {
        input.select();
      }
    },

    closeMenu() {
      if (!this.menuOpen) return;
      this.menuOpen = false;
      //@ts-ignore
      this.$el.querySelector("input").focus();
    },

    toggleMenu() {
      if (this.menuOpen) this.closeMenu();
      else this.openMenu();
    },
  },

  created() {
    const propMeta = this.valueMeta;
    if (!propMeta) {
      throw "c-select requires value metadata. Specify it with the 'for' prop'";
    }

    // This needs to be late initialized so we have the correct "this" reference.
    this.getCaller = new ModelApiClient(this.modelObjectMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller(
        "item",
        function (c) {
          throw "expected calls to be made with invokeWithArgs";
        },
        () => ({ id: null as any }),
        (c, args) => c.get(args.id)
      )
      .setConcurrency("debounce");

    this.listCaller = new ModelApiClient(this.modelObjectMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller("list", (c) => {
        return c.list({
          pageSize: 100,
          ...this.params,
          search: this.search || undefined,
        });
      })
      .onFulfilled(() => {
        // Vuetify (2, anyway) is super buggy and won't highlight the first item
        // if we just directly set this to 0. We have to null it, and then after a cycle set it to 0.
        this.pendingSelection = 0;
      })
      .onRejected((state) => {
        this.error = [state.message || "Unknown Error"];
      })
      .setConcurrency("debounce");

    this.$watch(
      () => JSON.stringify(mapParamsToDto(this.params)),
      () => this.listCaller()
    );

    this.$watch(
      () => this.pendingSelection,
      async () => {
        await this.$nextTick();
        await this.$nextTick();
        var listDiv = this.listRef?.$el as HTMLElement;
        var selectedItem = listDiv?.querySelector(".v-list-item--active");
        selectedItem?.scrollIntoView({
          behavior: "auto",
          block: "nearest",
          inline: "nearest",
        });
      }
    );

    // Load the initial contents of the list.
    this.listCaller().then(() => {
      if (this.internalModelValue || this.internalKeyValue) {
        // Don't preselect if there's already a value selected.
        return;
      }

      const first = this.items[0];
      if (
        first &&
        (this.preselectFirst ||
          (this.preselectSingle && this.items.length === 1))
      ) {
        this.onInput(first, true);
      }
    });
  },

  mounted() {},
});
</script>
