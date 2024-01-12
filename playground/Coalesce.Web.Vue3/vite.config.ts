import path from "path";

import { defineConfig } from "vite";

import createVuePlugin from "@vitejs/plugin-vue";
import vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import createVueComponentImporterPlugin from "unplugin-vue-components/vite";
import { Vuetify3Resolver } from "unplugin-vue-components/resolvers";

import { createAspNetCoreHmrPlugin } from "../../src/coalesce-vue/src/build";
import { CoalesceVuetifyResolver } from "../../src/coalesce-vue-vuetify3/src/build";

const libRoot = path.resolve(__dirname, "../../src/") + "/";

export default defineConfig(async ({ command, mode }) => {
  return {
    build: {
      outDir: "wwwroot",
      rollupOptions: {
        output: {
          manualChunks(id) {
            if (id.match(/home/i)) return "index";
            // All views are chunked together so that dynamic imports can be
            // used in `router.ts`(which makes for a much more readable file).
            // Without this, each dynamic import would get its own chunk.
            if (id.includes("views")) return "views";
            return "index";
          },
        },
      },
    },

    plugins: [
      (await import("vite-plugin-inspect")).default(),
      
      createVuePlugin({
        template: { transformAssetUrls },
      }),

      vuetify({
        autoImport: true,
      }),

      // Transforms usages of Vuetify and Coalesce components into treeshakable imports
      createVueComponentImporterPlugin({
        dts: false,
        // DONT USE CoalesceVuetifyResolver because it will inject imports into component sources
        // that will import from coalesce-vue-vuetify3's index.ts,
        // which then creates circular imports that break HMR.
        // Instead, give the plugin the actual source paths
        // so they can be imported directly from their exact locations.
        globs: [
          "src/components/**/*.{vue}",
          libRoot + "coalesce-vue-vuetify3/src/components/**/*.{vue,ts}",
        ],
      }),

      // Integrations with UseViteDevelopmentServer from IntelliTect.Coalesce.Vue
      createAspNetCoreHmrPlugin(),

      // Perform type checking during development and build time.
      // createCheckerPlugin({
      //   // VLS: Vue Language Server, the language server portion of Vetur.
      //   vls: {
      //     vetur: {
      //       // Template validation is turned off because this mirrors the classic behavior of vue-cli.
      //       // However, modern syntax like the null propagating operator IS available in this configuration,
      //       // so these options can be turned on if desired.
      //       validation: {
      //         template: false,
      //         templateProps: false,
      //         interpolation: false,
      //       },
      //     },
      //   },
      // }),
    ],

    resolve: {
      dedupe: ["vue", "vue-router", "vuetify"],
      alias: [
        { find: "@", replacement: path.resolve(__dirname, "src") },
        {
          find: "coalesce-vue/lib",
          replacement: libRoot + "coalesce-vue/src",
        },
        {
          find: "coalesce-vue",
          replacement: libRoot + "coalesce-vue/src",
        },
        {
          find: "coalesce-vue-vuetify3",
          replacement: libRoot + "coalesce-vue-vuetify3/src/index.ts",
        },
      ],
    },
    server: {
      host: "0.0.0.0",
      fs: {
        allow: [
          libRoot + "coalesce-vue",
          libRoot + "coalesce-vue-vuetify3",
          ".",
        ],
      },
    },

    esbuild: {
      // vue-class-component uses the original names of classes as the component name.
      keepNames: true,
    },

    optimizeDeps: {
      include: ["vuetify"],
    },

    // test: <VitestInlineConfig>{
    //   globals: true,
    //   environment: "jsdom",
    //   setupFiles: ["tests/setupTests.ts"],
    //   coverage: {
    //     exclude: ["**/*.g.ts", "test{,s}/**"],
    //   },
    //   deps: {
    //     inline: ["vuetify"],
    //   },
    // },
  };
});
