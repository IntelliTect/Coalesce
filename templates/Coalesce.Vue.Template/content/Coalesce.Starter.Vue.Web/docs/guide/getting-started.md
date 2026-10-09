# Getting started

## Writing pages

Pages are markdown files under `docs/`. Add each one to `themeConfig.sidebar` in `docs/.vitepress/config.ts` to list it in the sidebar.

::: tip
Markdown extensions from [VitePress](https://vitepress.dev/guide/markdown) work here, including custom containers like this one, code groups, and syntax highlighting.
:::

## Linking from the app

Use `c-docs-help` to show a page in a dialog from any screen:

```vue
<c-docs-help doc="/docs/guide/getting-started#writing-pages" />
```
