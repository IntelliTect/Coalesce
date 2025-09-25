<template>
  <div class="c-filter-row">
    <!-- Null/Not Null toggle for all fields -->
    <div class="mb-3">
      <v-btn-group variant="outlined" density="compact">
        <v-btn
          size="small"
          :variant="!filter.isNull ? 'flat' : 'outlined'"
          :color="!filter.isNull ? 'primary' : undefined"
          @click="filter.value = ''"
        >
          Has Value
        </v-btn>
        <v-btn
          size="small"
          :variant="filter.isNull ? 'flat' : 'outlined'"
          :color="filter.isNull ? 'primary' : undefined"
          @click="filter.value = null"
        >
          No Value
        </v-btn>
      </v-btn-group>
      <v-btn-group variant="outlined" density="compact" class="ml-2">
        <v-btn
          size="small"
          variant="outlined"
          prepend-icon="$close"
          color="error"
          @click="emit('clear')"
          title="Remove this filter"
        >
          Remove
        </v-btn>
      </v-btn-group>
    </div>

    <!-- Value input - only show when not filtering for null -->
    <div v-if="!filter.isNull">
      <!-- Boolean filter - dropdown -->
      <v-select
        v-if="filter.propMeta && filter.propMeta.type == 'boolean'"
        v-model="filter.value"
        :items="[
          { title: 'True', value: 'true' },
          { title: 'False', value: 'false' },
        ]"
        placeholder="Select true or false..."
        clearable
        autofocus
        hide-details
        density="compact"
        variant="outlined"
      />

      <!-- Foreign key / primary key / reference navigation dropdown -->
      <c-select
        v-else-if="
          filter.propMeta &&
          (filter.propMeta.role == 'foreignKey' ||
            filter.propMeta.role == 'primaryKey' ||
            filter.propMeta.role == 'referenceNavigation')
        "
        v-model:keyValue="filter.value"
        :for="
          filter.propMeta.role == 'primaryKey'
            ? list.$metadata.name
            : filter.propMeta.role == 'referenceNavigation'
              ? filter.propMeta
              : (filter.propMeta.navigationProp ??
                filter.propMeta.principalType)
        "
        clearable
        :multiple="filter.propMeta.type != 'string'"
        autofocus
        hide-details
        density="compact"
        variant="outlined"
      />

      <c-input
        v-else-if="filter.propMeta"
        :for="filter.propMeta"
        v-model="filter.value"
        label=""
        placeholder="Enter filter value..."
        density="compact"
        variant="outlined"
        clearable
        autofocus
        :hint="getInputHint(filter.propMeta)"
        persistent-hint
      />

      <!-- Text field for everything else -->
      <v-text-field
        v-else
        v-model="filter.value"
        placeholder="Enter filter value..."
        density="compact"
        variant="outlined"
        clearable
        autofocus
        :hint="getInputHint(filter.propMeta)"
        persistent-hint
      />
    </div>

    <!-- Null state message -->
    <div v-else class="font-italic" style="min-height: 40px">
      Showing records where {{ filter.displayName }} has no value
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ListViewModel } from "coalesce-vue";
import type { FilterInfo } from "./use-list-filters";

defineProps<{
  filter: FilterInfo;
  list: ListViewModel;
}>();

const emit = defineEmits<{
  clear: [];
}>();

function getInputHint(propMeta: any): string {
  if (!propMeta) return "";

  switch (propMeta.type) {
    case "string":
      return "Append an asterisk (*) for partial matches";
    case "date":
      return "Dates without time match the entire day";
    default:
      return "";
  }
}
</script>

<style lang="scss" scoped>
.c-filter-row {
  padding: 8px 0;
}
</style>
