import { inject, type Component } from "vue";
import type { ModelType, ObjectType } from "coalesce-vue";
import { coalesceVuetifyKey } from "../install";

export function useAdminExtensions() {
  const coalesce = inject(coalesceVuetifyKey, null);
  return {
    resolveTableToolbarActions(
      type: ModelType | ObjectType,
    ): Component | undefined {
      return (
        coalesce?.adminExtensions.get(type)?.tableToolbarActions ??
        coalesce?.adminExtensions.get("*")?.tableToolbarActions
      );
    },
  };
}
