<template>
  <v-input
    class="c-select"
    :class="{
      'c-select--is-menu-active': menuOpen,
    }"
    :focused="focused"
    v-bind="inputBindAttrs"
    :modelValue="hasSelection"
    :disabled="isDisabled"
    :readonly="isReadonly"
    #default="{ isValid }"
  >
    <v-field
      :error="isValid.value === false"
      append-inner-icon="$dropdown"
      v-bind="fieldAttrs"
      :clearable="isInteractive && clearable"
      :active="hasSelection || focused || !!placeholder"
      :dirty="hasSelection"
      :focused="focused"
      @click:clear.stop.prevent="emit('input', null, true)"
      @keydown="onInputKey($event)"
    >
      <div class="v-field__input">
        <slot name="selections" :search />

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
          :placeholder="hasSelection ? undefined : placeholder"
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

          <slot name="items" :search :pendingSelection />

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

.c-select__menu-content {
  .v-list-item.pending-selection {
    &::after {
      opacity: calc(0.15 * var(--v-theme-overlay-multiplier));
    }
    > .v-list-item__overlay {
      opacity: calc(var(--v-focus-opacity) * var(--v-theme-overlay-multiplier));
    }
  }
}
</style>

<script setup lang="ts">
import { getMessageForError, ListApiState, Model } from "coalesce-vue";
import {
  camelize,
  ComponentPublicInstance,
  computed,
  nextTick,
  ref,
  watch,
} from "vue";
import { useCustomInput } from "../c-metadata-component";
import { VField } from "vuetify/components";

const props = defineProps<{
  inputBindAttrs: any;
  hasSelection: boolean;
  clearable: boolean;
  listCaller: ListApiState<[], Model>;
  listItems: Model[];
  create?: {
    getLabel: (search: string, items: any[]) => string | false;
    getItem: (search: string, label: string) => Promise<Model>;
  };
}>();

defineSlots<{
  ["selections"]?(props: {
    search: string | null;
    pendingSelection: number;
  }): any;
}>();

const emit = defineEmits<{
  input: [item: Model | null, dontFocus?: boolean];
}>();

const { isDisabled, isReadonly, isInteractive } = useCustomInput(props);

const mainInputRef = ref<HTMLInputElement>();
const listRef = ref<ComponentPublicInstance>();
const searchRef = ref<ComponentPublicInstance>();

const fieldAttrs = computed(() =>
  VField.filterProps(
    Object.fromEntries(
      // We have to perform prop name normalization ourselves here
      // because vuetify's filterProps doesn't support the non-camelized names.
      Object.entries(props.inputBindAttrs).map(([k, v]) => [camelize(k), v])
    )
  )
);

const search = ref(null as string | null);
const focused = ref(false);
const createItemLoading = ref(false);
const createItemError = ref("" as string | null);
const pendingSelection = ref(0);
const menuOpen = ref(false);
const menuOpenForced = ref(false);
const searchChanged = ref(new Date());
const mainValue = ref("");

const createItemLabel = computed(() => {
  if (!props.create || !search.value) return null;

  const result = props.create.getLabel(search.value, props.listItems);
  if (result) {
    return result;
  }
  return null;
});

async function createItem() {
  if (!createItemLabel.value) return;
  try {
    createItemLoading.value = true;
    const item = await props.create!.getItem(
      search.value!,
      createItemLabel.value
    );
    if (!item) return;
    emit("input", item);
    props.listCaller(); // Refresh the list, because the new item is probably now in the results.
  } catch (e: unknown) {
    createItemError.value = getMessageForError(e);
  } finally {
    createItemLoading.value = false;
  }
}

/** When a key is pressed on the top level input */
function onInputKey(event: KeyboardEvent) {
  if (!isInteractive.value) return;

  switch (event.key.toLowerCase()) {
    case "delete":
    case "backspace":
      if (!menuOpen.value) {
        emit("input", null, true);
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
  var item = props.listItems[pendingSelection.value];
  if (!item) return;
  emit("input", item);
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

  if (props.reloadOnOpen) props.listCaller();

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

watch(createItemLabel, () => {
  createItemError.value = null;
});

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
    props.listCaller();
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

defineExpose({
  search,
  pendingSelection,
  openMenu,
  closeMenu,
});
</script>
