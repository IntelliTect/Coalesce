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

    <v-card>
      <!-- <v-btn @click="selectAll" size="small" variant="text">
          Select All
        </v-btn>
        <v-btn @click="selectNone" size="small" variant="text">
          Select None
        </v-btn>
        <v-spacer /> -->
      <v-list density="compact">
        <v-list-item
          v-for="prop in availableProps"
          :key="prop.name"
          @click.stop="
            toggleColumn(prop.name, !selectedColumns.includes(prop.name))
          "
        >
          <template v-slot:prepend>
            <v-checkbox-btn
              density="compact"
              class="mr-2"
              :model-value="selectedColumns.includes(prop.name)"
            />
          </template>
          <v-list-item-title>{{ prop.displayName }}</v-list-item-title>
        </v-list-item>
      </v-list>
      <v-divider></v-divider>
      <v-btn
        @click="resetToDefault"
        prepend-icon="fa fa-undo"
        class="ml-3 my-1"
        variant="text"
      >
        Reset
      </v-btn>
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
  if (selected && !props.selectedColumns.includes(propName)) {
    // Insert the column in the correct position based on availableProps order
    const availableOrder = props.availableProps.map((p) => p.name);
    const newColumns = [...props.selectedColumns, propName].sort(
      (a, b) => availableOrder.indexOf(a) - availableOrder.indexOf(b),
    );
    emit("update:selectedColumns", newColumns);
  } else if (!selected) {
    const newColumns = props.selectedColumns.filter((c) => c != propName);
    emit("update:selectedColumns", newColumns);
  }
}

function selectAll() {
  emit(
    "update:selectedColumns",
    props.availableProps.map((p) => p.name),
  );
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
