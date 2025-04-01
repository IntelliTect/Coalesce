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

  const canCreate = computed(() => {
    const meta = metadata.value;
    return (
      meta &&
      (meta.behaviorFlags & BehaviorFlags.Create) != 0 &&
      !unref(list).$modelOnlyMode
    );
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

  function getItemRoute(item?: ViewModel) {
    // Resolve to an href to allow overriding of admin routes in userspace.
    // If we just gave a named raw location, it would always use the coalesce admin route
    // instead of the user-overridden one (that the user overrides by declaring another
    // route with the same path).
    return router.resolve({
      name: "coalesce-admin-item",
      params: {
        type: item?.$metadata.name ?? metadata.value.name,
        id: item?.$primaryKey,
      },
      query: Object.fromEntries(
        Object.entries(mapParamsToDto(unref(list).$params) || {}).filter(
          (entry) =>
            entry[0].startsWith("filter.") || entry[0].startsWith("dataSource")
        )
      ),
    }).fullPath;
  }

  return {
    metadata,
    canCreate,
    canEdit,
    canDelete,
    hasInstanceMethods,
    getItemRoute,
  };
}
