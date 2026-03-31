import path from "path";
import fs from "fs";
import { defineConfig } from "vite";

import vue from "@vitejs/plugin-vue";
import vueJsx from "@vitejs/plugin-vue-jsx";

import Components from "unplugin-vue-components/vite";
import { Vuetify3Resolver } from "unplugin-vue-components/resolvers";

import pkg from "./package.json" with { type: "json" };

const entries: Record<string, string> = {
  index: path.resolve(__dirname, "src/index.ts"),
};
for (const file of fs.globSync("src/**/*.{ts,vue}", {
  exclude: (name) => /\.spec\.|\.d\.ts$|env\.d\.ts$|build\.ts$/.test(name),
})) {
  // e.g. "src/components/admin/c-admin-table.vue" -> "components/admin/c-admin-table"
  const key = file
    .replaceAll("\\", "/")
    .replace(/^src\//, "")
    .replace(/\.(ts|vue)$/, "");
  if (key !== "index") {
    entries[key] = path.resolve(__dirname, file);
  }
}

export default defineConfig({
  build: {
    minify: false,
    sourcemap: true,
    lib: {
      entry: entries,
      formats: ["es"],
    },
    outDir: "dist",
    rolldownOptions: {
      preserveEntrySignatures: "allow-extension",
      external: [
        ...Object.keys(pkg.dependencies),
        ...Object.keys(pkg.peerDependencies),
        ...Object.keys(pkg.optionalDependencies),
      ].map((pkg) => new RegExp("^" + pkg)),
      output: {
        assetFileNames: `coalesce-vue-vuetify.[ext]`,
        entryFileNames: "[name].js",
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
