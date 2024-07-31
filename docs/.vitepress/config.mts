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
    link + (path.extname(link) ? "" : ".md")
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
    path.resolve(__dirname, "../stacks/vue/coalesce-vue-vuetify/components")
  )
  .map((f) => autoTitle("/stacks/vue/coalesce-vue-vuetify/components/" + f));

function getComponentCategory(item: (typeof vuetifyComponents)[0]) {
  return item.text.startsWith("c-admin")
    ? "admin"
    : [
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
    path.resolve(
      __dirname,
      "../modeling/model-components/attributes"
    )
  )
  .map((f) =>
    autoTitle("/modeling/model-components/attributes/" + f)
  );

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Coalesce",
  description: "Documentation for Coalesce by IntelliTect",
  base: "/Coalesce/",
  head: [["link", { rel: "shortcut icon", href: "/Coalesce/favicon.ico" }]],
  markdown: {
    config(md) {
      registerImportMdPlugin(md);
    },
    theme: "dark-plus",
  },

  themeConfig: {
    logo: "/coalesce-icon-color.svg",
    outline: {
      level: [2, 3],
    },
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: "Home", link: "/" },
      { text: "Guide", link: "/introduction", activeMatch: "/.+" },
    ],

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
          autoTitle("/modeling/model-types/entities"),
          autoTitle("/modeling/model-types/external-types"),
          autoTitle("/modeling/model-types/services"),
          {
            text: "Advanced",
            collapsed: true,
            items: [
              autoTitle("/modeling/model-types/dtos"),
              autoTitle("/modeling/model-types/standalone-entities"),
            ]
          },
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
              ...attributes.filter(f => !f.deprecated),
              {
                text: "Deprecated",
                collapsed: true,
                items: attributes.filter(f => f.deprecated),
              }]
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
              ...vuetifyComponents.filter((i) => getComponentCategory(i) == "display"),
            ],
          },
          {
            text: "Input",
            collapsed: false,
            items: [
              ...vuetifyComponents.filter((i) => getComponentCategory(i) == "input").sort((a,b) => {
                if (a.text == 'c-input') return -1;
                return a.text.localeCompare(b.text);
              }),
            ],
          },
          {
            text: "Admin",
            collapsed: true,
            items: [
              ...vuetifyComponents.filter((i) => getComponentCategory(i) == "admin"),
            ],
          },
        ],
      },
      {
        text: "Topics",
        items: [
          { text: "Config: ASP.NET Core", link: "/topics/startup" },
          autoTitle("/topics/audit-logging"),
          { text: "Config: Code Gen", link: "/topics/coalesce-json" },
          autoTitle("/concepts/include-tree"),
          autoTitle("/concepts/includes"),

          { text: "Vue 2 to Vue 3", link: "/stacks/vue/vue2-to-vue3" },
          {
            text: "Generated Code",
            link: "/stacks/agnostic/generation",
            collapsed: true,
            items: [autoTitle("/stacks/agnostic/dtos")],
          },
        ].sort((a, b) => a.text.localeCompare(b.text)),
      },
    ],

    socialLinks: [
      { icon: "github", link: "https://github.com/IntelliTect/Coalesce" },
    ],

    search: {
      provider: "algolia",
      options: {
        appId: "SDGLJOI8GP",
        apiKey: "7aac3b70e2be40bd6bb55bc603e7bf46",
        indexName: "coalesce",
      },
    },
  },
});
