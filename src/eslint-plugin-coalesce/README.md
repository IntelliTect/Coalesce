# eslint-plugin-coalesce

ESLint plugin to detect common mistakes in [Coalesce](https://intellitect.github.io/Coalesce/) applications.

## Installation

```bash
npm install --save-dev eslint-plugin-coalesce
```

## Usage

Add the recommended config to your `eslint.config.mjs`:

```js
import coalesce from "eslint-plugin-coalesce";

export default [
  // ... your other configs
  coalesce.configs.recommended,
];
```

## Rules

### `coalesce/no-load-before-auto-load`

Detects when `$load()` is called alongside `$useAutoLoad()` or `$startAutoLoad()` on the same object. The `$useAutoLoad` method already supports an `immediate` option that performs the initial load, making a separate `$load()` call redundant and potentially racy.

```js
// Bad
const list = new PersonListViewModel();
list.$load();
list.$useAutoLoad();

// Good
const list = new PersonListViewModel();
list.$useAutoLoad({ immediate: true });
```

### `coalesce/no-sort-in-computed`

Detects in-place `.sort()` calls inside `computed()` functions, which can produce infinite reactivity loops since `.sort()` mutates the array in-place and triggers reactivity updates.

`.sort()` is allowed when chained after array-producing methods (`.map()`, `.filter()`, `.slice()`, etc.) or called on array literals (`[...arr].sort()`).

```js
// Bad
const sorted = computed(() => items.value.sort());

// Good
const sorted = computed(() => items.value.toSorted());
const sorted = computed(() => [...items.value].sort());
const sorted = computed(() => items.value.slice().sort());
```

### `coalesce/no-static-router-import`

In `.vue` files, detects direct default imports of the router instance (e.g. `import router from '@/router'`), which almost always creates circular dependencies. Use `useRouter()` from `vue-router` instead.

```js
// Bad (in a .vue file)
import router from "@/router";

// Good
import { useRouter } from "vue-router";
const router = useRouter();
```
