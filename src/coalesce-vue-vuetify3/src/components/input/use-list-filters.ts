import { computed, Ref } from "vue";
import {
  mapToDto,
  ModelReferenceNavigationProperty,
  parseValue,
  type ListViewModel,
  type ModelType,
  type Property,
} from "coalesce-vue";

export interface FilterInfo {
  key: string;
  selectFor: ModelType | ModelReferenceNavigationProperty | null;
  selectForMultiple: boolean;
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
    const meta: ModelType = listValue.$metadata;

    // Start with the set of valid filter properties
    const filterNames = (Object.values(meta.props) as Property[])
      .filter(
        (p) =>
          !p.dontSerialize &&
          (filterTypes.includes(p.type) ||
            (p.type == "collection" && filterTypes.includes(p.itemType.type))),
      )
      .map((p) => p.name);

    // Add in any actually existing filters that aren't represented by a prop.
    // This is unlikely, but someone can write a custom datasource that can look for any filter prop.
    for (const key in filter) {
      if (!filterNames.includes(key)) filterNames.push(key);
    }

    return filterNames
      .map((key) => {
        const value = filter[key];
        let propMeta = meta.props[key] ?? null;

        if (propMeta) {
          if (
            propMeta?.role == "value" &&
            (propMeta?.type == "enum" || propMeta?.type == "number")
          ) {
            // single enums and numbers accept CSV collections
            propMeta = {
              ...propMeta,
              type: "collection",
              itemType: propMeta,
            } as any;
          } else if (propMeta?.type == "date" && propMeta.role == "value") {
            // We don't use dates as datepickers at the moment
            // because date filter behavior uses complex filtering that allows
            // a time to either be included or excluded, which can't be done with a picker.
            propMeta = {
              ...propMeta,
              type: "string",
            } as any;
          }
          propMeta = {
            ...propMeta,
            rules: undefined,
            // If createOnly is true, it changes input to only accept when the field blurs
            createOnly: false,
          } as any;
        }

        const selectFor = !propMeta
          ? null
          : propMeta.role == "primaryKey"
            ? meta
            : propMeta.role == "referenceNavigation"
              ? propMeta
              : propMeta.role == "foreignKey"
                ? (propMeta.navigationProp ?? propMeta.principalType)
                : null;

        const selectForMultiple =
          !!selectFor &&
          // Multiple disabled in string keys for now because commas are ambiguous.
          // Is this restriction not useful since real world keys are unlikely to contain commas?
          propMeta.type != "string" &&
          // For dates, only System.DateOnly supports CSV on the backend.
          // This check isn't perfect because of DateKindAttribute, but its close enough.
          !(propMeta.type == "date" && propMeta.dateKind != "date");

        return {
          key,
          selectFor,
          selectForMultiple,
          get value() {
            if (propMeta) {
              if (selectForMultiple && typeof value == "string") {
                return value
                  .split(",")
                  .filter((x) => x)
                  .map((v) => parseValue(v, propMeta));
              }
              return parseValue(value, propMeta) as any;
            }
            return filter[key];
          },
          set value(value) {
            if (selectFor && value) {
              // If this is a key-like value, it could be a date, which requires correct mapping.
              if (Array.isArray(value)) {
                value = value.map((v) => mapToDto(v, propMeta));
              } else {
                value = mapToDto(value, propMeta);
              }
            }

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
