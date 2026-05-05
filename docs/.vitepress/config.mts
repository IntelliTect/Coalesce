import { defineConfig } from "vitepress";
import { registerImportMdPlugin } from "./importMdPlugin";

import path from "path";
import fs from "fs";
import url from "url";
import matter from "gray-matter";

function autoTitle(link: string) {
  const fullPath = path.join(
    url.fileURLToPath(import.meta.url),
    "../../",
    link + (path.extname(link) ? "" : ".md"),
  );

  const { data, content } = matter(fs.readFileSync(fullPath));

  // Check if title exists in frontmatter
  if (data && data.title && typeof data.title === "string") {
    return { text: data.title, link, deprecated: data.deprecated };
  }

  // If title is not in frontmatter, try to find the first H1 heading
  const h1Match = content.match(/^#\s+(.+)/m);
  if (h1Match && h1Match[1] && typeof h1Match[1] === "string") {
    return { text: h1Match[1], link, deprecated: data.deprecated };
  }

  throw Error("Cannot find a title for " + link);
}

const vuetifyComponents = fs
  .readdirSync(
    path.resolve(__dirname, "../stacks/vue/coalesce-vue-vuetify/components"),
  )
  .map((f) => autoTitle("/stacks/vue/coalesce-vue-vuetify/components/" + f));

function getComponentCategory(item: (typeof vuetifyComponents)[0]) {
  return item.text.startsWith("c-admin")
    ? "admin"
    : [
          "c-app-update-alert",
          "c-display",
          "c-loader-status",
          "c-list-range-display",
          "c-table",
        ].includes(item.text)
      ? "display"
      : "input";
}

const attributes = fs
  .readdirSync(
    path.resolve(__dirname, "../modeling/model-components/attributes"),
  )
  .map((f) => autoTitle("/modeling/model-components/attributes/" + f));

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Coalesce",
  description: "Documentation for Coalesce by IntelliTect",
  head: [["link", { rel: "shortcut icon", href: "/favicon.ico" }]],
  markdown: {
    config(md) {
      registerImportMdPlugin(md);
    },
    theme: "dark-plus",
    codeTransformers: [
      // Color Vue component tags green (like VS Code with Volar)
      {
        name: "vue-component-tag-color",
        code(hast: any) {
          if (this.options.lang !== "vue") return;
          const componentTagRegex = /^[A-Z]|^[a-z]+-[a-z]/;

          // Walk each line and look for patterns: <ComponentName or </ComponentName
          for (const line of hast.children) {
            if (line.type !== "element") continue;
            const spans = line.children?.filter(
              (c: any) => c.type === "element",
            );
            if (!spans) continue;

            for (let i = 0; i < spans.length; i++) {
              const span = spans[i];
              const prevSpan = spans[i - 1];
              const text = span.children?.[0]?.value;
              const prevText = prevSpan?.children?.[0]?.value;

              if (!text) continue;

              // If previous token ends with < or </ and this token matches component pattern
              if (
                prevText &&
                (prevText.endsWith("<") || prevText.endsWith("</")) &&
                componentTagRegex.test(text)
              ) {
                span.properties = span.properties || {};
                span.properties.style = "color:#4EC9B0";
              }
            }
          }
        },
      },
      // Color bound attribute values like TypeScript expressions
      {
        name: "vue-bound-attr-ts-color",
        code(hast: any) {
          if (this.options.lang !== "vue") return;
          for (const line of hast.children) {
            if (line.type !== "element") continue;
            const spans = line.children?.filter(
              (c: any) => c.type === "element",
            );
            if (!spans) continue;

            for (let i = 0; i < spans.length; i++) {
              const span = spans[i];
              const text = span.children?.[0]?.value;
              if (!text) continue;

              // Check if previous spans indicate this is a bound attribute value
              // Pattern: attr name starting with : or @, then =, then "value"
              const prevSpan = spans[i - 1];
              const prevPrevSpan = spans[i - 2];
              const prevText = prevSpan?.children?.[0]?.value;
              const prevPrevText = prevPrevSpan?.children?.[0]?.value;

              const isBoundValue =
                prevText === "=" &&
                prevPrevText &&
                (prevPrevText.trimStart().startsWith(":") ||
                  prevPrevText.trimStart().startsWith("@") ||
                  prevPrevText.trimStart().startsWith("v-"));

              if (!isBoundValue) continue;

              // Replace the single string span with multiple colored spans
              const inner = text.slice(1, -1); // strip quotes
              if (!inner) continue;

              // Tokenize the expression simply
              const tokens: { text: string; color: string }[] = [];
              const tokenRegex =
                /(\$?\w+)|(\.\s*)|([[\](),])|(\s+)|(["'`][^"'`]*["'`])/g;
              let match;
              let lastIndex = 0;

              while ((match = tokenRegex.exec(inner)) !== null) {
                if (match.index > lastIndex) {
                  tokens.push({
                    text: inner.slice(lastIndex, match.index),
                    color: "#D4D4D4",
                  });
                }
                if (match[1]) {
                  // identifier
                  tokens.push({ text: match[1], color: "#9CDCFE" });
                } else if (match[2]) {
                  // dot
                  tokens.push({ text: match[2], color: "#D4D4D4" });
                } else if (match[3]) {
                  // brackets/parens
                  tokens.push({ text: match[3], color: "#D4D4D4" });
                } else if (match[4]) {
                  // whitespace
                  tokens.push({ text: match[4], color: "#D4D4D4" });
                } else if (match[5]) {
                  // string literal
                  tokens.push({ text: match[5], color: "#CE9178" });
                }
                lastIndex = match.index + match[0].length;
              }
              if (lastIndex < inner.length) {
                tokens.push({
                  text: inner.slice(lastIndex),
                  color: "#D4D4D4",
                });
              }

              // Build new children: opening quote + tokens + closing quote
              const newChildren: any[] = [
                {
                  type: "element",
                  tagName: "span",
                  properties: { style: "color:#D4D4D4" },
                  children: [{ type: "text", value: text[0] }],
                },
                ...tokens.map((t) => ({
                  type: "element",
                  tagName: "span",
                  properties: { style: `color:${t.color}` },
                  children: [{ type: "text", value: t.text }],
                })),
                {
                  type: "element",
                  tagName: "span",
                  properties: { style: "color:#D4D4D4" },
                  children: [{ type: "text", value: text[text.length - 1] }],
                },
              ];

              // Replace the span in the line's children
              const idx = line.children.indexOf(span);
              if (idx !== -1) {
                line.children.splice(idx, 1, ...newChildren);
              }
            }
          }
        },
      },
      // Remove code lines annotated with [!code hide]
      {
        name: "remove-code-hide",
        code(hast: any) {
          const getTextContent = (element: any): string => {
            if (element.type === "text") return element.value;
            if (element.type === "element" && element.tagName === "span")
              return element.children.map(getTextContent).join("");
            return "";
          };

          const linesToKeep = [];
          for (let i = 0; i < hast.children.length; i++) {
            const child = hast.children[i];

            if (getTextContent(child).includes("[!code hide]")) {
              i++; // Skip the next element too (newline)
              continue;
            }

            linesToKeep.push(child);
          }

          hast.children = linesToKeep;
        },
      },
      // Wrap a code block in a `class` declaration so that TS class members highlight correctly.
      // This looks for blocks like ``` ts class-wrap ... ```
      {
        name: "wrap-transformer",
        preprocess(code: string, options: any) {
          // Check if the code block has the 'class-wrap' attribute
          if (options.meta?.__raw?.includes("class-wrap")) {
            // Inject class wrapper at the beginning and end
            const lines = code.split("\n");
            const wrappedCode = [
              "class x { // [!code hide]",
              ...lines,
              "} // [!code hide]",
            ].join("\n");
            return wrappedCode;
          }
          return code;
        },
      },
    ],
  },

  themeConfig: {
    logo: "/coalesce-icon-color.svg",
    outline: {
      level: [2, 3],
    },
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: "Home", link: "/" },
      {
        text: "Guide",
        link: "/introduction",
        activeMatch: "^/(?!changelog).+",
      },
      { text: "Changelog", link: "/changelog" },
    ],

    socialLinks: [
      { icon: "github", link: "https://github.com/IntelliTect/Coalesce" },
    ],

    search: {
      provider: "algolia",
      options: {
        appId: "NV8KF7QVGD",
        apiKey: "b44fafadc065e1db9f555d3349fcc1cf",
        indexName: "Coalesce Crawler",
      },
    },

    editLink: {
      pattern: "https://github.com/IntelliTect/Coalesce/blob/main/docs/:path",
    },

    sidebar: [
      {
        text: "Introduction",
        link: "/introduction",
        items: [
          { text: "Getting Started", link: "/stacks/vue/getting-started" },
        ],
      },
      {
        text: "Data Modeling",
        items: [
          {
            ...autoTitle("/modeling/model-types/crud"),
            collapsed: false,
            items: [
              autoTitle("/modeling/model-types/entities"),
              autoTitle("/modeling/model-types/standalone-entities"),
              autoTitle("/modeling/model-types/dtos"),
            ],
          },
          autoTitle("/modeling/model-types/services"),
          autoTitle("/modeling/model-types/simple-models"),
        ],
      },
      {
        text: "Model Customization",
        items: [
          // This is deliberately here in a prominent place because its important,
          // even though the path is inconsistent with these other pages.
          autoTitle("/topics/security"),
          autoTitle("/modeling/model-components/properties"),
          {
            text: "Attributes",
            link: "/modeling/model-components/attributes.html",
            collapsed: true,
            items: [
              ...attributes.filter((f) => !f.deprecated),
              {
                text: "Deprecated",
                collapsed: true,
                items: attributes.filter((f) => f.deprecated),
              },
            ],
          },
          autoTitle("/modeling/model-components/methods"),
          autoTitle("/modeling/model-components/data-sources"),
          autoTitle("/modeling/model-components/behaviors"),
        ],
      },
      {
        text: "Frontend - Vue",
        link: "/stacks/vue/overview",
        // collapsed: false,
        items: [
          { text: "Metadata", link: "/stacks/vue/layers/metadata" },
          { text: "Models", link: "/stacks/vue/layers/models" },
          { text: "API Clients", link: "/stacks/vue/layers/api-clients" },
          { text: "View Models", link: "/stacks/vue/layers/viewmodels" },
          { text: "Admin Pages", link: "/stacks/vue/admin-pages" },
        ],
      },
      {
        text: "Vuetify Components",
        collapsed: true,
        link: "/stacks/vue/coalesce-vue-vuetify/overview",
        items: [
          {
            text: "Display",
            collapsed: false,
            items: [
              ...vuetifyComponents.filter(
                (i) => getComponentCategory(i) == "display",
              ),
            ],
          },
          {
            text: "Input",
            collapsed: false,
            items: [
              ...vuetifyComponents
                .filter((i) => getComponentCategory(i) == "input")
                .sort((a, b) => {
                  if (a.text == "c-input") return -1;
                  return a.text.localeCompare(b.text);
                }),
            ],
          },
          {
            text: "Admin",
            collapsed: true,
            items: [
              ...vuetifyComponents.filter(
                (i) => getComponentCategory(i) == "admin",
              ),
            ],
          },
        ],
      },
      {
        text: "Topics",
        items: [
          autoTitle("/concepts/include-tree"),
          autoTitle("/concepts/includes"),

          autoTitle("/topics/analyzers"),
          autoTitle("/topics/coalesce-json"),
          autoTitle("/topics/eslint-plugin"),
          autoTitle("/topics/immutability"),
          autoTitle("/topics/startup"),
          autoTitle("/topics/audit-logging"),
          autoTitle("/topics/mcp-server"),
          autoTitle("/topics/upgrading"),
          autoTitle("/topics/coalesce-swashbuckle"),
          autoTitle("/topics/template-features"),
          autoTitle("/topics/vite-integration"),
          autoTitle("/topics/working-with-dates"),

          autoTitle("/stacks/vue/vue2-to-vue3"),
          {
            text: "Generated Code",
            link: "/stacks/agnostic/generation",
            collapsed: true,
            items: [autoTitle("/stacks/agnostic/dtos")],
          },
        ].sort((a, b) => a.text.localeCompare(b.text)),
      },
    ],
  },
});
