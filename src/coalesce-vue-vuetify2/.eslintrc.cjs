module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    "plugin:vue/essential",
    "eslint:recommended",
    "@vue/eslint-config-typescript",
  ],
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "error" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
    "@typescript-eslint/no-unused-vars": "error",
    "no-unused-vars": "off",
    "no-case-declarations": "off",
    "no-undef": "off", // Redundant with Typescript
  },
  ignorePatterns: ["lib", "dist", "node_modules"],
};
