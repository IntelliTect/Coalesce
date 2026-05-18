import { inject, type Component } from "vue";
import type { Value } from "coalesce-vue";
import { coalesceVuetifyKey } from "../install";
import CInput from "../components/input/c-input.vue";
import CAdminDisplay from "../components/admin/c-admin-display.vue";

export function useAdminOverrides() {
  const coalesce = inject(coalesceVuetifyKey, null);
  return {
    resolveAdminInputComponent(value: Value): Component {
      return (
        // Silly `as unknown` to avoid a "type instantiation too deep"
        coalesce?.adminOverrides.get(value)?.input ??
        (CInput as unknown as Component)
      );
    },
    resolveAdminDisplayComponent(value: Value): Component {
      return coalesce?.adminOverrides.get(value)?.display ?? CAdminDisplay;
    },
  };
}
