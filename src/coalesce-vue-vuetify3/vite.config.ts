import path from "path";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import vueJsx from "@vitejs/plugin-vue-jsx";

import Components from "unplugin-vue-components/vite";
import { Vuetify3Resolver } from "unplugin-vue-components/resolvers";

import pkg from "./package.json";

export default defineConfig({
  build: {
    minify: false,
    sourcemap: true,
    lib: {
      entry: path.resolve(__dirname, "src/index.ts"),
      formats: ["es"],
    },
    outDir: "dist",
    rollupOptions: {
      external: [
        // ...Object.keys(pkg.dependencies),
        ...Object.keys(pkg.peerDependencies),
        ...Object.keys(pkg.optionalDependencies),
      ].map((pkg) => new RegExp("^" + pkg)),
      output: {
        assetFileNames: `coalesce-vue-vuetify.[ext]`,
      },
    },
  },
  resolve: {
    alias: [
      { find: "@", replacement: path.resolve(__dirname, "src") },
      { find: "@test", replacement: path.resolve(__dirname, "test") },
      {
        find: "@test-targets",
        replacement: path.resolve(__dirname, "../test-targets"),
      },
      // Imports for generated test targets
      {
        find: "coalesce-vue/lib",
        replacement: path.resolve(__dirname, "../coalesce-vue/src"),
      },
      {
        find: "coalesce-vue",
        replacement: path.resolve(__dirname, "../coalesce-vue/src"),
      },
    ],
  },
  plugins: [
    vue(),
    vueJsx(),

    Components({
      dts: false,
      resolvers: [Vuetify3Resolver()],
      extensions: ["vue", "ts"],
      include: [/\.vue$/, /\.ts$/, /\.vue\?vue/],
    }),
  ],
  test: {
    globals: true,
    environment: "jsdom",
    include: ["**/*.spec.{ts,tsx}"],
    server: {
      deps: {
        inline: [/vuetify/],
      },
    },
  },
});
