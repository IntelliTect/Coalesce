import { readdir, readFile } from "node:fs/promises";
import * as path from "node:path";

import {
  normalizePath,
  type FilterPattern,
  type Plugin,
  type ViteDevServer,
} from "vite";
import {
  createMarkdownRenderer,
  disposeMdItInstance,
  resolveConfig,
  type DefaultTheme,
  type MarkdownEnv,
  type MarkdownRenderer,
  type SiteConfig,
} from "vitepress";

import {
  SEARCH_MODULE_ID,
  VIRTUAL_MODULE_ID,
  docDir,
  docPath,
  withBaseSidebar,
  type DocMeta,
  type DocSearchSection,
} from "./shared.js";

const RESOLVED_VIRTUAL_MODULE_ID = "\0" + VIRTUAL_MODULE_ID;
const RESOLVED_SEARCH_MODULE_ID = "\0" + SEARCH_MODULE_ID;

export interface VitepressDocsOptions {
  /** Directory containing `.vitepress/config.ts`, relative to the Vite root. Defaults to `docs`. */
  root?: string;
  /** URL the docs are mounted at. Defaults to `base` from the VitePress config. */
  base?: string;
}

/**
 * Compiles the markdown under `options.root` into Vue components with the
 * markdown renderer VitePress uses. Generates `virtual:coalesce-vue-docs` with
 * the docs' base URL, sidebar, and page loaders, and
 * `virtual:coalesce-vue-docs-search` with the search index.
 */
