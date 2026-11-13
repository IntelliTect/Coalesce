<template>
  <div class="c-docs-layout">
    <!-- The rail stretches to the full grid row so its divider runs the height
         of the page; the sticky child is what follows the scroll. -->
    <div
      class="c-docs-layout__rail c-docs-layout__rail--start"
      :class="{ 'c-docs-layout__rail--open': menuOpen }"
    >
      <!-- Only shown on narrow screens, where the sidebar collapses. -->
      <v-btn
        class="c-docs-layout__menu-toggle"
        variant="text"
        block
        :prepend-icon="menuOpen ? '$collapse' : '$menu'"
        :aria-expanded="menuOpen"
        @click="menuOpen = !menuOpen"
      >
        Menu
      </v-btn>
      <div class="c-docs-layout__sticky">
        <c-docs-search />
        <c-docs-sidebar />
      </div>
    </div>

    <!-- Unkeyed: one page component serves every doc URL and reloads from its
         own route watcher. Keying it would remount the content and blink the
         table of contents. -->
    <router-view />

    <div class="c-docs-layout__rail c-docs-layout__rail--end">
      <div class="c-docs-layout__sticky">
        <c-docs-toc :headers="headers" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { provide, ref, watch } from "vue";
import { onBeforeRouteUpdate, useRoute } from "vue-router";
import type { Header } from "vitepress";

import { canonicalDocLocation, docsShellState } from "./docs";
import CDocsSearch from "./c-docs-search.vue";
import CDocsSidebar from "./c-docs-sidebar.vue";
import CDocsToc from "./c-docs-toc.vue";

defineOptions({ name: "CDocsLayout" });

const route = useRoute();

const menuOpen = ref(false);
watch(
  () => route.path,
  () => (menuOpen.value = false),
);

// Written by the page component as each one loads.
const headers = ref<Header[]>([]);
provide(docsShellState, { headers });

// The route's `beforeEnter` only covers arriving from outside the docs.
onBeforeRouteUpdate(canonicalDocLocation);
</script>
