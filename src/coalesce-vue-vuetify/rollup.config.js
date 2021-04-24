

import { babel } from '@rollup/plugin-babel';
import typescript from 'rollup-plugin-typescript2'
import sourcemaps from 'rollup-plugin-sourcemaps';
import postcss from 'rollup-plugin-postcss';
import autoprefixer from 'autoprefixer'
import vue from 'rollup-plugin-vue'
import vuetify from 'rollup-plugin-vuetify'

import pkg from './package.json';

export default {
  input: "src/index.ts",
  plugins: [
    vue({
      css: false
    }),
    typescript({
      typescript: require('typescript'),
      tsconfig: 'tsconfig.build.json',
    }),
    sourcemaps(),
    postcss({
      extract: 'coalesce-vue-vuetify.css',
      use: ['sass'],
      plugins: [
        autoprefixer(),
      ]
    }),
    vuetify(),
    babel({ babelHelpers: 'bundled', extensions: ['.js', '.ts', '.vue'] }),
  ],
  external: [
    ...Object.keys(pkg.dependencies),
    ...Object.keys(pkg.peerDependencies),
  ],
  output: [
    {
      sourcemap: true,
      file: pkg.main,
      format: "cjs"
    },
    {
      sourcemap: true,
      file: pkg.module,
      format: "esm"
    }
  ]
};
