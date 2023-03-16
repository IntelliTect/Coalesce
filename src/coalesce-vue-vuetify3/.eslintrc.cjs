module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    "plugin:vue/vue3-essential",
    "eslint:recommended",
    "@vue/eslint-config-typescript",
  ],
  parserOptions: {
    ecmaVersion: "latest",
  },
  rules: {
    "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    "no-useless-escape": "off",
    "no-case-declarations": "off",
    "no-console": "off",
    "vue/no-mutating-props": "off", // Falsely reports for mutating children of props
    "no-undef": "off", // Redundant with Typescript
  },
  ignorePatterns: ["dist", "node_modules"],
};
