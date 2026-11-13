// Types for doc pages imported directly from app code, e.g. `import Page from "../docs/guide.md"`.
// Reference with `/// <reference types="coalesce-vue-docs/client" />`.

declare module "*.md" {
  import type { Component } from "vue";
  import type { Header } from "vitepress";

  const component: Component;
  export default component;

  export const meta: {
    title: string;
    frontmatter: Record<string, unknown>;
    headers: Header[];
    /** Path of the source file relative to the docs directory. */
    relativePath: string;
    /** App URL that serves this page. */
    docPath: string;
    /** URL directory this page's relative links resolve against. */
    docDir: string;
  };
  export const frontmatter: Record<string, unknown>;
  export const headers: Header[];
}
