declare module "virtual:coalesce-vue-docs" {
  import type { DefaultTheme } from "vitepress";

  /** URL the docs are mounted at, with a trailing slash. */
  export const base: string;
  export const sidebar: DefaultTheme.Sidebar;
  /** Root-relative directory of the markdown sources, with slashes on both ends. */
  export const srcPrefix: string;
  export const modules: Record<string, () => Promise<any>>;
  /** Calls `callback` with the re-imported module after an HMR update to `file`. Returns an unsubscribe function. */
  export function onPageUpdate(
    file: string,
    callback: (mod: any) => void,
  ): () => void;
}

declare module "virtual:coalesce-vue-docs-search" {
  /** Every page's text, split into sections at its headings. */
  const sections: {
    id: string;
    path: string;
    anchor: string;
    pageTitle: string;
    heading: string;
    text: string;
  }[];
  export default sections;
}

declare module "*.scss" {}
