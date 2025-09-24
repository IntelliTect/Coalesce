<template>
  <v-card class="c-admin-table" :class="'type-' + metadata.name">
    <c-admin-table-toolbar
      :list="viewModel"
      :page-sizes="pageSizes"
      @update:editable="editable = $event"
      :editable="canEdit ? editable : undefined"
      :color="color"
      :selected-columns="effectiveColumns"
      @update:selected-columns="onColumnsUpdated"
    />

    <div v-if="metadata.description" class="c-admin-page--description">
      <i class="fa fa-info-circle"></i>
      {{ metadata.displayName }}: {{ metadata.description }}
    </div>

    <v-card-text>
      <c-table
        admin
        :editable="editable"
        :list="viewModel"
        :props="effectiveColumns"
        :extra-headers="[{ header: 'Actions', isFixed: true }]"
        :loaders="{
          '': list.$modelOnlyMode
            ? []
            : [
                ...viewModel.$items.map((i) => i.$delete),
                ...(!editable
                  ? []
                  : viewModel.$items.flatMap((i) => [i.$save, i.$bulkSave])),
              ],
        }"
        @click:item="
          (item) =>
            $router.push(
              getItemRoute(item) ??
                (() => {
                  throw new Error('Unable to get admin route for item');
                })(),
            )
        "
      >
        <template #item-append="{ item, isHorizontalScrollbarVisible }">
          <td
            width="1%"
            :class="{ ['fixed-column-right']: isHorizontalScrollbarVisible }"
            class="c-admin-table--actions"
          >
            <div class="d-flex flex-nowrap text-no-wrap ga-1" no-gutters>
              <v-btn
                v-if="editable && !effectiveAutoSave"
                title="Save"
                color="success"
                :variant="item.$bulkSavePreview().isDirty ? 'elevated' : 'text'"
                :loading="item.$bulkSave.isLoading"
                icon
                @click="item.$bulkSave()"
              >
                <!-- Using an <i> directly is more performant than v-icon. -->
                <i aria-hidden="true" class="v-icon notranslate fa fa-save"></i>
              </v-btn>

              <v-btn
                :title="canEdit || hasInstanceMethods ? 'Edit' : 'View'"
                variant="text"
                icon
                :to="getItemRoute(item)"
              >
                <i
                  aria-hidden="true"
                  class="v-icon notranslate fa"
                  :class="
                    canEdit || hasInstanceMethods ? 'fa-edit' : 'fa-ellipsis-h'
                  "
                ></i>
              </v-btn>

              <v-btn
                v-if="canDelete"
                title="Delete"
                variant="text"
                icon
                @click="deleteItemWithConfirmation(item)"
                :loading="item.$delete.isLoading"
              >
                <i
                  aria-hidden="true"
                  class="v-icon notranslate fa fa-trash-alt"
                ></i>
              </v-btn>
            </div>
          </td>
        </template>
      </c-table>

      <c-admin-create-btn
        v-if="editable"
        :list
        @add="addItem"
        label="Add Item"
        :color
        class="my-2"
      ></c-admin-create-btn>

      <c-list-pagination
        :list="list"
        :page-sizes="pageSizes"
        class="mt-4"
      ></c-list-pagination>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import {
  ListViewModel,
  ViewModel,
  useBindListParametersToQueryString,
  ModelType,
  HiddenAreas,
  useBindToQueryString,
} from "coalesce-vue";

import {
  computed,
  customRef,
  ref,
  toRef,
  watch,
  getCurrentInstance,
  onMounted,
} from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAdminTable } from "./useAdminTable";
import { copyParamsToNewViewModel } from "./util";

import CAdminCreateBtn from "./c-admin-create-btn.vue";

defineOptions({
  name: "c-admin-table",
});

const props = withDefaults(
  defineProps<{
    list: ListViewModel;
    pageSizes?: number[];
    color?: string;
    queryBind?: boolean;
    autoSave?: "auto" | boolean;
    columns?: string[];
    columnSelectionKey?: string;
  }>(),
  {
    autoSave: "auto",
  },
);

const router = useRouter();
const route = useRoute();
const instance = getCurrentInstance()!;
const { metadata, canEdit, canDelete, hasInstanceMethods, getItemRoute } =
  useAdminTable(toRef(props, "list"));

const editable = ref(false);

const viewModel = computed((): ListViewModel => {
  if (props.list instanceof ListViewModel) return props.list;
  throw Error(
    "c-admin-table: prop `list` is required, and must be a ListViewModel.",
  );
});

// Column selection logic
const availableProps = computed(() => {
  return Object.values(metadata.value.props).filter((p) => {
    if (p.role == "foreignKey" && p.navigationProp) {
      return false;
    }
    return true;
  });
});

