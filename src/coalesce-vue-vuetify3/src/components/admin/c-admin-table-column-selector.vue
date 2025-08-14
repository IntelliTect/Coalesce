<template>
  <v-menu
    v-model="menuOpen"
    location="bottom end"
    :close-on-content-click="false"
  >
    <template v-slot:activator="{ props: menuProps }">
      <v-btn
        v-bind="menuProps"
        variant="text"
        title="Configure Columns"
        class="c-admin-table-column-selector--button"
        prepend-icon="fa fa-columns"
      >
        Columns
      </v-btn>
    </template>

    <v-card min-width="250" max-width="400">
      <v-card-title class="d-flex align-center justify-space-between">
        <span> Table Columns </span>
      </v-card-title>

      <v-list density="compact">
        <v-list-item
          v-for="column in tablePreferences.availableColumns.value"
          :key="column.name"
          :disabled="column.disabled"
          @click.stop="
            tablePreferences.toggleColumn(
              column.name,
              !tablePreferences.effectiveColumnNames.value.includes(
                column.name,
              ),
            )
          "
        >
          <template v-slot:prepend>
            <v-checkbox-btn
              :model-value="
                tablePreferences.effectiveColumnNames.value.includes(
                  column.name,
                )
              "
              :disabled="column.disabled"
              density="compact"
              class="mr-2"
            />
          </template>
          <v-list-item-title>{{ column.displayName }}</v-list-item-title>
          <v-list-item-subtitle v-if="column.disabled">
            Required
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>

      <v-card-actions>
        <v-btn
          @click="resetToDefaults"
          variant="text"
          prepend-icon="fa fa-undo"
        >
          Reset
        </v-btn>
        <v-spacer />
        <v-btn @click="menuOpen = false" variant="text">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-menu>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { ModelType } from "coalesce-vue";
import { useTablePreferences } from "./useTablePreferences";

const props = defineProps<{
  metadata: ModelType;
  suffix?: string;
  defaultColumns?: string[];
}>();

const menuOpen = ref(false);

// Use the composable directly
const tablePreferences = useTablePreferences({
  metadata: props.metadata,
  suffix: props.suffix,
  defaultColumns: props.defaultColumns,
});

function resetToDefaults() {
  tablePreferences.resetColumnPreferences();
  menuOpen.value = false;
}
</script>

<style lang="scss">
.c-admin-table-column-selector--button {
  min-width: auto;
}
</style>
