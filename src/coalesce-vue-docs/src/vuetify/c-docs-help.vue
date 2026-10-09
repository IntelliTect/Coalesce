<template>
  <v-overlay
    v-model="open"
    content-class="c-docs-help__panel"
    transition="slide-x-reverse-transition"
    scroll-strategy="block"
  >
    <template #activator="{ props: activatorProps }">
      <slot name="activator" :props="activatorProps">
        <v-btn
          v-bind="activatorProps"
          prepend-icon="$info"
          color="primary"
          size="small"
          variant="outlined"
          text="Help"
          :aria-label="title ? `Help: ${title}` : 'Help'"
        />
      </slot>
    </template>

    <v-card class="c-docs-help" elevation="10">
      <v-card-title class="c-docs-help__header">
        <v-btn
          v-if="history.length"
          icon="$prev"
          variant="text"
          size="small"
          aria-label="Back"
          @click="current = history.pop()!"
        />
        <span class="c-docs-help__title">{{ meta?.title ?? title }}</span>
        <v-btn
          v-if="page"
          :to="current"
          variant="text"
          size="small"
          append-icon="$next"
          text="Open in docs"
          @click="open = false"
        />
        <v-btn
          icon="$close"
          variant="text"
          size="small"
          aria-label="Close"
          @click="open = false"
        />
      </v-card-title>
      <v-divider />

      <v-card-text ref="content" class="c-docs-help__content" @click="onClick">
        <component :is="component" v-if="component" />

        <v-alert v-else-if="missing" type="warning" variant="tonal">
          No documentation page exists at <code>{{ current }}</code>
        </v-alert>

        <div v-else class="c-docs-layout__loading">
          <v-progress-circular indeterminate size="28" width="3" />
        </div>
      </v-card-text>
    </v-card>
  </v-overlay>
</template>

<script setup lang="ts">
import { nextTick, ref, useTemplateRef, watch } from "vue";
import { useRouter } from "vue-router";

import { findDocPage, useDocPage } from "./docs";
import { useDocsContent } from "./useDocsContent";

defineOptions({ name: "CDocsHelp" });

const props = defineProps<{
  /** App URL of the doc page to show, e.g. `/docs/guide/orders#refunds`. */
  doc: string;
  /** Names the page in the default activator's label, and titles the panel until the page loads. */
  title?: string;
}>();

const emit = defineEmits<{
  /** The page's content was rendered or hot-updated. */
  rendered: [el: HTMLElement];
}>();

const open = defineModel<boolean>({ default: false });

const router = useRouter();
const content = useTemplateRef<{ $el: HTMLElement }>("content");

/**
 * The page shown in the panel, which may differ from `doc` after following a
 * link. Kept after closing so the content doesn't vanish during the transition.
 */
const current = ref<string>();
const history = ref<string[]>([]);

watch(
  open,
  (open) => {
    if (!open) return;
    current.value = props.doc;
    history.value = [];
  },
  { immediate: true },
);

const pathname = () =>
  current.value ? new URL(current.value, location.origin).pathname : undefined;

const { component, meta, missing, page } = useDocPage(pathname);

const { onClick } = useDocsContent({
  baseDir: () => meta.value?.docDir,
  path: () => meta.value?.docPath,
  navigate(url) {
    const target = url.pathname + url.search + url.hash;
    if (url.pathname === pathname()) {
      current.value = target;
      scrollToHash();
    } else if (findDocPage(url.pathname)) {
      history.value.push(current.value!);
      current.value = target;
    } else {
      open.value = false;
      void router.push(target);
    }
  },
});

watch(
  meta,
  async (meta, oldMeta) => {
    const el = content.value?.$el;
    if (!meta || !el) return;
    await nextTick();
    if (meta.docPath !== oldMeta?.docPath) scrollToHash();
    emit("rendered", el);
  },
  { flush: "post" },
);

function scrollToHash() {
  const el = content.value?.$el;
  if (!el) return;

  const hash = current.value
    ? new URL(current.value, location.origin).hash
    : "";
  const target = hash
    ? el.querySelector<HTMLElement>(
        `[id="${CSS.escape(decodeURIComponent(hash.slice(1)))}"]`,
      )
    : null;

  if (target) target.scrollIntoView();
  else el.scrollTop = 0;
}
</script>
