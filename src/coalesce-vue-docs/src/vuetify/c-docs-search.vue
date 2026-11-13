<template>
  <button type="button" class="c-docs-search__trigger" @click="show = true">
    <v-icon :icon="searchIcon" size="small" />
    <span>Search docs</span>
    <kbd class="c-docs-search__key">/</kbd>
  </button>

  <v-dialog
    v-model="show"
    max-width="680"
    scrollable
    content-class="c-docs-search-dialog"
  >
    <v-card class="c-docs-search">
      <v-card-title class="pa-3">
        <v-text-field
          ref="field"
          v-model="query"
          density="comfortable"
          variant="outlined"
          placeholder="Search the docs"
          :prepend-inner-icon="searchIcon"
          aria-label="Search the docs"
          hide-details
          clearable
          @keydown.down.prevent="move(1)"
          @keydown.up.prevent="move(-1)"
          @keydown.enter.prevent="open(results[active])"
        />
      </v-card-title>

      <v-card-text v-if="query?.trim()" class="pa-2">
        <v-list density="compact" nav>
          <v-list-item
            v-for="(result, i) in results"
            :key="result.id"
            :active="i === active"
            @click="open(result)"
          >
            <div class="c-docs-search__heading">
              {{ result.heading }}
              <span
                v-if="result.heading !== result.pageTitle"
                class="c-docs-search__page"
              >
                · {{ result.pageTitle }}
              </span>
            </div>
            <!-- eslint-disable-next-line vue/no-v-html -- escaped in snippet() -->
            <div class="c-docs-search__snippet" v-html="snippet(result)"></div>
          </v-list-item>
        </v-list>
        <div v-if="index && !results.length" class="c-docs-search__empty">
          No matches
        </div>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import {
  computed,
  onBeforeUnmount,
  onMounted,
  ref,
  shallowRef,
  useTemplateRef,
  watch,
} from "vue";
import { useRouter } from "vue-router";
import type MiniSearch from "minisearch";

import type { DocSearchSection } from "../shared";

defineOptions({ name: "CDocsSearch" });

type Result = DocSearchSection & { terms: string[] };

// Vuetify has no search icon alias, and the app's icon set is unknown.
const searchIcon =
  "svg:M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z";

const router = useRouter();
const show = ref(false);
const query = ref("");
const active = ref(0);
const field = useTemplateRef<{ focus(): void; $el: HTMLElement }>("field");

// Loaded on first open, so pages that never search don't download the index.
const index = shallowRef<MiniSearch<DocSearchSection>>();

async function loadIndex() {
  if (index.value) return;
  const [{ default: MiniSearch }, { default: sections }] = await Promise.all([
    import("minisearch"),
    import("virtual:coalesce-vue-docs-search"),
  ]);
  const search = new MiniSearch<DocSearchSection>({
    fields: ["pageTitle", "heading", "text"],
    storeFields: ["path", "anchor", "pageTitle", "heading", "text"],
    searchOptions: {
      boost: { heading: 3, pageTitle: 2 },
      prefix: true,
      fuzzy: 0.2,
    },
  });
  search.addAll(sections);
  index.value = search;
}

const results = computed(() =>
  index.value && query.value?.trim()
    ? (index.value.search(query.value).slice(0, 12) as unknown as Result[])
    : [],
);

watch(results, () => (active.value = 0));

watch(show, (open) => {
  if (!open) return;
  void loadIndex();
  // Once the dialog has rendered its content. The last search is kept, and
  // selected so typing replaces it.
  setTimeout(() => {
    field.value?.focus();
    field.value?.$el.querySelector("input")?.select();
  });
});

function move(step: number) {
  if (!results.value.length) return;
  active.value =
    (active.value + step + results.value.length) % results.value.length;
}

const escapeHtml = (text: string) =>
  text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

/** The part of a section's text around its first match, with matches marked. */
function snippet(result: Result) {
  const text = result.text;
  const lower = text.toLowerCase();
  const hit = Math.min(
    ...result.terms
      .map((t) => lower.indexOf(t.toLowerCase()))
      .filter((i) => i >= 0),
  );
  const start = Number.isFinite(hit) ? Math.max(0, hit - 40) : 0;
  let excerpt = text.slice(start, start + 180);
  if (start > 0) excerpt = "…" + excerpt.replace(/^\S*\s/, "");
  if (start + 180 < text.length) excerpt = excerpt.replace(/\s\S*$/, "") + "…";

  const html = escapeHtml(excerpt);
  if (!result.terms.length) return html;
  // One pass, so a term can't match inside a <mark> added for another.
  const pattern = result.terms
    .map((t) => escapeHtml(t).replace(/[.*+?^${}()|[\]\\]/g, "\\$&"))
    .join("|");
  return html.replace(new RegExp(`(${pattern})`, "gi"), "<mark>$1</mark>");
}

function open(section: DocSearchSection | undefined) {
  if (!section) return;
  show.value = false;
  void router.push(section.path + (section.anchor ? "#" + section.anchor : ""));
}

// "/" opens search, unless the user is typing somewhere.
function onKeydown(event: KeyboardEvent) {
  if (event.key !== "/" || event.ctrlKey || event.metaKey || event.altKey) {
    return;
  }
  const target = event.target;
  if (
    target instanceof Element &&
    target.closest("input, textarea, select, [contenteditable]")
  ) {
    return;
  }
  event.preventDefault();
  show.value = true;
}
onMounted(() => window.addEventListener("keydown", onKeydown));
onBeforeUnmount(() => window.removeEventListener("keydown", onKeydown));
</script>
