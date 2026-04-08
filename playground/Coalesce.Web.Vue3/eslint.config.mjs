import pluginVue from "eslint-plugin-vue";
import vueTsEslintConfig from "@vue/eslint-config-typescript";
import coalesce from "eslint-plugin-coalesce";

export default [
  {
    name: "app/files-to-lint",
    files: ["**/*.{ts,mts,tsx,vue}"],
  },

  {
    name: "app/files-to-ignore",
    ignores: ["**/wwwroot/**", "**/*.g.ts"],
  },

  ...pluginVue.configs["flat/essential"],
  ...vueTsEslintConfig(),
  coalesce.configs.recommended,

  {
    rules: {
      "vue/multi-word-component-names": "off",
    },
  },
];
