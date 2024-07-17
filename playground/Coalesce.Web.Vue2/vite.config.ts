import path from "path";

import { defineConfig } from "vite";

import createVuePlugin from "@vitejs/plugin-vue2";
import createCheckerPlugin from "vite-plugin-checker";
import createVueComponentImporterPlugin from "unplugin-vue-components/vite";
import { VuetifyResolver } from "unplugin-vue-components/resolvers";

import { createAspNetCoreHmrPlugin } from '../../src/coalesce-vue/src/build'
//const { CoalesceVuetifyResolver } = jitiRequire('../../src/coalesce-vue-vuetify2/src/build.ts')

import { sassPlugin } from "esbuild-sass-plugin";

import type { StringOptions } from "sass";

const libRoot = path.resolve(__dirname, '../../src/') + "/";

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
      createVuePlugin({
        script: {
          sourceMap: false,
        },
      }),

      // This project is just a little bit too messed up to _also_ get alacarte components functional.
      // coalesce-vue-vuetify is included from source, but this doesn't seem able to properly transform
      // the vuetify component references contained within it.
      //createVueComponentImporterPlugin({
      //  dts: false,
      //  resolvers: [VuetifyResolver(), CoalesceVuetifyResolver()],
      //}),

      // Integrations with UseViteDevelopmentServer from IntelliTect.Coalesce.Vue
      createAspNetCoreHmrPlugin(),

      // Perform type checking during development and build time.
      createCheckerPlugin({
        // VLS: Vue Language Server, the language server portion of Vetur.
        vls: {
          vetur: {
            // Template validation is turned off because this mirrors the classic behavior of vue-cli.
            // However, modern syntax like the null propagating operator IS available in this configuration,
            // so these options can be turned on if desired.
            validation: {
              template: false,
              templateProps: false,
              interpolation: false,
            },
          },
        },
      }),
    ],

    resolve: {
      alias: [
        { find: '@', replacement: path.resolve(__dirname, 'src') },
        {
          find: 'coalesce-vue/lib',
          replacement: libRoot + 'coalesce-vue/src',
        },
        {
          find: 'coalesce-vue',
          replacement: libRoot + 'coalesce-vue/src',
        },
        {
          find: /^coalesce-vue-vuetify/,
          replacement: path.resolve(
            __dirname,
            // Import from the version of index with global restirations
            // since this app doesn't use alacarte registrations
            libRoot + 'coalesce-vue-vuetify2/src/index.dist.ts'
          ),
        },
        { find: /^vue$/, replacement: path.resolve(__dirname, 'node_modules/vue/dist/vue.runtime.common.js') },
        {
          find: /^vue-router/,
          replacement: path.resolve(__dirname, 'node_modules/vue-router'),
        },
      ],
    },
    server: {
      host: '0.0.0.0',
      fs: {
        allow: [libRoot + 'coalesce-vue', libRoot + 'coalesce-vue-vuetify2', '.'],
      },
    },

    esbuild: {
      // vue-class-component uses the original names of classes as the component name.
      keepNames: true,
    },

    css: { preprocessorOptions: { sass: sassOptions } },

    optimizeDeps: {
      // The configuration for SASS here is so that Vuetify's styles
      // will be included in Vite's Dependency Pre-Bundling feature.
      // Without this, in serve mode there will be nearly 100 extra HTTP requests,
      // one for each individual Vuetify component's stylesheet.
      // See https://github.com/vitejs/vite/issues/7719
      extensions: [".scss", ".sass"],
      esbuildOptions: {
        sourcemap: false,
        plugins: [
          sassPlugin({
            type: "style",
            ...sassOptions,
          }),
        ],
      },
    },
  };
});

const sassOptions: StringOptions<"sync"> = {
  quietDeps: true,
  // Logger warn override is a workaround for deprecation warning spam. See
  // https://github.com/sass/sass/issues/3065#issuecomment-868302160.
  // `quietDeps` is supposed to have the same effect, but doesn't work.
  logger: {
    warn(message, options) {
      if (
        (options.deprecation && options.stack?.includes("node_modules")) ||
        message.includes("repetitive deprecation")
      ) {
        return;
      }
      console.warn(
        `\x1b[33mSASS WARNING\x1b[0m: ${message}\n${options.stack === "null" ? "" : options.stack
        }\n`
      );
    },
  },
};
