import { computed } from "vue";
import { useLocalStorage } from "@vueuse/core";
import { ModelType, Property, HiddenAreas } from "coalesce-vue";

export interface ColumnPreferences {
  [columnName: string]: boolean;
}

export interface TablePreferences {
  columns?: ColumnPreferences;
}

export interface UseTablePreferencesOptions {
  /** The model metadata for generating the storage key */
  metadata: ModelType;
  /** Optional storage key suffix for uniqueness */
  suffix?: string;
  /** Default columns to show if no preferences are saved */
  defaultColumns?: string[];
}

export function useTablePreferences(options: UseTablePreferencesOptions) {
  const { metadata, suffix, defaultColumns } = options;

  // Create a unique storage key based on model name and optional suffix
  const storageKey = computed(() => {
    const base = `coalesce-admin-table-${metadata.name}`;
    return suffix ? `${base}-${suffix}` : base;
  });

  // Use VueUse's useLocalStorage for reactive localStorage
  const preferences = useLocalStorage<TablePreferences>(storageKey.value, {});

  // Get all available columns (properties that can be displayed in a table)
  const availableColumns = computed(() => {
    return Object.values(metadata.props)
      .filter((p: Property) => {
        // Exclude hidden columns and primary keys
        if (p.hidden !== undefined && (p.hidden & HiddenAreas.List) !== 0) {
          return false;
        }
        if (p.role === "primaryKey") {
          return false;
        }
        return true;
      })
      .map((p: Property) => ({
        name: p.name,
        displayName: p.displayName,
        property: p,
        disabled: false, // Could add logic for mandatory columns here
      }));
  });

  // Get the effective columns based on preferences and defaults
  const effectiveColumns = computed(() => {
    const prefs = preferences.value.columns;

    return availableColumns.value.filter((col) => {
      // Check user preferences first
      if (prefs && prefs[col.name] !== undefined) {
        return prefs[col.name];
      }

      // Fall back to defaults if provided
      if (defaultColumns) {
        return defaultColumns.includes(col.name);
      }
      0;
      // Default to showing all available columns
      return true;
    });
  });

  // Get effective column names as string array (for c-table props)
  const effectiveColumnNames = computed(() => {
    return effectiveColumns.value.map((col) => col.name);
  });

  // Get effective column properties (for c-table internal use)
  const effectiveColumnProperties = computed(() => {
    return effectiveColumns.value.map((col) => col.property);
  });

  function toggleColumn(columnName: string, selected: boolean) {
    const updatedColumns = {
      ...preferences.value.columns,
      [columnName]: selected,
    };

    preferences.value = {
      ...preferences.value,
      columns: updatedColumns,
    };
  }

  function resetColumnPreferences() {
    const updated = { ...preferences.value };
    delete updated.columns;
    preferences.value = updated;
  }

  function updatePreferences(newPreferences: TablePreferences) {
    preferences.value = newPreferences;
  }

  return {
    preferences,
    availableColumns,
    effectiveColumns,
    effectiveColumnNames,
    effectiveColumnProperties,
    toggleColumn,
    resetColumnPreferences,
    updatePreferences,
  };
}
