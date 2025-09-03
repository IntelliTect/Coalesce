<template>
  <v-menu class="c-list-filters--menu" :close-on-content-click="false">
    <template #activator="{ props: menuProps }">
      <v-btn class="c-list-filters" variant="text" v-bind="menuProps">
        <v-badge
          :model-value="!!activeCount"
          location="top start"
          floating
          :content="activeCount"
          color="secondary"
        >
          <v-icon :start="$vuetify.display.mdAndUp">
            {{ columnSelection ? "fa fa-columns" : "fa fa-filter" }}
          </v-icon>
        </v-badge>
        <span class="hidden-sm-and-down">
          {{ columnSelection ? "Columns & Filters" : "Filters" }}
        </span>
      </v-btn>
    </template>

    <v-card class="c-list-filters--card">
      <v-list density="compact">
        <v-list-item
          v-for="{ prop, filter } in availableProps
            .map((prop) => ({ prop, filter: getFilterInfo(prop) }))
            .filter((x) => (columnSelection ? true : !!x.filter))"
          :key="prop.name"
          @click.stop="
            toggleColumn(prop.name, !selectedColumns?.includes(prop.name))
          "
        >
          <template #prepend v-if="columnSelection">
            <v-checkbox-btn
              density="compact"
              class="mr-2"
              title="Toggle Visibility"
              :model-value="selectedColumns?.includes(prop.name) ?? false"
            />
          </template>
          <v-list-item-title>{{ prop.displayName }}</v-list-item-title>

          <!-- Filter sub-menu -->
          <template #append v-if="filter">
            <v-menu
              location="end"
              :close-on-content-click="false"
              content-class="c-list-filter--prop-menu"
            >
              <template #activator="{ props: submenuProps }">
                <v-divider vertical class="mr-1 my-n1"></v-divider>
                <v-btn
                  v-bind="submenuProps"
                  size="small"
                  height="32"
                  :color="filter.isActive ? 'primary' : undefined"
                  :variant="filter.isActive ? 'flat' : 'text'"
                  class="mr-n3"
                >
                  <v-icon>fa fa-filter</v-icon>
                </v-btn>
              </template>
              <template #default="{ isActive }">
                <v-card class="c-column-filter-card">
                  <v-card-title> Filter: {{ prop.displayName }} </v-card-title>
                  <v-card-text class="py-0">
                    <c-list-filter-input
                      :filter="filter"
                      :list="list"
                      @clear="
                        filter.remove();
                        isActive.value = false;
                      "
                    />
                  </v-card-text>
                </v-card>
              </template>
            </v-menu>
          </template>
        </v-list-item>
        <v-divider></v-divider>
        <v-list-item @click="resetToDefault">
          <template #prepend>
            <v-icon style="margin-left: 2px; margin-right: -21px">
              fa fa-undo
            </v-icon>
          </template>
          <v-list-item-title>Reset</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card>
  </v-menu>
</template>

<style lang="scss">
.c-column-filter-card {
  min-width: 350px;
  max-width: 500px;
}
</style>

<script setup lang="ts">
import { computed, PropType, toRef } from "vue";
import { ListViewModel, ModelType, Property } from "coalesce-vue";
import CListFilterInput from "../input/c-list-filter-input.vue";
import { useListFilters } from "../input/use-list-filters";

const props = defineProps<{
  list: ListViewModel;
  columnSelection?: boolean;
}>();

const selectedColumns = defineModel<string[] | null>("selectedColumns", {
  required: false,
});

const availableProps = computed(() => {
  return Object.values((props.list.$metadata as ModelType).props).filter(
    (p) => {
      if (p.role == "foreignKey" && p.navigationProp) {
        return false;
      }
      return true;
    },
  );
});

// Use the shared filter logic
const { getFilterInfo, activeCount } = useListFilters(toRef(props, "list"));

function toggleColumn(propName: string, selected: boolean) {
  if (!props.columnSelection) return;

  if (selected && !selectedColumns.value?.includes(propName)) {
    const newColumns = [...(selectedColumns.value || []), propName];
    selectedColumns.value = newColumns;
  } else if (!selected && selectedColumns.value) {
    const newColumns = selectedColumns.value.filter((c) => c != propName);
    selectedColumns.value = newColumns;
  }
}

function resetToDefault() {
  if (props.columnSelection) {
    selectedColumns.value = null;
  }
  props.list.$filter = {};
}
</script>
