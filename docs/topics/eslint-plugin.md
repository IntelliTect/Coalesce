# ESLint Plugin

Coalesce provides an ESLint plugin (`eslint-plugin-coalesce`) that detects common mistakes in Coalesce Vue applications. The plugin ships with a recommended configuration that enables all rules as warnings.

## Setup

The plugin is included in the [project template](/stacks/vue/getting-started.md) by default.

To add it to an existing project, install it:

```sh
npm install --save-dev eslint-plugin-coalesce
```

Then add the recommended config to your `eslint.config.mjs`:

```js
import coalesce from "eslint-plugin-coalesce";

export default [
  // ... your other configs
  coalesce.configs.recommended,
];
```

## Rules

### `coalesce/no-load-before-auto-load`

Detects when `$load()` is called alongside `$useAutoLoad()` or `$startAutoLoad()` on the same object in the same scope. The auto-load methods support an `immediate` option that performs the initial load, making a separate `$load()` call redundant and potentially racy.

```ts
// ❌ Bad
const list = new PersonListViewModel();
list.$load();
list.$useAutoLoad();

// ✅ Good
const list = new PersonListViewModel();
list.$useAutoLoad({ immediate: true });
```

### `coalesce/no-sort-in-computed`

Detects in-place `.sort()` calls inside `computed()` functions. Since `.sort()` mutates the array in-place, it can trigger reactivity updates that cause infinite loops.

The rule allows `.sort()` when chained after methods that produce new arrays (`.map()`, `.filter()`, `.slice()`, `.flatMap()`, `.concat()`, `.toReversed()`, `.toSpliced()`, `.flat()`), or when called on array literals like `[...arr].sort()`.

```ts
// ❌ Bad
const sorted = computed(() => items.value.sort());

// ✅ Good
const sorted = computed(() => items.value.toSorted());
const sorted = computed(() => [...items.value].sort());
const sorted = computed(() => items.value.slice().sort());
```

### `coalesce/no-static-router-import`

In `.vue` files, detects direct default imports of the router instance (e.g. `import router from '@/router'`), which almost always creates circular dependencies. Use `useRouter()` from `vue-router` instead.

```ts
// ❌ Bad (in a .vue file)
import router from "@/router";

// ✅ Good
import { useRouter } from "vue-router";
const router = useRouter();
```

## Configuration

All rules are enabled as warnings in the recommended config. To customize severity:

```js
import coalesce from "eslint-plugin-coalesce";

export default [
  coalesce.configs.recommended,
  {
    rules: {
      "coalesce/no-sort-in-computed": "error",
      "coalesce/no-static-router-import": "off",
    },
  },
];
```
