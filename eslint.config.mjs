import pluginVue from "eslint-plugin-vue";
import vueTsEslintConfig from "@vue/eslint-config-typescript";
import pluginVitest from "@vitest/eslint-plugin";
import eslintPluginPrettier from "eslint-plugin-prettier/recommended";
import coalesce from "./src/eslint-plugin-coalesce/src/index.ts";

const tsconfigRootDir = import.meta.dirname;

export default [
  {
    name: "app/files-to-lint",
    files: ["**/*.{ts,mts,tsx,vue}"],
  },

  {
    name: "app/files-to-ignore",
    ignores: [
      "templates/**",
      "**/node_modules/**",
      "**/bin/**",
      "**/.vitepress/cache/**",
      "**/.vitepress/dist/**",
      "**/obj/**",
      "**/wwwroot/**",
      "**/coverage/**",
      "**/*.g.ts",
      "src/coalesce-vue/lib/**",
      "src/coalesce-vue-vuetify3/dist/**",
      "src/eslint-plugin-coalesce/lib/**",
      "src/coalesce-mcp/dist/**",
      "eslint.config.mjs",
    ],
  },

  ...pluginVue.configs["flat/recommended"],
  ...vueTsEslintConfig(),
  {
    languageOptions: {
      parserOptions: {
        tsconfigRootDir,
      },
    },
  },
  {
    ...pluginVitest.configs.recommended,
    files: [
      "src/*/src/**/__tests__/*",
      "src/*/src/**/*.spec.*",
      "playground/*/src/**/*.spec.*",
    ],
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
      "vue/no-mutating-props": ["error", { shallowOnly: true }],

      "prefer-const": "error",
      "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
      "no-console": "off",
      "no-undef": "off", // Redundant with Typescript

      "@typescript-eslint/no-explicit-any": "off",
      "@typescript-eslint/no-unused-expressions": "off",
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

  // coalesce-vue-vuetify3 overrides
  {
    name: "coalesce-vue-vuetify3/overrides",
    files: ["src/coalesce-vue-vuetify3/**/*.{ts,mts,tsx,vue}"],
    rules: {
      "vue/attribute-hyphenation": "off",
      "vue/component-definition-name-casing": "off",
      "vue/require-default-prop": "off",
    },
  },

  // TSX overrides (no-unused-expressions is too noisy with JSX)
  {
    name: "app/tsx-overrides",
    files: ["**/*.tsx"],
    rules: {
      "@typescript-eslint/no-unused-expressions": "off",
      "vitest/expect-expect": "off", // https://github.com/vitest-dev/eslint-plugin-vitest/issues/697
    },
  },

  // Playground uses eslint-plugin-coalesce
  {
    name: "playground/overrides",
    files: ["playground/**/*.{ts,mts,tsx,vue}"],
    ...coalesce.configs.recommended,
    rules: {
      ...coalesce.configs.recommended.rules,
      "vue/require-default-prop": "off",
      "@typescript-eslint/no-unused-vars": "off",
    },
  },
];
