<template>
  <v-menu>
    <template v-slot:activator="{ props: menuProps }">
      <v-btn
        class="c-admin-table-column-selector"
        variant="text"
        v-bind="menuProps"
        title="Select Columns"
      >
        <v-icon start>fa fa-columns</v-icon>
        <span class="hidden-sm-and-down">Columns</span>
      </v-btn>
    </template>

    <v-card min-width="250" max-width="400">
      <v-card-title class="text-subtitle-1 font-weight-bold">
        Select Columns
      </v-card-title>
      
      <v-card-text>
        <v-checkbox
          v-for="prop in availableProps"
          :key="prop.name"
          :model-value="selectedColumns.includes(prop.name)"
          @update:model-value="(value: boolean | null) => toggleColumn(prop.name, !!value)"
          :label="prop.displayName"
          density="compact"
          hide-details
        />
      </v-card-text>

      <v-card-actions>
        <v-btn @click="selectAll" size="small" variant="text">
          Select All
        </v-btn>
        <v-btn @click="selectNone" size="small" variant="text">
          Select None
        </v-btn>
        <v-spacer />
        <v-btn @click="resetToDefault" size="small" variant="text" color="primary">
          Reset
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-menu>
</template>

<script setup lang="ts">
import { computed, PropType } from "vue";
import { Property } from "coalesce-vue";

const props = defineProps({
  availableProps: { required: true, type: Array as PropType<Property[]> },
  selectedColumns: { required: true, type: Array as PropType<string[]> },
  defaultColumns: { required: true, type: Array as PropType<string[]> },
});

const emit = defineEmits<{
  "update:selectedColumns": [columns: string[]];
}>();

function toggleColumn(propName: string, selected: boolean) {
  const newColumns = [...props.selectedColumns];
  if (selected && !newColumns.includes(propName)) {
    newColumns.push(propName);
  } else if (!selected) {
    const index = newColumns.indexOf(propName);
    if (index >= 0) {
      newColumns.splice(index, 1);
    }
  }
  emit("update:selectedColumns", newColumns);
}

function selectAll() {
  emit("update:selectedColumns", props.availableProps.map(p => p.name));
}

function selectNone() {
  emit("update:selectedColumns", []);
}

function resetToDefault() {
  emit("update:selectedColumns", [...props.defaultColumns]);
}
</script>

<style lang="scss">
.c-admin-table-column-selector {
  min-width: auto;
}
</style>