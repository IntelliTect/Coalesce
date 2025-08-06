import {
  BehaviorFlags,
  ListViewModel,
  ModelType,
  ViewModel,
  mapParamsToDto,
} from "coalesce-vue";
import { MaybeRef, unref, computed } from "vue";
import { useRouter } from "vue-router";

export function useAdminTable(list: MaybeRef<ListViewModel>) {
  const metadata = computed((): ModelType => unref(list).$metadata);

  const creatableTypes = computed(() => {
    const meta = metadata.value;
    if (!meta || unref(list).$modelOnlyMode) return [];

    const types = [meta, ...(meta.derivedTypes ?? [])];
    return types
      .filter((type) => (type.behaviorFlags & BehaviorFlags.Create) != 0)
      .map((type) => ({
        metadata: type,
        route: getItemRoute(undefined, type),
      }));
  });

  const canCreate = computed(() => {
    return !!creatableTypes.value.length;
  });

  const canEdit = computed(() => {
    const meta = metadata.value;
    return (
      meta &&
      (meta.behaviorFlags & BehaviorFlags.Edit) != 0 &&
      !unref(list).$modelOnlyMode
    );
  });

  const canDelete = computed(() => {
    const meta = metadata.value;
    return (
      meta &&
      (meta.behaviorFlags & BehaviorFlags.Delete) != 0 &&
      !unref(list).$modelOnlyMode
    );
  });

  const hasInstanceMethods = computed(() => {
    const meta = metadata.value;
    return (
      meta &&
      Object.values(meta.methods).some((m) => !m.isStatic) &&
      !unref(list).$modelOnlyMode
    );
  });

  const router = useRouter();

  function getItemRoute(item?: ViewModel, meta?: ModelType) {
    // Resolve to an href to allow overriding of admin routes in userspace.
    // If we just gave a named raw location, it would always use the coalesce admin route
    // instead of the user-overridden one (that the user overrides by declaring another
    // route with the same path).
    try {
      // Will throw if coalesce-admin-item is not a mapped route.
      return router.resolve({
        name: "coalesce-admin-item",
        params: {
          type: (item?.$metadata ?? meta ?? metadata.value).name,
          id: item?.$primaryKey,
        },
        query: Object.fromEntries(
          Object.entries(mapParamsToDto(unref(list).$params) || {}).filter(
            (entry) =>
              entry[0].startsWith("filter.") ||
              entry[0].startsWith("dataSource"),
          ),
        ),
      }).fullPath;
    } catch {
      return undefined;
    }
  }

  return {
    metadata,
    creatableTypes,
    canCreate,
    canEdit,
    canDelete,
    hasInstanceMethods,
    getItemRoute,
  };
}
