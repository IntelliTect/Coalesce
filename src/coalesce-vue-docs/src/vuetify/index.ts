import "./content.scss";
import "./layout.scss";

export { default as CDocsLayout } from "./c-docs-layout.vue";
export { default as CDocsPage } from "./c-docs-page.vue";
export { default as CDocsSidebar } from "./c-docs-sidebar.vue";
export { default as CDocsToc } from "./c-docs-toc.vue";
export { default as CDocsSearch } from "./c-docs-search.vue";
export { default as CDocsHelp } from "./c-docs-help.vue";

export { docsRoutes, type DocsRoutesOptions } from "./routes";
export { useDocsContent, type DocsContentOptions } from "./useDocsContent";
export {
  docsManifest,
  docPages,
  findDocPage,
  canonicalDocLocation,
  sidebarTrail,
  pagerFor,
  type DocsPager,
} from "./docs";
export type {
  DocsManifest,
  DocMeta,
  DocModule,
  DocPage,
  DocSearchSection,
} from "../shared";
