import path from "path";
import { defineConfig } from "vite";

import vue from "@vitejs/plugin-vue";
import Components from "unplugin-vue-components/vite";
import { Vuetify3Resolver } from "unplugin-vue-components/resolvers";

import pkg from "./package.json" with { type: "json" };

export default defineConfig({
  build: {
    minify: false,
    sourcemap: true,
    lib: {
      entry: {
        "vuetify/index": path.resolve(__dirname, "src/vuetify/index.ts"),
      },
      formats: ["es"],
      cssFileName: "styles",
    },
    outDir: "dist",
    rolldownOptions: {
      external: [
        ...Object.keys(pkg.dependencies),
        ...Object.keys(pkg.peerDependencies),
        "virtual:coalesce-vue-docs",
      ].map((pkg) => new RegExp("^" + pkg)),
      output: {
        entryFileNames: "[name].js",
        // Lib mode extracts the CSS without importing it from the JS.
        banner: (chunk) => (chunk.isEntry ? `import "../styles.css";` : ""),
      },
    },
  },
  plugins: [
    vue(),
    Components({
      dts: false,
      resolvers: [Vuetify3Resolver()],
    }),
  ],
  test: {
    globals: true,
    environment: "jsdom",
    include: ["**/*.spec.ts"],
  },
});
