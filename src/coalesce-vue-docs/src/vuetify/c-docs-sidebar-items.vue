<template>
  <template
    v-for="(item, index) in items"
    :key="item.link ?? item.text ?? index"
  >
    <template v-if="item.items?.length && depth === 0">
      <div class="c-docs-sidebar__section">{{ item.text }}</div>
      <ul>
        <c-docs-sidebar-items :items="item.items" :depth="1" />
      </ul>
    </template>

    <li v-else-if="item.items?.length">
      <div class="c-docs-sidebar__group">{{ item.text }}</div>
      <ul class="c-docs-sidebar__nested">
        <c-docs-sidebar-items :items="item.items" :depth="depth + 1" />
      </ul>
    </li>

    <li v-else-if="item.link">
      <router-link
        :to="item.link"
        class="c-docs-sidebar__link"
        :class="{ 'c-docs-sidebar__link--current': item.link === route.path }"
      >
        {{ item.text }}
      </router-link>
    </li>
  </template>
</template>

<script setup lang="ts">
import { useRoute } from "vue-router";
import type { DefaultTheme } from "vitepress";

defineOptions({ name: "CDocsSidebarItems" });

withDefaults(
  defineProps<{ items: DefaultTheme.SidebarItem[]; depth?: number }>(),
  { depth: 0 },
);

// Not router-link's active class: that counts the docs index as active on
// every page under it.
const route = useRoute();
</script>
