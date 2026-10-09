import { defineConfig } from "vitepress";

// Read by `vitepressDocs()` in vite.config.ts, which renders these pages at `base` inside the app.
export default defineConfig({
  title: "Coalesce.Starter.Vue",
  base: "/docs/",
  cleanUrls: true,

  themeConfig: {
    sidebar: [
      {
        text: "Introduction",
        items: [{ text: "Overview", link: "/" }],
      },
      {
        text: "Guide",
        items: [{ text: "Getting started", link: "/guide/getting-started" }],
      },
    ],
  },

  markdown: {
    // Builds the "On this page" table of contents.
    headers: { level: [2, 3] },
  },
});
