<template>
  <v-card class="c-admin-table" :class="'type-' + metadata.name">
    <c-admin-table-toolbar
      :list="viewModel"
      :page-sizes="pageSizes"
      @update:editable="editable = $event"
      :editable="canEdit ? editable : undefined"
      :color="color"
      elevation="4"
    />

    <v-card-text>
      <c-table
        admin
        :editable="editable"
        :list="viewModel"
        :extra-headers="
          canEdit || canDelete || hasInstanceMethods
            ? [{ header: 'Actions', isFixed: true }]
            : []
        "
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
                :variant="item.$isDirty ? 'elevated' : 'text'"
                :loading="item.$bulkSave.isLoading"
                icon
                @click="item.$bulkSave()"
              >
                <!-- TODO: (#413) ^^^ read dirty state for the whole bulk save. -->
                <!-- Using an <i> directly is more performant than v-icon. -->
                <i aria-hidden="true" class="v-icon notranslate fa fa-save"></i>
              </v-btn>

              <v-btn
                v-if="canEdit || hasInstanceMethods"
                title="Edit"
                variant="text"
                icon
                :to="getItemRoute(item)"
              >
                <i aria-hidden="true" class="v-icon notranslate fa fa-edit"></i>
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

      <v-btn
        v-if="editable && canCreate"
        @click="addItem"
        prepend-icon="fa fa-plus"
        :color
        class="my-2"
      >
        Add Item
      </v-btn>

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
} from "coalesce-vue";

import { computed, defineComponent, PropType, ref, toRef } from "vue";
import { useRouter } from "vue-router";
import { useAdminTable } from "./useAdminTable";

export default defineComponent({
  name: "c-admin-table",

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
  },

  setup(props) {
    const tableProps = useAdminTable(toRef(props, "list"));
    const editable = ref(false);

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
    };
  },

  computed: {
    viewModel(): ListViewModel {
      if (this.list instanceof ListViewModel) return this.list;
      throw Error(
        "c-admin-table: prop `list` is required, and must be a ListViewModel."
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

    addItem() {
      const viewModel = new ViewModel.typeLookup![this.metadata.name]();
      viewModel.$params = this.list.$params;
      this.list.$items.push(viewModel);
    },
  },

  created() {
    if (this.queryBind) {
      bindToQueryString(
        this,
        this,
        "editable",
        "editable",
        (v) => v === "true",
        "replace"
      );

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
                      Object.entries(old!).map((e) => [e[0], undefined])
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
        { deep: true }
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
            this.viewModel.$metadata
          );
        },
        { immediate: true }
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
      { immediate: true }
    );

    this.viewModel.$load.setConcurrency("debounce");
    this.viewModel.$startAutoLoad(this, { wait: 0 });
    this.viewModel.$load();
  },
});
</script>

<style lang="scss">
.c-admin-table {
  a {
    text-decoration: none;
  }
}
</style>
