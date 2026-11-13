import {
  onScopeDispose,
  ref,
  shallowRef,
  watch,
  type Component,
  type InjectionKey,
  type Ref,
} from "vue";
import type { RouteLocationNormalized } from "vue-router";
import type { DefaultTheme, Header } from "vitepress";
import * as virtual from "virtual:coalesce-vue-docs";

import {
  canonicalDocPath,
  docPath,
  type DocMeta,
  type DocModule,
  type DocPage,
  type DocsManifest,
} from "../shared";

export const docsManifest: DocsManifest = {
  base: virtual.base,
  sidebar: virtual.sidebar,
};

export const docPages: DocPage[] = Object.entries(virtual.modules)
  .map(([file, load]) => ({
    file,
    path: docPath(virtual.base, file.slice(virtual.srcPrefix.length)),
    load: load as () => Promise<DocModule>,
  }))
  .sort((a, b) => a.path.localeCompare(b.path));

/** Finds the page serving `path`, tolerating a trailing slash or `/index`. */
export function findDocPage(path: string): DocPage | undefined {
  const canonical = canonicalDocPath(path, virtual.base);
  return docPages.find((p) => p.path === canonical);
}

/** Redirects a doc URL with a trailing slash or `/index` to the one URL the page is served at. */
export function canonicalDocLocation(to: RouteLocationNormalized) {
  const path = canonicalDocPath(to.path, virtual.base);
  if (path === to.path) return;
  return { path, query: to.query, hash: to.hash, replace: true };
}

/**
 * Picks the sidebar for `path`. A multi-sidebar config keys its sidebars by URL
 * prefix, so the longest matching prefix wins.
 */
export function sidebarFor(path: string): DefaultTheme.SidebarItem[] {
  const { sidebar } = docsManifest;
  if (Array.isArray(sidebar)) return sidebar;

  const match = Object.keys(sidebar)
    .filter((prefix) => path.startsWith(prefix))
    .sort((a, b) => b.length - a.length)[0];

  if (!match) return [];

  const value = sidebar[match];
  return Array.isArray(value) ? value : value.items;
}

/** Texts of the sidebar groups that contain the page at `path`, outermost first. */
export function sidebarTrail(path: string): string[] {
  const walk = (
    items: DefaultTheme.SidebarItem[],
    trail: string[],
  ): string[] | undefined => {
    for (const item of items) {
      if (item.link === path) return trail;
      if (item.items?.length) {
        const found = walk(
          item.items,
          item.text ? [...trail, item.text] : trail,
        );
        if (found) return found;
      }
    }
  };
  return walk(sidebarFor(path), []) ?? [];
}

export interface DocsPager {
  prev?: { text: string; link: string };
  next?: { text: string; link: string };
}

/**
 * The pages before and after `path` in sidebar order, staying within the
 * page's top-level sidebar section.
 */
export function pagerFor(path: string): DocsPager {
  const links = (
    items: DefaultTheme.SidebarItem[],
  ): { text: string; link: string }[] =>
    items.flatMap((item) => [
      ...(item.link ? [{ text: item.text ?? "", link: item.link }] : []),
      ...links(item.items ?? []),
    ]);

  for (const section of sidebarFor(path)) {
    const pages = links([section]);
    const index = pages.findIndex((p) => p.link === path);
    if (index >= 0) return { prev: pages[index - 1], next: pages[index + 1] };
  }
  return {};
}

/**
 * Flattens the header tree for the table of contents. Which levels are in the
 * tree at all is `markdown.headers.level` in the VitePress config.
 */
export function tocEntries(headers: Header[]): Header[] {
  const flat: Header[] = [];

  const walk = (items: Header[]) => {
    for (const item of items) {
      flat.push(item);
      if (item.children?.length) walk(item.children);
    }
  };

  walk(headers);
  return flat;
}

/** Lets the page report its headings to the layout's table of contents. */
export const docsShellState = Symbol("docsShellState") as InjectionKey<{
  headers: Ref<Header[]>;
}>;

/** Loads the doc page at a URL, and reloads its meta when HMR updates it. */
export function useDocPage(path: () => string | undefined) {
  const component = shallowRef<Component>();
  const meta = shallowRef<DocMeta>();
  const missing = ref(false);
  const page = shallowRef<DocPage>();

  // A slow page must not overwrite a newer one.
  let loadToken = 0;
  let stopHmr = () => {};

  async function load(target: string | undefined) {
    const token = ++loadToken;
    stopHmr();

    page.value = target ? findDocPage(target) : undefined;
    if (!page.value) {
      missing.value = !!target;
      component.value = undefined;
      meta.value = undefined;
      return;
    }

    missing.value = false;
    const file = page.value.file;
    const mod = await page.value.load();
    if (token !== loadToken) return;

    component.value = mod.default;
    meta.value = mod.meta;
    stopHmr = virtual.onPageUpdate(file, (updated: DocModule) => {
      if (token === loadToken) meta.value = updated.meta;
    });
  }

  watch(path, load, { immediate: true });
  onScopeDispose(() => stopHmr());

  return { component, meta, missing, page };
}
