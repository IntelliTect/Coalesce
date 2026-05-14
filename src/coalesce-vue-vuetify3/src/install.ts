import { App, Component, InjectionKey } from "vue";
import { Domain, Value } from "coalesce-vue";
import { metadataKey } from "./composables/useMetadata";

type MetadataValue = Value & object;

export interface AdminOverride {
  /** Replaces c-input for this metadata value in admin interfaces. */
  input?: Component;
  /** Replaces c-admin-display for this metadata value in admin interfaces. */
  display?: Component;
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
}

export interface CoalesceVuetifyInstance {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata: Domain;
  readonly adminOverrides: ReadonlyMap<MetadataValue, AdminOverride>;
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
      const { metadata, components = {}, adminOverrides } = options;
      const instance: CoalesceVuetifyInstance = {
        metadata,
        adminOverrides: adminOverrides
          ? adminOverrides instanceof Map
            ? adminOverrides
            : new Map(adminOverrides)
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
