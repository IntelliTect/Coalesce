module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    "plugin:vue/vue3-essential",
    "eslint:recommended",
    "@vue/eslint-config-typescript",
    "@vue/eslint-config-prettier",
  ],
  parserOptions: {
    ecmaVersion: "latest",
  },
  rules: {
    "prettier/prettier": [
      "error",
      {
        endOfLine: "auto",
      },
    ],

    "vue/multi-word-component-names": "off",
    "prefer-const": "error",
    "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    "no-console": "off",
    "no-undef": "off", // Redundant with Typescript

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
  ignorePatterns: ["wwwroot", "node_modules", "/**/*.g.ts"],
};
