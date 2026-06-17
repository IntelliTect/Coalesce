import { inject, type Component } from "vue";
import type { ModelType, ObjectType } from "coalesce-vue";
import { coalesceVuetifyKey } from "../install";

export function useAdminExtensions() {
  const coalesce = inject(coalesceVuetifyKey, null);

  function resolve(
    type: ModelType | ObjectType,
    field: "tableToolbarActions" | "editorToolbarActions" | "editorActions" | "tableRowActions" | "tablePageHeader" | "editorPageHeader",
  ): Component | undefined {
    return (
      coalesce?.adminExtensions.get(type)?.[field] ??
      coalesce?.adminExtensions.get("*")?.[field]
    );
  }

  return {
    resolveTableToolbarActions(type: ModelType | ObjectType) {
      return resolve(type, "tableToolbarActions");
    },
    resolveEditorToolbarActions(type: ModelType | ObjectType) {
      return resolve(type, "editorToolbarActions");
    },
    resolveEditorActions(type: ModelType | ObjectType) {
      return resolve(type, "editorActions");
    },
    resolveTableRowActions(type: ModelType | ObjectType) {
      return resolve(type, "tableRowActions");
    },
    resolveTablePageHeader(type: ModelType | ObjectType) {
      return resolve(type, "tablePageHeader");
    },
    resolveEditorPageHeader(type: ModelType | ObjectType) {
      return resolve(type, "editorPageHeader");
    },
  };
}
