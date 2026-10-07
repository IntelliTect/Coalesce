<template>
  <div ref="content" class="c-docs-layout__content" @click="onClick">
    <template v-if="component">
      <div v-if="trail.length" class="c-docs-layout__trail">
        {{ trail.join(" › ") }}
      </div>

      <component :is="component" />

      <nav
        v-if="pager.prev || pager.next"
        class="c-docs-pager"
        aria-label="Previous and next pages"
      >
        <router-link
          v-if="pager.prev"
          :to="pager.prev.link"
          class="c-docs-pager__link"
        >
          <span class="c-docs-pager__label">Previous</span>
          <span class="c-docs-pager__title">{{ pager.prev.text }}</span>
        </router-link>
        <router-link
          v-if="pager.next"
          :to="pager.next.link"
          class="c-docs-pager__link c-docs-pager__link--next"
        >
          <span class="c-docs-pager__label">Next</span>
          <span class="c-docs-pager__title">{{ pager.next.text }}</span>
        </router-link>
      </nav>
    </template>

    <v-alert v-else-if="missing" type="warning" variant="tonal">
      <div class="font-weight-bold">Page not found</div>
      <div>
        No documentation page exists at <code>{{ route.path }}</code>
      </div>
      <template #append>
        <v-btn :to="docsManifest.base" variant="text">Docs home</v-btn>
      </template>
    </v-alert>

    <div v-else class="c-docs-layout__loading">
      <v-progress-circular indeterminate size="28" width="3" />
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  computed,
  inject,
  nextTick,
  onBeforeUnmount,
  useTemplateRef,
  watch,
} from "vue";
import { useRoute } from "vue-router";

import {
  docsManifest,
  docsShellState,
  findDocPage,
  pagerFor,
  sidebarTrail,
  useDocPage,
} from "./docs";
import { useDocsContent } from "./useDocsContent";

defineOptions({ name: "CDocsPage" });

const emit = defineEmits<{
  /** The page's content was rendered or hot-updated. */
  rendered: [el: HTMLElement];
}>();

const route = useRoute();
const content = useTemplateRef<HTMLElement>("content");

// The table of contents is drawn by the layout, which outlives this component.
const shell = inject(docsShellState, undefined);

const { component, meta, missing, page } = useDocPage(() => route.path);

const pager = computed(() => (page.value ? pagerFor(page.value.path) : {}));

// Sidebar groups have no page of their own, so this is plain text, not links.
const trail = computed(() =>
  page.value && page.value !== findDocPage(docsManifest.base)
    ? sidebarTrail(page.value.path)
    : [],
);

const { onClick } = useDocsContent({ baseDir: () => meta.value?.docDir });

// Restored when leaving the docs. Doc navigations reuse this component.
const originalTitle = document.title;
onBeforeUnmount(() => (document.title = originalTitle));

watch(
  meta,
  async (meta, oldMeta) => {
    if (shell) shell.headers.value = meta?.headers ?? [];
    document.title = [meta?.title, originalTitle].filter((t) => t).join(" - ");

    if (!meta || !content.value) return;
    await nextTick();
    // An HMR update to the same page keeps the reader's scroll position.
    if (meta.docPath !== oldMeta?.docPath) scrollToHash();
    emit("rendered", content.value);
  },
  { flush: "post" },
);

watch(() => route.hash, scrollToHash);

function scrollToHash() {
  if (!route.hash) {
    window.scrollTo({ top: 0 });
    return;
  }
  document
    .getElementById(decodeURIComponent(route.hash.slice(1)))
    ?.scrollIntoView();
}
</script>
