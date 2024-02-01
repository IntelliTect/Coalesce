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
          canEdit || canDelete || hasInstanceMethods ? ['Actions'] : []
        "
        :loaders="{
          '': [
            ...viewModel.$items.map((i) => i.$delete),
            ...viewModel.$items.map((i) => i.$save),
          ],
        }"
      >
        <template #item-append="{ item }">
          <td width="1%" class="c-admin-table--actions">
            <v-row class="flex-nowrap" no-gutters>
              <v-btn
                v-if="canEdit || hasInstanceMethods"
                class="mx-1"
                title="Edit"
                variant="text"
                icon
                :to="editRoute(item)"
              >
                <!-- Using an <i> directly is much more performant than v-icon. -->
                <i aria-hidden="true" class="v-icon notranslate fa fa-edit"></i>
              </v-btn>

              <v-btn
                v-if="canDelete"
                class="mx-1"
                title="Delete"
                variant="text"
                icon
                @click="deleteItemWithConfirmation(item)"
              >
                <i
                  aria-hidden="true"
                  class="v-icon notranslate fa fa-trash-alt"
                ></i>
              </v-btn>
            </v-row>
          </td>
        </template>
      </c-table>

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
  ModelType,
  ViewModel,
  BehaviorFlags,
  ListParameters,
  mapQueryToParams,
  mapParamsToDto,
  bindToQueryString,
} from "coalesce-vue";

import { defineComponent, PropType } from "vue";
import { useRouter } from "vue-router";

export default defineComponent({
  name: "c-admin-table",

  props: {
    list: { required: true, type: Object as PropType<ListViewModel> },
    pageSizes: { required: false, type: Array as PropType<number[]> },
    color: { required: false, type: String, default: null },
    queryBind: { type: Boolean, default: false },
  },

  setup() {
    return { router: useRouter() };
  },

  data() {
    return {
      editable: false,
    };
  },

  computed: {
    viewModel(): ListViewModel {
      if (this.list instanceof ListViewModel) return this.list;
      throw Error(
        "c-admin-table: prop `list` is required, and must be a ListViewModel."
      );
    },

    metadata(): ModelType {
      return this.viewModel.$metadata;
    },

    canEdit() {
      return (
        this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Edit) != 0
      );
    },

    canDelete() {
      return (
        this.metadata &&
        (this.metadata.behaviorFlags & BehaviorFlags.Delete) != 0
      );
    },

    hasInstanceMethods() {
      return (
        this.metadata &&
        Object.values(this.metadata.methods).some((m) => !m.isStatic)
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

    editRoute(item: ViewModel) {
      // Resolve to an href to allow overriding of admin routes in userspace.
      // If we just gave a named raw location, it would always use the coalesce admin route
      // instead of the user-overridden one (that the user overrides by declaring another
      // route with the same path).
      return this.router.resolve({
        name: "coalesce-admin-item",
        params: {
          type: this.metadata.name,
          id: item.$primaryKey,
        },
        query: this.queryBind
          ? // If there's a data source for the list, pass it to the edit page
            // in case that data source is the only thing allowing the item to be loaded.
            mapParamsToDto({ dataSource: this.viewModel.$params.dataSource })
          : {},
      }).fullPath;
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
      () => this.editable,
      (editable) => {
        if (editable && !this.viewModel.$isAutoSaveEnabled) {
          this.viewModel.$startAutoSave(this, { wait: 100 });
        } else if (!editable) {
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
