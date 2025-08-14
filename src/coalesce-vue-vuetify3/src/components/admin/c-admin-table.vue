<template>
  <v-card class="c-admin-table" :class="'type-' + metadata.name">
    <c-admin-table-toolbar
      :list="viewModel"
      @update:editable="editable = $event"
      :editable="canEdit ? editable : undefined"
      :pageSizes
      :color
      :showColumnSelector
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
        :props="tablePreferences.effectiveColumnNames.value"
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
} from "coalesce-vue";

import { computed, defineComponent, PropType, ref, toRef } from "vue";
import { useRouter } from "vue-router";
import { useAdminTable } from "./useAdminTable";
import { useTablePreferences } from "./useTablePreferences";
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
    showColumnSelector: { type: Boolean, default: true },
  },

  setup(props) {
    const tableProps = useAdminTable(toRef(props, "list"));
    const editable = ref(false);

    // Initialize table preferences
    const tablePreferences = useTablePreferences({
      metadata: tableProps.metadata.value,
    });

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
      tablePreferences,
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
