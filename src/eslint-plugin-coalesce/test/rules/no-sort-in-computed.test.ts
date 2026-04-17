import { RuleTester } from "eslint";
import rule from "../../src/rules/no-sort-in-computed.js";

const ruleTester = new RuleTester({
  languageOptions: { ecmaVersion: 2022, sourceType: "module" },
});

ruleTester.run("no-sort-in-computed", rule, {
  valid: [
    // .sort() outside of computed - fine
    {
      code: `const sorted = arr.sort();`,
    },
    // .toSorted() inside computed - fine
    {
      code: `const sorted = computed(() => arr.toSorted());`,
    },
    // .sort() chained after .map() inside computed - fine (new array)
    {
      code: `const sorted = computed(() => arr.map(x => x).sort());`,
    },
    // .sort() chained after .filter() inside computed - fine
    {
      code: `const sorted = computed(() => arr.filter(x => x).sort());`,
    },
    // .sort() chained after .slice() inside computed - fine
    {
      code: `const sorted = computed(() => arr.slice().sort());`,
    },
    // .sort() chained after .flatMap() inside computed - fine
    {
      code: `const sorted = computed(() => arr.flatMap(x => x).sort());`,
    },
    // .sort() chained after .concat() inside computed - fine
    {
      code: `const sorted = computed(() => arr.concat(other).sort());`,
    },
    // .sort() on array literal inside computed - fine
    {
      code: `const sorted = computed(() => [...arr].sort());`,
    },
    // .sort() on inline array - fine
    {
      code: `const sorted = computed(() => [3, 1, 2].sort());`,
    },
    // .sort() on local array variable initialized with [] - fine
    {
      code: `const sorted = computed(() => { const arr = []; arr.push(1); arr.sort(); return arr; });`,
    },
    // .sort() on local array variable with spread init - fine
    {
      code: `const sorted = computed(() => { const arr = [...items.value]; arr.sort(); return arr; });`,
    },
  ],
  invalid: [
    // .sort() on reactive array inside computed
    {
      code: `const sorted = computed(() => arr.sort());`,
      errors: [{ messageId: "noSort" }],
    },
    // .sort() on property access inside computed
    {
      code: `const sorted = computed(() => state.items.sort());`,
      errors: [{ messageId: "noSort" }],
    },
    // .sort() with comparator inside computed
    {
      code: `const sorted = computed(() => arr.sort((a, b) => a - b));`,
      errors: [{ messageId: "noSort" }],
    },
    // .sort() inside computed with function expression
    {
      code: `const sorted = computed(function() { return arr.sort(); });`,
      errors: [{ messageId: "noSort" }],
    },
  ],
});
