<template>
  <nav v-if="entries.length" class="c-docs-toc" aria-label="On this page">
    <div class="c-docs-toc__title">On this page</div>
    <ul class="c-docs-toc__list">
      <li
        v-for="entry in entries"
        :key="entry.slug"
        :class="`c-docs-toc__item c-docs-toc__item--level-${entry.level}`"
      >
        <a
          :href="`#${entry.slug}`"
          :class="{ 'c-docs-toc__link--active': entry.slug === activeSlug }"
          class="c-docs-toc__link"
        >
          {{ entry.title }}
        </a>
      </li>
    </ul>
  </nav>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";
import type { Header } from "vitepress";

import { tocEntries } from "./docs";

defineOptions({ name: "CDocsToc" });

const props = defineProps<{ headers: Header[] }>();

const entries = computed(() => tocEntries(props.headers));
const activeSlug = ref<string>();

let observer: IntersectionObserver | undefined;

function observe() {
  observer?.disconnect();
  if (!entries.value.length) return;

  const targets = entries.value
    .map((entry) => document.getElementById(entry.slug))
    .filter((el): el is HTMLElement => !!el);

  if (!targets.length) return;

  observer = new IntersectionObserver(
    (records) => {
      // The heading nearest the top of the viewport wins, so the highlight
      // doesn't jump to a later heading that is also visible.
      const visible = records
        .filter((record) => record.isIntersecting)
        .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0];
      if (visible) activeSlug.value = visible.target.id;
    },
    {
      rootMargin: `-${appBarHeight(targets[0])}px 0px -70% 0px`,
      threshold: 0,
    },
  );

  targets.forEach((target) => observer!.observe(target));
}

// Set by c-docs-layout; custom properties compute to their resolved value.
function appBarHeight(el: HTMLElement) {
  return (
    parseFloat(getComputedStyle(el).getPropertyValue("--c-docs-offset")) || 0
  );
}

watch(entries, () => nextTick(observe), { immediate: true });
onBeforeUnmount(() => observer?.disconnect());
</script>
