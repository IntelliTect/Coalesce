import { App, Component, InjectionKey } from "vue";
import { Domain, ModelType, ObjectType, Value } from "coalesce-vue";
import { metadataKey } from "./composables/useMetadata";

type MetadataValue = Value & object;
type AdminExtensionKey = ModelType | ObjectType | "*";

export interface AdminOverride {
  /** Replaces c-input for this metadata value in admin interfaces. */
  input?: Component;
  /** Replaces c-admin-display for this metadata value in admin interfaces. */
  display?: Component;
}

export interface AdminExtension {
  /** Component rendered after the built-in buttons in c-admin-table-toolbar.
   * Receives `list: ListViewModel` and `editable: boolean | null` as props. */
  tableToolbarActions?: Component;

  /** Component rendered in the c-admin-editor toolbar, before the Save/Delete/Reload buttons.
   * Receives `model: ViewModel` and `editable: boolean` as props. */
  editorToolbarActions?: Component;

  /** Component rendered in the c-admin-editor card-actions area, before the Save button.
   * Receives `model: ViewModel` and `editable: boolean` as props. */
  editorActions?: Component;

  /** Component rendered in each row's actions column in c-admin-table,
   * alongside the Edit/Delete buttons.
   * Receives `model: ViewModel`, `list: ListViewModel`, and `editable: boolean` as props. */
  tableRowActions?: Component;

  /** Component rendered before the table in c-admin-table-page.
   * Receives `list: ListViewModel` as a prop. */
  tablePageHeader?: Component;

  /** Component rendered before the editor in c-admin-editor-page.
   * Receives `model: ViewModel` as a prop. */
  editorPageHeader?: Component;
}

export interface CoalesceVuetifyOptions {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  metadata: Domain;

  /** Components to globally register if not using CoalesceVuetifyResolver */
  components?: Record<string, any>;

  /** Components to use in admin interfaces for specific metadata values. */
  adminOverrides?:
    | ReadonlyMap<MetadataValue, AdminOverride>
    | ReadonlyArray<readonly [MetadataValue, AdminOverride]>;

  /** Extensions to admin interfaces for specific model types, or "*" for a global default. */
  adminExtensions?:
    | ReadonlyMap<AdminExtensionKey, AdminExtension>
    | ReadonlyArray<readonly [AdminExtensionKey, AdminExtension]>;
}

export interface CoalesceVuetifyInstance {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata: Domain;
  readonly adminOverrides: ReadonlyMap<MetadataValue, AdminOverride>;
  readonly adminExtensions: ReadonlyMap<AdminExtensionKey, AdminExtension>;
}

export const coalesceVuetifyKey: InjectionKey<CoalesceVuetifyInstance> =
  Symbol("coalesceVuetify");

declare module "vue" {
  export interface ComponentCustomProperties {
    readonly $coalesce: CoalesceVuetifyInstance;
  }
}

export const createCoalesceVuetify = (options: CoalesceVuetifyOptions) => {
  return {
    install(app: App) {
      const {
        metadata,
        components = {},
        adminOverrides,
        adminExtensions,
      } = options;
      const instance: CoalesceVuetifyInstance = {
        metadata,
        adminOverrides: adminOverrides
          ? adminOverrides instanceof Map
            ? adminOverrides
            : new Map(adminOverrides)
          : new Map(),
        adminExtensions: adminExtensions
          ? adminExtensions instanceof Map
            ? adminExtensions
            : new Map(adminExtensions)
          : new Map(),
      };

      for (const key in components) {
        app.component(key, components[key]);
      }

      app.provide(metadataKey, metadata);
      app.provide(coalesceVuetifyKey, instance);

      app.mixin({
        computed: {
          $coalesce() {
            return instance;
          },
        },
      });
    },
  };
};