export function vitepressDocs(options: VitepressDocsOptions = {}): Plugin {
  let viteRoot: string;
  let docsRoot: string;
  let siteConfig: SiteConfig;
  let md: MarkdownRenderer;
  let srcDir: string;
  let base: string;
  let command: "serve" | "build" = "serve";

  interface CompiledDoc {
    /** Markdown this was compiled from, so a stale entry is never reused. */
    src: string;
    sfc: string;
    includes: string[];
  }

  // An edit compiles the page twice: once for plugin-vue's HMR diff and once
  // for the transform that follows it.
  const compiled = new Map<string, CompiledDoc>();

  async function loadDocsConfig(mode: string) {
    // createMarkdownRenderer memoizes one instance per process; drop the
    // previous one so a config change is picked up.
    disposeMdItInstance();

    siteConfig = await resolveConfig(docsRoot, command, mode);
    srcDir = normalizePath(siteConfig.srcDir);
    base = options.base ?? siteConfig.site.base;
    if (!base.endsWith("/")) base += "/";

    md = await createMarkdownRenderer(
      siteConfig.srcDir,
      siteConfig.markdown,
      base,
      undefined,
      siteConfig.publicDir,
    );
    compiled.clear();
  }

  async function compileDoc(src: string, file: string): Promise<CompiledDoc> {
    const cached = compiled.get(file);
    if (cached?.src === src) return cached;

    const relativePath = path.posix.relative(srcDir, file);

    const env: MarkdownEnv = {
      path: file,
      relativePath,
      cleanUrls: !!siteConfig.cleanUrls,
      includes: [],
    };

    const html = await md.renderAsync(src, env);
    const { frontmatter = {}, headers = [], sfcBlocks } = env;
    const title =
      typeof frontmatter.title === "string"
        ? frontmatter.title
        : (env.title ?? "");

    const scripts = sfcBlocks?.scripts.map((block) => block.content) ?? [];
    injectDocMeta(scripts, {
      title,
      frontmatter,
      headers,
      relativePath,
      docPath: docPath(base, relativePath),
      docDir: docDir(base, relativePath),
    });

    const result: CompiledDoc = {
      src,
      sfc: [
        ...scripts,
        `<template><div class="c-doc vp-doc">${html}</div></template>`,
        ...(sfcBlocks?.styles.map((block) => block.content) ?? []),
        ...(sfcBlocks?.customBlocks.map((block) => block.content) ?? []),
      ].join("\n"),
      includes: env.includes ?? [],
    };

    compiled.set(file, result);
    return result;
  }

  function isDocFile(id: string) {
    // Sub-requests plugin-vue makes for a compiled block have a query
    // (`?vue&type=style`), which has to fall through to plugin-vue.
    if (!id.endsWith(".md")) return false;
    const file = normalizePath(id);
    return (
      file.startsWith(srcDir + "/") && !file.startsWith(srcDir + "/.vitepress/")
    );
  }

  /** Splits every page's rendered HTML into sections at its headings. */
  async function buildSearchIndex(): Promise<DocSearchSection[]> {
    const files = (await readdir(srcDir, { recursive: true }))
      .map((f) => normalizePath(f))
      .filter((f) => f.endsWith(".md") && !f.startsWith(".vitepress/"));

    const sections: DocSearchSection[] = [];
    for (const relativePath of files) {
      const src = await readFile(path.join(srcDir, relativePath), "utf-8");
      const env: MarkdownEnv = {
        path: path.posix.join(srcDir, relativePath),
        relativePath,
        cleanUrls: !!siteConfig.cleanUrls,
      };
      const html = await md.renderAsync(src, env);
      if (env.frontmatter?.search === false) continue;

      const pageTitle =
        typeof env.frontmatter?.title === "string"
          ? env.frontmatter.title
          : (env.title ?? "");
      const pagePath = docPath(base, relativePath);

      const headingRE = /<h([1-3])[^>]*\bid="([^"]*)"[^>]*>([\s\S]*?)<\/h\1>/g;
      const matches = [...html.matchAll(headingRE)];
      const parts = matches.length
        ? matches.map((m, i) => ({
            anchor: m[1] === "1" ? "" : m[2],
            heading: m[1] === "1" ? pageTitle : toText(m[3]),
            body: html.slice(
              m.index + m[0].length,
              matches[i + 1]?.index ?? html.length,
            ),
          }))
        : [{ anchor: "", heading: pageTitle, body: html }];

      for (const part of parts) {
        sections.push({
          id: `${pagePath}#${part.anchor}`,
          path: pagePath,
          anchor: part.anchor,
          pageTitle,
          heading: part.heading,
          text: toText(part.body),
        });
      }
    }
    return sections;
  }

  function invalidateModule(server: ViteDevServer, id: string) {
    const { moduleGraph } = server.environments.client;
    const mod = moduleGraph.getModuleById(id);
    if (mod) moduleGraph.invalidateModule(mod);
  }

  /** Drops the virtual modules and every compiled page. */
  function invalidateDocs(server: ViteDevServer) {
    const { moduleGraph } = server.environments.client;

    invalidateModule(server, RESOLVED_VIRTUAL_MODULE_ID);
    invalidateModule(server, RESOLVED_SEARCH_MODULE_ID);

    for (const mod of moduleGraph.idToModuleMap.values()) {
      if (mod.id && isDocFile(mod.id)) moduleGraph.invalidateModule(mod);
    }
  }

  function virtualModuleCode() {
    const themeConfig = (siteConfig.site.themeConfig ??
      {}) as DefaultTheme.Config;

    // `import.meta.glob` only works in code Vite transforms, which excludes the
    // prebuilt components in this package.
    const relativeSrcDir = path.posix.relative(viteRoot, srcDir);
    const srcPrefix = relativeSrcDir ? `/${relativeSrcDir}/` : "/";

    return `
export const base = ${JSON.stringify(base)};
export const sidebar = ${JSON.stringify(withBaseSidebar(themeConfig.sidebar ?? [], base))};
export const srcPrefix = ${JSON.stringify(srcPrefix)};
export const modules = import.meta.glob(${JSON.stringify(srcPrefix + "**/*.md")});

export function onPageUpdate(file, callback) {
  if (!import.meta.hot) return () => {};

  // plugin-vue swaps the rendered component in place, but the meta exports
  // are only read on load. Re-import past the browser's module cache; Vite
  // strips the \`t\` query before transforming.
  const handler = async (payload) => {
    if (!payload.updates.some((u) => u.acceptedPath.split("?")[0] === file)) return;
    callback(await import(/* @vite-ignore */ file + "?t=" + Date.now()));
  };
  import.meta.hot.on("vite:afterUpdate", handler);
  return () => import.meta.hot.off("vite:afterUpdate", handler);
}
`;
  }

  return {
    name: "coalesce-vue-docs",
    enforce: "pre",

    config(config) {
      const root = config.root ?? process.cwd();
      const dir = normalizePath(path.resolve(root, options.root ?? "docs"));

      return {
        // The prebuilt components import the virtual module, which the
        // dependency optimizer cannot resolve.
        optimizeDeps: {
          exclude: ["coalesce-vue-docs", "coalesce-vue-docs/vuetify"],
          include: ["coalesce-vue-docs > minisearch"],
        },
        server: {
          watch: {
            // Written by `resolveConfig` and by a standalone `vitepress build`.
            ignored: [
              `${dir}/.vitepress/cache/**`,
              `${dir}/.vitepress/dist/**`,
            ],
          },
        },
      };
    },

    async configResolved(config) {
      viteRoot = normalizePath(config.root);
      docsRoot = path.resolve(config.root, options.root ?? "docs");
      command = config.command;
      await loadDocsConfig(config.mode);

      if (srcDir !== viteRoot && !srcDir.startsWith(viteRoot + "/")) {
        throw new Error(
          `coalesce-vue-docs: the docs directory ${srcDir} must be inside the Vite root ${viteRoot}.`,
        );
      }

      const vue = config.plugins.find((p) => p.name === "vite:vue");
      if (!vue) {
        throw new Error(
          "coalesce-vue-docs: vitepressDocs() requires @vitejs/plugin-vue.",
        );
      }

      // Pages reach plugin-vue as `.md` files holding compiled SFC source.
      const api = vue.api as { include: FilterPattern | undefined };
      const include = api.include ?? /\.vue$/;
      api.include = [
        ...(Array.isArray(include) ? include : [include]),
        new RegExp(`^${escapeRegExp(srcDir)}/(?!\\.vitepress/).*\\.md$`),
      ];
    },

    configureServer(server) {
      // The config file and its imports are outside the module graph, so watch
      // them by hand and rebuild the renderer when they change.
      const configFiles = [siteConfig.configPath, ...siteConfig.configDeps]
        .filter((file): file is string => !!file)
        .map(normalizePath);

      if (!configFiles.length) return;

      server.watcher.add(configFiles);
      server.watcher.on("change", async (file) => {
        if (!configFiles.includes(normalizePath(file))) return;
        await loadDocsConfig(server.config.mode);
        invalidateDocs(server);
        server.environments.client.hot.send({ type: "full-reload" });
      });
    },

    resolveId(id) {
      if (id === VIRTUAL_MODULE_ID) return RESOLVED_VIRTUAL_MODULE_ID;
      if (id === SEARCH_MODULE_ID) return RESOLVED_SEARCH_MODULE_ID;
    },

    async load(id) {
      if (id === RESOLVED_VIRTUAL_MODULE_ID) return virtualModuleCode();
      if (id === RESOLVED_SEARCH_MODULE_ID) {
        return `export default ${JSON.stringify(await buildSearchIndex())};`;
      }
    },

    async transform(code, id) {
      if (!isDocFile(id)) return;

      const doc = await compileDoc(code, normalizePath(id));

      // `<<<` code snippets pull in files that nothing else imports.
      for (const include of doc.includes) {
        this.addWatchFile(include);
      }

      return { code: doc.sfc, map: null };
    },

    // plugin-vue diffs the SFC blocks to decide what to hot-update, and for a
    // non-`.vue` file it caches whatever `read()` returns. Give it compiled
    // source, or it would parse raw markdown as an SFC and see every block
    // disappear.
    async handleHotUpdate(ctx) {
      if (!isDocFile(ctx.file)) return;

      // Picked up on the next page load; not worth a reload of its own.
      invalidateModule(ctx.server, RESOLVED_SEARCH_MODULE_ID);

      const read = ctx.read;
      const file = normalizePath(ctx.file);
      ctx.read = async () => (await compileDoc(await read(), file)).sfc;
    },

    closeBundle() {
      if (command === "build") disposeMdItInstance();
    },
  };
}

