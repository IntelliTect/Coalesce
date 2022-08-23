import path from 'path';

import { defineConfig } from 'vite';

import createVuePlugin from '@vitejs/plugin-vue';
import createCheckerPlugin from 'vite-plugin-checker';
import createVueComponentImporterPlugin from 'unplugin-vue-components/vite';
import { Vuetify3Resolver } from 'unplugin-vue-components/resolvers';

// jiti is a workaround for https://github.com/vitejs/vite/issues/9202
import jiti from 'jiti';
const { createAspNetCoreHmrPlugin } = jiti(__filename)('../../src/coalesce-vue/src/build.ts')
const { CoalesceVuetifyResolver } = jiti(__filename)('../../src/coalesce-vue-vuetify3/src/build.ts')

import { sassPlugin } from 'esbuild-sass-plugin';

import type { InlineConfig as VitestInlineConfig } from 'vitest';
import type { StringOptions } from 'sass';

const libRoot = path.resolve(__dirname, '../../src/') + "/";

export default defineConfig(async ({ command, mode }) => {
  return {
    build: {
      outDir: 'wwwroot',
      rollupOptions: {
        output: {
          manualChunks(id) {
            if (id.match(/home/i)) return 'index';
            // All views are chunked together so that dynamic imports can be
            // used in `router.ts`(which makes for a much more readable file).
            // Without this, each dynamic import would get its own chunk.
            if (id.includes('views')) return 'views';
            return 'index';
          },
        },
      },
    },

    plugins: [
      createVuePlugin(),

      // vuetify(),

      // Transforms usages of Vuetify and Coalesce components into treeshakable imports
      createVueComponentImporterPlugin({
        dts: false,
        resolvers: [Vuetify3Resolver(), CoalesceVuetifyResolver()],
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
          find: 'coalesce-vue-vuetify',
          replacement: libRoot + 'coalesce-vue-vuetify3/src/index.ts',
        },
        {
          find: 'vue',
          replacement: path.resolve(__dirname, 'node_modules/vue'),
        },
        {
          find: 'vue-router',
          replacement: path.resolve(__dirname, 'node_modules/vue-router'),
        },
      ],
    },
    server: {
      host: '0.0.0.0',
      fs: {
        allow: [libRoot + 'coalesce-vue', libRoot + 'coalesce-vue-vuetify3', '.'],
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
      extensions: ['.scss', '.sass'],
      esbuildOptions: {
        plugins: [
          sassPlugin({
            type: 'style',
            ...sassOptions,
          }),
        ],
      },
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

const sassOptions: StringOptions<'sync'> = {
  quietDeps: true,
  // Logger warn override is a workaround for deprecation warning spam. See
  // https://github.com/sass/sass/issues/3065#issuecomment-868302160.
  // `quietDeps` is supposed to have the same effect, but doesn't work.
  logger: {
    warn(message, options) {
      if (
        (options.deprecation && options.stack?.includes('node_modules')) ||
        message.includes('repetitive deprecation')
      ) {
        return;
      }
      console.warn(
        `\x1b[33mSASS WARNING\x1b[0m: ${message}\n${
          options.stack === 'null' ? '' : options.stack
        }\n`
      );
    },
  },
};