const defaultColumns = computed(() => {
  return (
    props.columns ||
    availableProps.value
      .filter(
        (p) => p.hidden === undefined || (p.hidden & HiddenAreas.List) == 0,
      )
      .map((p) => p.name)
  );
});

const storageKey = computed(() => {
  const base = `c-admin-columns-${metadata.value.name}`;
  if (props.columnSelectionKey) {
    return `${base}-${props.columnSelectionKey}`;
  }
  return `${base}-${route.path}`;
});

const effectiveColumns = computed(() => {
  const defaults = defaultColumns.value;
  const saved = columnPreferences.value;

  if (!saved || Object.keys(saved).length === 0) {
    return defaults;
  }

  // Start with defaults and apply preferences
  const result: string[] = [];
  for (const col of availableProps.value.map((p) => p.name)) {
    const preference = saved[col];
    if (preference === true) {
      // Explicitly included
      result.push(col);
    } else if (preference === false) {
      // Explicitly excluded - skip
    } else if (defaults.includes(col)) {
      // No explicit preference, but in defaults
      result.push(col);
    }
  }

  return result;
});

function onColumnsUpdated(newColumns: string[] | null) {
  if (newColumns == null) {
    columnPreferences.value = null;
    return;
  }

  const currentEffective = effectiveColumns.value;
  const preferences = columnPreferences.value || {};

  // IMPORTANT: Only make changes to "preferences" based on the user's explicit change.
  // Don't capture the entire column state into preferences so that when adjustments
  // are made to a table's defaults, they aren't preemptively excluded for the user
  // if the user has never made an explicit decision about the column.

  for (const newCol of newColumns) {
    if (!currentEffective.includes(newCol)) {
      // Column is newly selected.
      preferences[newCol] = true;
    }
  }
  for (const oldCol of currentEffective) {
    if (!newColumns.includes(oldCol)) {
      // Column is newly deselected.
      preferences[oldCol] = false;
    }
  }

  columnPreferences.value = preferences;
}

const columnPreferences = customRef<Record<string, boolean> | null>(
  (track, trigger) => {
    return {
      get(): Record<string, boolean> | null {
        track();
        const saved = localStorage.getItem(storageKey.value);
        if (saved) {
          const parsed = JSON.parse(saved);
          if (
            typeof parsed === "object" &&
            parsed !== null &&
            !Array.isArray(parsed)
          ) {
            return parsed;
          }
        }
        return null;
      },
      set(preferences: Record<string, boolean> | null) {
        if (preferences === null) {
          localStorage.removeItem(storageKey.value);
        } else {
          localStorage.setItem(storageKey.value, JSON.stringify(preferences));
        }
        trigger();
      },
    };
  },
);

const effectiveAutoSave = computed(() => {
  if (!editable.value) return false;

  const value = props.autoSave;
  if (value == null || value == "auto") {
    const meta = metadata.value;
    for (const propName in meta.props) {
      if (meta.props[propName].createOnly) {
        return false;
      }
    }
    return true;
  }
  return value;
});

async function deleteItemWithConfirmation(item: ViewModel<any, any>) {
  if (confirm("Are you sure you wish to delete this item?")) {
    await item.$delete();
    await viewModel.value.$load();
  }
}

function addItem(meta: ModelType) {
  const vm = new ViewModel.typeLookup![meta.name]();
  copyParamsToNewViewModel(vm, props.list.$params);
  props.list.$items.push(vm);
}

onMounted(() => {
  if (props.queryBind) {
    useBindToQueryString(editable, {
      queryKey: "editable",
      parse: (v) => v === "true",
    });

    // Set up two-way binding between list parameters and query string
    useBindListParametersToQueryString(viewModel.value);
  }

  watch(
    () => effectiveAutoSave.value,
    (effectiveAutoSave) => {
      if (effectiveAutoSave && !viewModel.value.$isAutoSaveEnabled) {
        viewModel.value.$startAutoSave(instance);
      } else if (!effectiveAutoSave && viewModel.value.$isAutoSaveEnabled) {
        viewModel.value.$stopAutoSave();
      }
    },
    { immediate: true },
  );

  viewModel.value.$load.setConcurrency("debounce");
  viewModel.value.$startAutoLoad(instance);
  viewModel.value.$load();
});
</script>

<style lang="scss">
.c-admin-table {
  .c-table:not(.c-table--editable) tbody tr {
    &:hover {
      cursor: pointer;
      td:not(.fixed-column-right) {
        background: rgba(var(--v-border-color), var(--v-hover-opacity));
      }
    }
  }
  a:not(.v-btn) {
    text-decoration: none;

    // Prevents unclickable space between wrapped lines in links that have to wrap to multiple lines:
    display: inline-block;

    &:hover {
      filter: brightness(1.2);
    }
  }
}
</style>
