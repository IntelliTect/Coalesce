import { App } from "vue";
import { Domain } from "coalesce-vue";
import { metadataKey } from "./composables/useMetadata";

export interface CoalesceVuetifyOptions {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  metadata: Domain;

  /** Components to globally register if not using CoalesceVuetifyResolver */
  components?: Record<string, any>;
}

export interface CoalesceVuetifyInstance {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata: Domain;
}

declare module "@vue/runtime-core" {
  export interface ComponentCustomProperties {
    readonly $coalesce: CoalesceVuetifyInstance;
  }
}

export const createCoalesceVuetify = (options: CoalesceVuetifyOptions) => {
  return {
    install(app: App) {
      const { metadata, components = {} } = options;

      for (const key in components) {
        app.component(key, components[key]);
      }

      app.provide(metadataKey, metadata);

      app.mixin({
        computed: {
          $coalesce() {
            return <CoalesceVuetifyInstance>{
              metadata,
            };
          },
        },
      });
    },
  };
};
