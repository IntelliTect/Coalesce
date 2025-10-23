import pluginVue from "eslint-plugin-vue";
import vueTsEslintConfig from "@vue/eslint-config-typescript";
import pluginVitest from "@vitest/eslint-plugin";
import eslintPluginPrettier from "eslint-plugin-prettier/recommended";
export default [
  {
    name: "app/files-to-lint",
    files: ["**/*.{ts,mts,tsx,vue}"],
    languageOptions: {
      parserOptions: {
        tsconfigRootDir: import.meta.dirname,
        // or, in CommonJS, __dirname
      },
    },
  },

  {
    name: "app/files-to-ignore",
    ignores: ["dist/**", "**/*.g.ts", "**/coverage/**", "eslint.config.mjs"],
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
      "vue/attribute-hyphenation": "off",
      "vue/component-definition-name-casing": "off",
      "vue/require-default-prop": "off", // Annoying and wrong about defineProps
      "vue/no-template-shadow": ["error", { allow: ["props"] }],
      "vue/no-mutating-props": ["error", { shallowOnly: true }],

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
            "^(props|mock|saveMock|_.*)$",
          args: "none",
        },
      ],
    },
  },

  {
    name: "app/tsx-overrides",
    files: ["**/*.tsx"],
    rules: {
      "@typescript-eslint/no-unused-expressions": "off",
      "vitest/expect-expect": "off", // https://github.com/vitest-dev/eslint-plugin-vitest/issues/697
    },
  },
];
