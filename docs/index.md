---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "Coalesce"
  text: Accelerated Web App Development
  tagline: ASP.NET Core • EF Core • Vue.js • TypeScript
  image:
    src: /coalesce-icon-color.svg
    alt: Coalesce
  actions:
    - theme: brand
      text: Introduction
      link: /introduction
    - theme: alt
      text: Get Started
      link: /stacks/vue/getting-started

features:
  - title: 🖨 Code Generated
    details: Design your data model and build awesome pages. Coalesce generates the boring parts in the middle.
  - title: 🧩 Extensible
    details: All functionality in Coalesce is configurable or overridable. You'll never be boxed in or get stuck.
  - title: 🔒 Secure
    details: Customization of table-level, row-level, and property-level security are all built-in. <a href="./topics/security.html">Read More.</a>
---

<SiteFooter />

<style>
.VPHero .text {
  font-size: 42px;
  line-height: 1.1;
  padding: 16px 0;
  color: var(--logo-text-color);
}

.VPHero .VPImage {
  width: 100%
}
</style>