import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";

import createVuePlugin from "@vitejs/plugin-vue";
import { createAspNetCoreHmrPlugin } from "coalesce-vue/lib/build";
import createAutoImport from "unplugin-auto-import/vite";

import createVueComponentImporterPlugin from "unplugin-vue-components/vite";
import { CoalesceVuetifyResolver } from "coalesce-vue-vuetify3/build";
import createVuetify, { transformAssetUrls } from "vite-plugin-vuetify";

export default defineConfig({
  build: {
    outDir: "wwwroot",
    rollupOptions: {
      output: {
        manualChunks(id) {
          // Chunk all styles together so that there aren't problems
          // with selectors getting reordered, which alters specificity.
          if (/\.s?css|type=style/.test(id)) return "styles";

          // Workers can't be chunked with other things
          if (id.includes("worker")) return undefined;

          if (id.match(/vuetify/)) return "vuetify";
          if (id.match(/node_modules/)) return "vendor";
          return "index";
        },
      },
    },
  },

  plugins: [
    createVuePlugin({
      template: { transformAssetUrls },
    }),

    // Integrations with UseViteDevelopmentServer from IntelliTect.Coalesce.Vue:
    createAspNetCoreHmrPlugin(),

    // Transforms usages of Vuetify and Coalesce components into treeshakable imports.
    // Vuetify3Resolver could be removed and replaced by vite-plugin-vuetify if desired.
    createVueComponentImporterPlugin({
      resolvers: [CoalesceVuetifyResolver()],
      dts: "src/types/components.d.ts",
    }),

    // Auto-import vue composition API functions,
    // and any custom composables in the src/composables directory:
    createAutoImport({
      imports: ["vue", "vue-router"],
      dirs: ["src/composables/*"],
      dts: "src/types/auto-imports.d.ts",
    }),

    // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
    createVuetify({
      autoImport: true,
      // OPTIONAL: https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#style-loading
      // Customize vuetify's styles with sass variables
      // styles: { configFile: "src/styles/settings.scss" },
    }),

    {
      name: "inject-build-date",
      transformIndexHtml() {
        return [
          {
            tag: "script",
            children: `BUILD_DATE = new Date('${new Date().toISOString()}');`,
          },
        ];
      },
    },
  ],

  resolve: {
    alias: {
      // Allow imports prefixed with "@" to be relative to the src folder.
      "@": fileURLToPath(new URL("src", import.meta.url)),
    },
  },

  css: {
    preprocessorOptions: {
      scss: {
        api: "modern",
      },
      sass: {
        api: "modern",
      },
    },
  },

  test: {
    globals: true,
    environment: "jsdom",
    coverage: {
      provider: "v8",
      exclude: ["**/*.g.ts", "**/*.spec.*", "test{,s}/**"],
    },
    server: {
      deps: {
        inline: [/vuetify/],
      },
    },
  },
});
