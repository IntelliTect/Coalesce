import path from "path";
import { defineConfig } from "vite";
import dts from "vite-plugin-dts";
import vue from "@vitejs/plugin-vue";

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
        ...Object.keys(pkg.dependencies),
        ...Object.keys(pkg.peerDependencies),
        ...Object.keys(pkg.optionalDependencies),
      ].map((pkg) => new RegExp("^" + pkg)),
      output: {
        assetFileNames: `coalesce-vue-vuetify.[ext]`,
      },
    },
  },
  plugins: [
    vue(),
    dts({
      entryRoot: "src",
      // logDiagnostics: true,
      // skipDiagnostics: false,
      // noEmitOnError: true,
      staticImport: true,
      compilerOptions: {
        skipLibCheck: true,
      },
    }),
    Components({
      dts: true,
      resolvers: [Vuetify3Resolver()],
      extensions: ["vue", "ts"],
      include: [/\.vue$/, /\.ts$/, /\.vue\?vue/],
    }),
  ],
});
