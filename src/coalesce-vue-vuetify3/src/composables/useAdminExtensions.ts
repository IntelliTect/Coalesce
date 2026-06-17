import { inject, type Component } from "vue";
import type { ModelType, ObjectType } from "coalesce-vue";
import { coalesceVuetifyKey, type AdminExtension } from "../install";

export function useAdminExtensions() {
  const coalesce = inject(coalesceVuetifyKey, null);

  return {
    resolve(
      type: ModelType | ObjectType,
      field: keyof AdminExtension,
    ): Component | undefined {
      return (
        coalesce?.adminExtensions.get(type)?.[field] ??
        coalesce?.adminExtensions.get("*")?.[field]
      );
    },
  };
}
