import { computed, Ref } from "vue";
import type { ListViewModel, Property } from "coalesce-vue";

export interface FilterInfo {
  key: string;
  displayName: string;
  isDefined: boolean;
  isActive: boolean;
  isNull: boolean;
  value?: any;
  propMeta?: Property;
  remove(): void;
}

const filterTypes = ["string", "number", "boolean", "enum", "date"];

export function useListFilters(list: Ref<ListViewModel | undefined | null>) {
  // Compute filters
  const filters = computed((): FilterInfo[] => {
    if (!list.value) return [];

    const listValue = list.value;
    const filter = listValue.$params.filter ?? {};
    const meta = listValue.$metadata;

    // Start with the set of valid filter properties
    const filterNames = (Object.values(meta.props) as Property[])
      .filter((p) => filterTypes.includes(p.type) && !p.dontSerialize)
      .map((p) => p.name);

    // Add in any actually existing filters that aren't represented by a prop.
    // This is unlikely, but someone can write a custom datasource that can look for any filter prop.
    for (const key in filter) {
      if (!filterNames.includes(key)) filterNames.push(key);
    }

    return filterNames
      .map((key) => {
        const value = filter[key];
        const propMeta = meta.props[key] ?? null;

        return {
          key,
          get value() {
            if (propMeta.type == "boolean") {
              if (value === "true") return true;
              if (value === "false") return false;
            }
            if (propMeta?.type == "enum" || propMeta?.type == "number") {
              // Enums use a real enum multiselect control, so we need to get our type ducks in a row
              if (filter[key] == null || filter[key] === undefined) {
                return [];
              }
              return String(filter[key])
                .split(",")
                .filter((v) => v !== "")
                .map((v) => (isNaN(v as any) ? v : +v));
            }
            return filter[key];
          },
          set value(value) {
            if (Array.isArray(value)) {
              value = value.join(",");
            }
            (listValue.$params.filter ??= {})[key] = value;
          },
          propMeta,
          isNull: value === null || value === "null",
          isDefined: key in filter,
          // Both undefined and emptystring are considered by Coalesce to be "not filtered".
          // `null` as a value is a filter that checks that the value is `null`.
          isActive: value !== "" && value !== undefined,
          displayName:
            (propMeta?.role == "foreignKey"
              ? propMeta.navigationProp?.displayName
              : undefined) ??
            propMeta?.displayName ??
            key,

          remove() {
            delete listValue.$params.filter![key];
          },
        } as FilterInfo;
      })
      .sort((a, b) => a.displayName.localeCompare(b.displayName));
  });

  const activeFilters = computed(() =>
    filters.value.filter((info) => info.isActive),
  );
  const activeCount = computed(() => activeFilters.value.length);

  function getFilterInfo(prop: Property | string): FilterInfo | undefined {
    if (typeof prop == "object") {
      if (prop.role == "referenceNavigation") {
        prop = prop.foreignKey;
      }
      prop = prop.name;
    }
    return filters.value.find((f) => f.key === prop);
  }

  return {
    filters,
    activeFilters,
    activeCount,
    getFilterInfo,
  };
}
