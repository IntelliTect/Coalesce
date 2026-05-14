import { App, Component, InjectionKey } from "vue";
import { Domain, Value } from "coalesce-vue";
import { metadataKey } from "./composables/useMetadata";

type MetadataValue = Value & object;

export interface CoalesceVuetifyOptions {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  metadata: Domain;

  /** Components to globally register if not using CoalesceVuetifyResolver */
  components?: Record<string, any>;

  /** Components to use in admin interfaces for specific metadata values. */
  adminOverrides?: {
    /** Replaces c-input for matching metadata values in admin interfaces. */
    input?: ReadonlyMap<MetadataValue, Component>;
    /** Replaces c-admin-display / c-display for matching metadata values in admin interfaces. */
    display?: ReadonlyMap<MetadataValue, Component>;
  };
}

export interface CoalesceVuetifyInstance {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata: Domain;
  readonly adminOverrides: {
    readonly input: ReadonlyMap<MetadataValue, Component>;
    readonly display: ReadonlyMap<MetadataValue, Component>;
  };
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
        adminOverrides: {
          input: adminOverrides?.input ?? new Map(),
          display: adminOverrides?.display ?? new Map(),
        },
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
