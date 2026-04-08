import { RuleTester } from "eslint";
import rule from "../../src/rules/no-static-router-import.js";

const ruleTester = new RuleTester({
  languageOptions: { ecmaVersion: 2022, sourceType: "module" },
});

ruleTester.run("no-static-router-import", rule, {
  valid: [
    // useRouter() - the correct approach
    {
      code: `import { useRouter } from 'vue-router';`,
      filename: "Component.vue",
    },
    // Not a .vue file - allowed
    {
      code: `import router from '@/router';`,
      filename: "main.ts",
    },
    // Not a default import named "router"
    {
      code: `import { createRouter } from '@/router';`,
      filename: "Component.vue",
    },
    // Default import with different name from router path
    {
      code: `import myRouter from '@/router';`,
      filename: "Component.vue",
    },
    // Import from a non-router path
    {
      code: `import router from '@/store';`,
      filename: "Component.vue",
    },
  ],
  invalid: [
    // Direct import of router in .vue file
    {
      code: `import router from '@/router';`,
      filename: "Component.vue",
      errors: [{ messageId: "useRouter" }],
    },
    // Relative path import
    {
      code: `import router from '../router';`,
      filename: "src/components/Component.vue",
      errors: [{ messageId: "useRouter" }],
    },
    // With index
    {
      code: `import router from '@/router/index';`,
      filename: "Component.vue",
      errors: [{ messageId: "useRouter" }],
    },
  ],
});
