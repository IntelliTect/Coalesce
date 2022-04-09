import { babel } from "@rollup/plugin-babel";
import { nodeResolve } from "@rollup/plugin-node-resolve";
import replace from "@rollup/plugin-replace";
import typescript from "rollup-plugin-typescript2";
import sourcemaps from "rollup-plugin-sourcemaps";
import postcss from "rollup-plugin-postcss";
import autoprefixer from "autoprefixer";
import vue from "rollup-plugin-vue";
import vuetify from "rollup-plugin-vuetify";

import pkg from "./package.json";

const sharedPlugins = [
  nodeResolve(),
  vue({
    css: false
  }),
  typescript({
    typescript: require("typescript"),
    tsconfig: "tsconfig.build.json"
  }),
  sourcemaps(),
  postcss({
    extract: "coalesce-vue-vuetify.css",
    use: ["sass"],
    plugins: [autoprefixer()]
  }),
  babel({
    babelHelpers: "bundled",
    extensions: [".js", ".ts", ".vue"],
    plugins: [
      // Needed for vue-cli support, since vue-cli is based on webpack4 and webpack4 can't
      // parse these syntax features
      "@babel/plugin-proposal-optional-chaining",
      "@babel/plugin-proposal-nullish-coalescing-operator"
    ],
    presets: [
      [
        "@vue/babel-preset-app",
        {
          useBuiltIns: false
        }
      ]
    ]
  })
];

const external = [
  ...Object.keys(pkg.dependencies),
  ...Object.keys(pkg.peerDependencies),
  "vuetify/lib"
];

export default [
  {
    // Non-treeshaking build, for use with `import Vuetify from 'vuetify'`.
    // Referenced vuetify components will be referenced by name, expecting global registrations.
    input: "src/index.ts",
    plugins: [
      replace({
        "process.env.TREESHAKE": false
      }),
      ...sharedPlugins
    ],
    external,

    treeshake: {
      // Strips the unused import of "vuetify/lib" that will be left over after all usages of imports from it
      // are stripped out by `process.env.TREESHAKE = false`.
      // If we don't do this, vite will inline an extra, unused copy of vuetify into coalesce-vue-vuetify...
      moduleSideEffects: false
    },

    output: [
      {
        sourcemap: true,
        file: "dist/coalesce-vue-vuetify.common.js",
        format: "cjs"
      },
      {
        sourcemap: true,
        file: "dist/coalesce-vue-vuetify.esm.js",
        format: "esm"
      }
    ]
  },
  {
    // Treeshaking build, for use with `import Vuetify from 'vuetify/lib'`.
    // Referenced vuetify components will be imported from 'vuetify/lib'.
    input: "src/index.ts",
    plugins: [
      replace({
        "process.env.TREESHAKE": true
      }),
      ...sharedPlugins,
      vuetify()
    ],
    external,
    output: [
      {
        sourcemap: true,
        file: "lib/coalesce-vue-vuetify.common.js",
        format: "cjs"
      },
      {
        sourcemap: true,
        file: "lib/coalesce-vue-vuetify.esm.js",
        format: "esm"
      }
    ]
  }
];
