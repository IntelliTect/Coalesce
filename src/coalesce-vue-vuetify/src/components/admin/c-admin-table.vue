<template>
  <v-card class="c-admin-table">
    <c-admin-table-toolbar :list="viewModel" :editable.sync="editable" />

    <v-card-text class="pt-3">
      <c-loader-status
        :loaders="{
          '': [
            ...viewModel.$items.map(i => i.$delete),
            ...viewModel.$items.map(i => i.$save)
          ]
        }"
      >
        <c-table
          admin
          :editable="editable"
          :list="viewModel"
          :extra-headers="
            canEdit || canDelete || hasInstanceMethods ? ['Actions'] : []
          "
        >
          <template #item.append="{item}">
            <td width="1%">
              <v-row class="flex-nowrap" no-gutters>
                <v-btn
                  v-if="canEdit || hasInstanceMethods"
                  class="mx-1"
                  title="Edit"
                  text
                  icon
                  :to="editRoute(item)"
                >
                  <!-- Using an <i> directly is much more performant than v-icon. -->
                  <i
                    aria-hidden="true"
                    class="v-icon notranslate fa fa-edit"
                  ></i>
                </v-btn>

                <v-btn
                  v-if="canDelete"
                  class="mx-1"
                  title="Delete"
                  text
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
      </c-loader-status>

      <c-list-pagination
        :list="list"
        :pageSizes="pageSizes"
        class="mt-4"
      ></c-list-pagination>
    </v-card-text>
  </v-card>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from "vue-property-decorator";
import {
  Model,
  ClassType,
  ListViewModel,
  Property,
  ModelType,
  ViewModel,
  BehaviorFlags,
  ListParameters,
  mapQueryToParams,
  mapParamsToDto,
  bindToQueryString
} from "coalesce-vue";

import CTable from "../display/c-table.vue";
import CAdminTableToolbar from "./c-admin-table-toolbar.vue";

@Component({
  name: "c-admin-table",
  components: {
    CTable,
    CAdminTableToolbar
  }
})
export default class extends Vue {
  @Prop({ required: true, type: Object })
  public list!: any;

  @Prop({ required: false })
  pageSizes?: number[];

  @Prop({ type: Boolean, default: false })
  queryBind?: boolean;

  editable = false;

  get viewModel(): ListViewModel {
    if (this.list instanceof ListViewModel) return this.list;
    throw Error(
      "c-admin-table: prop `list` is required, and must be a ListViewModel."
    );
  }

  async deleteItemWithConfirmation(item: ViewModel<any, any>) {
    if (confirm("Are you sure you wish to delete this item?")) {
      await item.$delete();
      await this.viewModel.$load();
    }
  }

  get metadata(): ModelType {
    return this.viewModel.$metadata;
  }

  get canEdit() {
    return (
      this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Edit) != 0
    );
  }
  get canDelete() {
    return (
      this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Delete) != 0
    );
  }
  get hasInstanceMethods() {
    return (
      this.metadata &&
      Object.values(this.metadata.methods).some(m => !m.isStatic)
    );
  }

  editRoute(item: ViewModel) {
    // Resolve to an href to allow overriding of admin routes in userspace.
    // If we just gave a named raw location, it would always use the coalesce admin route
    // instead of the user-overridden one (that the user overrides by declaring another
    // route with the same path).
    return this.$router.resolve({
      name: "coalesce-admin-item",
      params: {
        type: this.metadata.name,
        id: item.$primaryKey
      }
    }).resolved.fullPath;
  }

  created() {
    if (this.queryBind) {
      bindToQueryString(
        this,
        this,
        "editable",
        "editable",
        v => v === "true",
        "replace"
      );

      // Pull initial parameters from the querystring before we setup watchers.
      this.viewModel.$params = mapQueryToParams(
        this.$route.query,
        ListParameters,
        this.viewModel.$metadata
      );

      this.$watch(
        () => mapParamsToDto(this.viewModel.$params),
        (mappedParams, old) => {
          this.$router.replace({
            query: {
              // First, take all existing query params so that any that aren't handled
              // by mapQueryToParams/mapParamsToDto don't get lost
              ...this.$route.query,
              // Next, set all previous query-mapped params to undefined
              // so that any that aren't in the new mappedParams object get unset
              ...(typeof old == "object"
                ? Object.fromEntries(
                    Object.entries(old!).map(e => [e[0], undefined])
                  )
                : {}),
              // Then layer on any new params, overwriting any that got set to undefined previously.
              ...(mappedParams as any)
            }
          });
        },
        { deep: true }
      );

      // When the query changes, grab the new value.
      this.$watch(
        () => this.$route.query,
        (v: any) => {
          this.viewModel.$params = mapQueryToParams(
            v,
            ListParameters,
            this.viewModel.$metadata
          );
        }
      );
    }

    this.$watch(
      () => [this.editable, ...this.viewModel.$items.map(i => i.$stableId)],
      () => {
        if (this.editable) {
          for (const item of this.viewModel.$items) {
            item.$startAutoSave(this, { wait: 100 });
          }
        }
      },
      { immediate: true, deep: true }
    );

    this.viewModel.$load.setConcurrency("debounce");
    this.viewModel.$startAutoLoad(this, { wait: 0 });
    this.viewModel.$load();
  }
}
</script>

<style lang="scss">
.c-admin-table {
  a {
    text-decoration: none;
  }
}
</style>
