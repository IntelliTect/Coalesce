<template>
  <nav class="c-docs-sidebar" aria-label="Documentation">
    <c-docs-sidebar-items :items="items ?? sidebarFor(route.path)" />
  </nav>
</template>

<script setup lang="ts">
import { useRoute } from "vue-router";
import type { DefaultTheme } from "vitepress";

import { sidebarFor } from "./docs";
import CDocsSidebarItems from "./c-docs-sidebar-items.vue";

defineOptions({ name: "CDocsSidebar" });

defineProps<{
  /** Defaults to the sidebar in the VitePress config for the current route. */
  items?: DefaultTheme.SidebarItem[];
}>();

const route = useRoute();
</script>
