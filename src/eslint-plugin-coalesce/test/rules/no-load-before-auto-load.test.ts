import { RuleTester } from "eslint";
import rule from "../../src/rules/no-load-before-auto-load.js";

const ruleTester = new RuleTester({
  languageOptions: { ecmaVersion: 2022, sourceType: "module" },
});

ruleTester.run("no-load-before-auto-load", rule, {
  valid: [
    // $useAutoLoad with immediate - no separate $load needed
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$useAutoLoad({ immediate: true });
        }
      `,
    },
    // Only $load, no auto-load - that's fine
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$load();
        }
      `,
    },
    // Only $useAutoLoad, no $load - that's fine
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$useAutoLoad();
        }
      `,
    },
    // Different objects - $load on one, $useAutoLoad on another
    {
      code: `
        function setup() {
          const list1 = new SomeListViewModel();
          const list2 = new SomeListViewModel();
          list1.$load();
          list2.$useAutoLoad();
        }
      `,
    },
    // $load in a different scope than $useAutoLoad
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$useAutoLoad();
        }
        function other() {
          const list = new SomeListViewModel();
          list.$load();
        }
      `,
    },
  ],
  invalid: [
    // $load then $useAutoLoad on same object
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$load();
          list.$useAutoLoad();
        }
      `,
      errors: [{ messageId: "useImmediate" }],
    },
    // $useAutoLoad then $load on same object
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$useAutoLoad();
          list.$load();
        }
      `,
      errors: [{ messageId: "useImmediate" }],
    },
    // $load then $startAutoLoad on same object
    {
      code: `
        function setup() {
          const list = new SomeListViewModel();
          list.$load();
          list.$startAutoLoad(this);
        }
      `,
      errors: [{ messageId: "useImmediateStartAutoLoad" }],
    },
    // Works with `this` as the object
    {
      code: `
        function setup() {
          this.$load();
          this.$useAutoLoad();
        }
      `,
      errors: [{ messageId: "useImmediate" }],
    },
  ],
});
