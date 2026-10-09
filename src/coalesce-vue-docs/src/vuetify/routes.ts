import type { RouteRecordRaw } from "vue-router";

import { canonicalDocLocation, docsManifest } from "./docs";
import CDocsLayout from "./c-docs-layout.vue";
import CDocsPage from "./c-docs-page.vue";

declare module "vue-router" {
  interface RouteMeta {
    /** Routes that share a key render one component instance instead of remounting on navigation. */
    key?: string;
  }
}

export interface DocsRoutesOptions {
  /** URL the docs are mounted at. Defaults to `base` from the VitePress config. */
  base?: string;
}

/** The route that serves every doc page, with the sidebar and table of contents around it. */
export function docsRoutes(options: DocsRoutesOptions = {}): RouteRecordRaw {
  const path = (options.base ?? docsManifest.base).replace(/\/+$/, "");

  return {
    path,
    component: CDocsLayout,
    // Keeps the layout mounted across doc pages when the app's router-view
    // is keyed on `route.meta.key ?? route.path`.
    meta: { key: path },
    beforeEnter: canonicalDocLocation,
    children: [
      // `:path(.*)*` does not match the parent's own path, so the docs index
      // needs a child of its own.
      { path: "", component: CDocsPage },
      { path: ":path(.*)*", component: CDocsPage },
    ],
  };
}