/** Rendered HTML to plain text for the search index. */
function toText(html: string): string {
  return (
    html
      .replace(/<button[\s\S]*?<\/button>/g, " ")
      .replace(/<span class="lang">[\s\S]*?<\/span>/g, " ")
      // Keep table rows and cells apart, or a table reads as one run-on line.
      .replace(/<\/t[hd]>\s*<\/tr>/g, ". ")
      .replace(/<\/t[hd]>/g, ": ")
      .replace(/<\/(p|li|tr|h[1-6])>/g, ". ")
      .replace(/<[^>]+>/g, " ")
      .replace(/\s+([.:])/g, "$1")
      .replace(/([.:])(\s*[.:])+/g, "$1")
      .replace(/&ZeroWidthSpace;|&#8203;|​/g, "")
      .replace(/&nbsp;/g, " ")
      .replace(/&lt;/g, "<")
      .replace(/&gt;/g, ">")
      .replace(/&quot;/g, '"')
      .replace(/&#39;/g, "'")
      .replace(/&amp;/g, "&")
      .replace(/\s+/g, " ")
      .trim()
  );
}

function escapeRegExp(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

const scriptCloseRE = /<\/script>/;
const scriptSetupRE = /<\s*script[^>]*\bsetup\b[^>]*/;
const scriptLangTsRE = /<\s*script[^>]*\blang=['"]ts['"][^>]*/;
const defaultExportRE = /((?:^|\n|;)\s*)export(\s*)default/;
const namedDefaultExportRE = /((?:^|\n|;)\s*)export(.+)as(\s*)default/;

/**
 * Adds the page's meta as named exports. An SFC may only have one non-setup
 * script, so this merges into the page's own `<script>` block where it has
 * one, the same way VitePress injects `__pageData`.
 */
function injectDocMeta(scripts: string[], meta: DocMeta) {
  const code = `
export const meta = JSON.parse(${JSON.stringify(JSON.stringify(meta))});
export const frontmatter = meta.frontmatter;
export const headers = meta.headers;`;

  const existing = scripts.findIndex(
    (tag) => scriptCloseRE.test(tag) && !scriptSetupRE.test(tag),
  );

  if (existing > -1) {
    const tag = scripts[existing];
    const hasDefaultExport =
      defaultExportRE.test(tag) || namedDefaultExportRE.test(tag);
    scripts[existing] = tag.replace(
      scriptCloseRE,
      code +
        (hasDefaultExport
          ? ""
          : `\nexport default { name: ${JSON.stringify(meta.relativePath)} };`) +
        `</script>`,
    );
  } else {
    const isTs = scripts.some((tag) => scriptLangTsRE.test(tag));
    scripts.unshift(
      `<script ${isTs ? 'lang="ts"' : ""}>${code}
export default { name: ${JSON.stringify(meta.relativePath)} };</script>`,
    );
  }
}
