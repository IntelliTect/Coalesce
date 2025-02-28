import pluginVue from "eslint-plugin-vue";
import vueTsEslintConfig from "@vue/eslint-config-typescript";
import pluginVitest from "@vitest/eslint-plugin";
import eslintPluginPrettier from "eslint-plugin-prettier/recommended";
export default [
  {
    name: "app/files-to-lint",
    files: ["**/*.{ts,mts,tsx,vue}"],
  },

  {
    name: "app/files-to-ignore",
    ignores: ["**/wwwroot/**", "**/*.g.ts", "**/coverage/**"],
  },

  ...pluginVue.configs["flat/recommended"],
  ...vueTsEslintConfig(),
  {
    ...pluginVitest.configs.recommended,
    files: ["src/**/__tests__/*", "src/**/*.spec.*"],
  },
  eslintPluginPrettier,

  {
    rules: {
      "prettier/prettier": [
        "error",
        {
          endOfLine: "auto",
        },
      ],

      "vue/multi-word-component-names": "off",
      "vue/no-template-shadow": ["error", { allow: ["props"] }],

      "prefer-const": "error",
      "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
      "no-console": "off",
      "no-undef": "off", // Redundant with Typescript

      "@typescript-eslint/no-explicit-any": "off",
      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          varsIgnorePattern:
            // Allow unused `const props = defineProps` (if props are only used in the template of an SFC)
            "props",
          args: "none",
        },
      ],
    },
  },
];
