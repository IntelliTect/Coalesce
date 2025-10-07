import path from "path";

import { defineConfig } from "vite";

import createVuePlugin from "@vitejs/plugin-vue";
import vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import createVueComponentImporterPlugin from "unplugin-vue-components/vite";
import { Vuetify3Resolver } from "unplugin-vue-components/resolvers";

import { createAspNetCoreHmrPlugin } from "../../src/coalesce-vue/src/build";
import { CoalesceVuetifyResolver } from "../../src/coalesce-vue-vuetify3/src/build";

const libRoot = path.resolve(__dirname, "../../src/") + "/";

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
      // repo root, where some NPM packages may be restored to
      allow: [path.resolve(__dirname, "../../")],
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
});
