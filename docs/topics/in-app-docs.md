# In-app Documentation

The `coalesce-vue-docs` package serves product documentation from inside a Coalesce app. Markdown under `docs/` is compiled by [VitePress](https://vitepress.dev)'s markdown renderer in the app's own Vite server, with no second server and no VitePress theme, and is drawn in a Vuetify layout at `/docs` with full-text search, a sidebar, previous and next links, and an "On this page" table of contents.

The [project template](/stacks/vue/getting-started.md) includes this setup when created with the `--Docs` option.

## Setup

Install the package along with VitePress. VitePress is a peer dependency pinned to the exact version Coalesce is tested against, because the package uses VitePress internals that are not yet stable:

```sh
pnpm add coalesce-vue-docs
pnpm add -D vitepress@2.0.0-alpha.19
```

Add the Vite plugin before `@vitejs/plugin-vue` in `vite.config.ts`:

```ts
import { vitepressDocs } from "coalesce-vue-docs/vite";

export default defineConfig({
  plugins: [
    vitepressDocs(),
    createVuePlugin(/* ... */),
    // ...
  ],
});
```

Add the route in `router.ts`:

```ts
import { docsRoutes } from "coalesce-vue-docs/vuetify";

const router = createRouter({
  routes: [
    // ...
    docsRoutes(),
  ],
});
```

If your `<router-view>` is keyed on `route.path` (as it is in the template), key it on `route.meta.key ?? route.path` instead. Otherwise the layout remounts on every page change:

```vue-html
<component :is="Component" :key="route.meta.key ?? route.path" />
```

Then create `docs/.vitepress/config.ts`:

```ts
import { defineConfig } from "vitepress";

export default defineConfig({
  base: "/docs/",
  cleanUrls: true,
  themeConfig: {
    sidebar: [
      {
        text: "Guide",
        items: [{ text: "Getting started", link: "/guide/getting-started" }],
      },
    ],
  },
  markdown: {
    // Required for the "On this page" table of contents.
    headers: { level: [2, 3] },
  },
});
```

Only `base`, `cleanUrls`, `srcDir`, `themeConfig.sidebar`, and `markdown` are used. Navigation, the theme, and other site options are ignored.

## Writing Pages

Each `.md` file under `docs/` is a page, served at its path relative to `docs/` under `base`. An `index.md` is served at its directory's URL. Pages support VitePress's [markdown extensions](https://vitepress.dev/guide/markdown), including custom containers, code groups, line highlighting, and Vue components.

Relative links between pages, such as `[Prices](./prices.md)`, are routed through vue-router.

Previous and next links follow the sidebar order, within the page's top-level sidebar section. Above the page, the sidebar groups that contain it are shown as a trail.

## Search

The search box at the top of the sidebar, also opened by pressing `/`, searches the text of every page, split into sections at its `h1`–`h3` headings. The index is built by `vitepressDocs` and downloaded the first time search is opened. Exclude a page with `search: false` in its frontmatter.

## Help Overlay

`c-docs-help` shows a doc page in a panel on the right side of the screen, from any screen. Links in the panel to other doc pages stay in the panel; other links close it and navigate the app.

```vue-html
<c-docs-help doc="/docs/guide/orders#refunds" title="Refunds" />
```

Supply the `activator` slot to replace the default Help button:

```vue-html
<c-docs-help doc="/docs/guide/orders">
  <template #activator="{ props }">
    <v-btn v-bind="props" text="How do orders work?" />
  </template>
</c-docs-help>
```

`c-docs-help` and the other `c-docs-*` components are resolved by `CoalesceVuetifyResolver`.

## API

### `vitepressDocs(options)`

Exported from `coalesce-vue-docs/vite`.

- `root`: Directory containing `.vitepress/config.ts`, relative to the Vite root. Defaults to `docs`.
- `base`: URL the docs are mounted at. Defaults to `base` from the VitePress config.

### `docsRoutes(options)`

Exported from `coalesce-vue-docs/vuetify`. Returns the route record for `/docs`.

- `base`: URL the docs are mounted at. Defaults to the base from `vitepressDocs`.

### Components

- `c-docs-layout`: The search box, sidebar, page, and table of contents. The route component of `docsRoutes()`. On narrow screens the sidebar collapses behind a Menu button.
- `c-docs-page`: The current route's page, with its sidebar trail and previous and next links. Emits `rendered` with the content element after each page renders.
- `c-docs-sidebar`: The sidebar from the VitePress config for the current route.
- `c-docs-search`: The search box and its dialog.
- `c-docs-toc`: A table of contents for the given `headers`.
- `c-docs-help`: A panel showing the page at `doc`. Emits `rendered` like `c-docs-page`.

### `useDocsContent(options)`

Returns a click handler that wires up the markup VitePress's markdown plugins emit: copy buttons, code group tabs, and relative links. Used by `c-docs-page` and `c-docs-help`, and available for showing pages elsewhere.

## Styling

Page content is styled under the `.c-doc` class using Vuetify's theme colors, and follows Vuetify's light and dark themes. The sticky sidebar and table of contents start below the app bar, whose height is read from `--navbar-height` if set, otherwise from Vuetify's layout.
