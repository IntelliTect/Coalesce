<template>
  <v-card class="c-admin-table" :class="'type-' + metadata.name">
    <c-admin-table-toolbar
      :list="viewModel"
      :page-sizes="pageSizes"
      @update:editable="editable = $event"
      :editable="canEdit ? editable : undefined"
      :color="color"
      :available-props="availableProps"
      :selected-columns="selectedColumns"
      :default-columns="defaultColumns"
      :column-selection-enabled="effectiveColumnSelection"
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
        :props="selectedColumns"
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

<script lang="ts">
import {
  ListViewModel,
  ViewModel,
  ListParameters,
  mapQueryToParams,
  mapParamsToDto,
  bindToQueryString,
  ModelType,
  HiddenAreas,
} from "coalesce-vue";

import { computed, defineComponent, PropType, ref, toRef, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAdminTable } from "./useAdminTable";
import { copyParamsToNewViewModel } from "./util";

import CAdminCreateBtn from "./c-admin-create-btn.vue";

export default defineComponent({
  name: "c-admin-table",
  components: { CAdminCreateBtn },

  props: {
    list: { required: true, type: Object as PropType<ListViewModel> },
    pageSizes: { required: false, type: Array as PropType<number[]> },
    color: { required: false, type: String, default: null },
    queryBind: { type: Boolean, default: false },
    autoSave: {
      required: false,
      type: [String, Boolean] as PropType<"auto" | boolean>,
      default: "auto",
    },
    columns: {
      required: false,
      type: Array as PropType<string[]>,
      default: undefined,
    },
    columnSelection: {
      required: false,
      type: Boolean,
      default: undefined,
    },
    columnSelectionKey: {
      required: false,
      type: String,
      default: undefined,
    },
  },

  setup(props) {
    const tableProps = useAdminTable(toRef(props, "list"));
    const editable = ref(false);
    const route = useRoute();

    // Column selection logic
    const availableProps = computed(() => {
      return Object.values(tableProps.metadata.value.props).filter(
        (p) => p.hidden === undefined || (p.hidden & HiddenAreas.List) == 0,
      );
    });

    const defaultColumns = computed(() => {
      return availableProps.value.map((p) => p.name);
    });

    const effectiveColumnSelection = computed(() => {
      // If columns prop is provided, disable column selection by default
      if (props.columns !== undefined) {
        return props.columnSelection === true;
      }
      // Otherwise, enable column selection by default unless explicitly disabled
      return props.columnSelection !== false;
    });

    const storageKey = computed(() => {
      if (props.columnSelectionKey) {
        return `coalesce-admin-table-columns-${props.columnSelectionKey}`;
      }
      return `coalesce-admin-table-columns-${route.path}`;
    });

    const selectedColumns = ref<string[]>([]);

    // Computed property to get effective columns based on preferences and defaults
    const effectiveColumns = computed(() => {
      // If columns are explicitly provided as props, use those
      if (props.columns) {
        return props.columns;
      }

      const defaults = defaultColumns.value;
      const saved = getSavedColumnPreferences();

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

    // Get saved column preferences from localStorage
    const getSavedColumnPreferences = (): Record<string, boolean> | null => {
      if (!effectiveColumnSelection.value) return null;

      try {
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
      } catch (error) {
        console.warn(
          "Failed to load column preferences from localStorage:",
          error,
        );
      }

      return null;
    };

    // Handle column updates with the new preference-based approach
    const onColumnsUpdated = (newColumns: string[]) => {
      if (!effectiveColumnSelection.value) {
        selectedColumns.value = newColumns;
        return;
      }

      const currentEffective = effectiveColumns.value;
      const preferences = getSavedColumnPreferences() || {};

      // IMPORTANT: Only make changes to "preferences" based on the user's explicit change.
      // Don't capture the entire column state into preferences so that when adjustments
      // are made to a table's defaults, they aren't pre-emptively excluded for the user
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

      saveColumnPreferences(preferences);
      selectedColumns.value = newColumns;
    };

    // Save column preferences to localStorage
    const saveColumnPreferences = (preferences: Record<string, boolean>) => {
      if (!effectiveColumnSelection.value) return;

      try {
        localStorage.setItem(storageKey.value, JSON.stringify(preferences));
      } catch (error) {
        console.warn(
          "Failed to save column preferences to localStorage:",
          error,
        );
      }
    };

    // Load initial column selection
    const loadColumnSelection = () => {
      selectedColumns.value = effectiveColumns.value;
    };

    // Watch for changes to available props and update selected columns if needed
    watch(
      availableProps,
      () => {
        loadColumnSelection();
      },
      { immediate: true },
    );

    // Watch for changes to props that affect column selection
    watch(
      () => [props.columns, props.columnSelection, props.columnSelectionKey],
      () => {
        loadColumnSelection();
      },
    );

    const effectiveAutoSave = computed(() => {
      if (!editable.value) return false;

      const value = props.autoSave;
      if (value == null || value == "auto") {
        const meta = tableProps.metadata.value;
        for (const propName in meta.props) {
          if (meta.props[propName].createOnly) {
            return false;
          }
        }
        return true;
      }
      return value;
    });

    return {
      router: useRouter(),
      ...tableProps,
      editable,
      effectiveAutoSave,
      availableProps,
      defaultColumns,
      selectedColumns,
      effectiveColumnSelection,
      onColumnsUpdated,
    };
  },

  computed: {
    viewModel(): ListViewModel {
      if (this.list instanceof ListViewModel) return this.list;
      throw Error(
        "c-admin-table: prop `list` is required, and must be a ListViewModel.",
      );
    },
  },

  methods: {
    async deleteItemWithConfirmation(item: ViewModel<any, any>) {
      if (confirm("Are you sure you wish to delete this item?")) {
        await item.$delete();
        await this.viewModel.$load();
      }
    },

    addItem(meta: ModelType) {
      const viewModel = new ViewModel.typeLookup![meta.name]();
      copyParamsToNewViewModel(viewModel, this.list.$params);
      this.list.$items.push(viewModel);
    },
  },

  created() {
    if (this.queryBind) {
      bindToQueryString(this, this, "editable", { parse: (v) => v === "true" });

      // Establish the baseline parameters that do not need to be represented in the query string.
      // I.E. don't put the default values of parameters in the query string.
      const baselineParams = mapParamsToDto(this.viewModel.$params);

      // When the parameters change, put them into the query string.
      this.$watch(
        () => mapParamsToDto(this.viewModel.$params),
        (mappedParams: any, old: any) => {
          // For any parameters that match the baseline parameters,
          // do not put those parameters in the query string.
          for (const key in baselineParams) {
            if (mappedParams[key] == baselineParams[key]) {
              delete mappedParams[key];
            }
          }

          this.router
            .replace({
              query: {
                // First, take all existing query params so that any that aren't handled
                // by mapQueryToParams/mapParamsToDto don't get lost
                ...this.router.currentRoute.value.query,
                // Next, set all previous query-mapped params to undefined
                // so that any that aren't in the new mappedParams object get unset
                ...(typeof old == "object"
                  ? Object.fromEntries(
                      Object.entries(old!).map((e) => [e[0], undefined]),
                    )
                  : {}),
                // Then layer on any new params, overwriting any that got set to undefined previously.
                ...(mappedParams as any),
              },
            })
            .catch((err) => {
              // Ignore errors about duplicate navigations. These are annoying and useless.
              if (err.name === "NavigationDuplicated") return;
              throw err;
            });
        },
        { deep: true },
      );

      // When the query changes, grab the new parameter values.
      this.$watch(
        () => this.router.currentRoute.value.query,
        (v: any) => {
          this.viewModel.$params = mapQueryToParams(
            {
              ...baselineParams,
              // Overlay the query values on top of the baseline parameters.
              ...v,
            },
            ListParameters,
            this.viewModel.$metadata,
          );
        },
        { immediate: true },
      );
    }

    this.$watch(
      () => this.effectiveAutoSave,
      (effectiveAutoSave) => {
        if (effectiveAutoSave && !this.viewModel.$isAutoSaveEnabled) {
          this.viewModel.$startAutoSave(this, { wait: 500 });
        } else if (!effectiveAutoSave && this.viewModel.$isAutoSaveEnabled) {
          this.viewModel.$stopAutoSave();
        }
      },
      { immediate: true },
    );

    this.viewModel.$load.setConcurrency("debounce");
    this.viewModel.$startAutoLoad(this, { wait: 0 });
    this.viewModel.$load();
  },
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
