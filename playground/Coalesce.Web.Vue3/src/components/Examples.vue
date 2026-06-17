<template>
  <v-container fluid class="pa-0">
    <div class="d-flex" style="position: relative">
      <v-sheet
        v-if="!mobile"
        class="d-flex elevation-3"
        style="
          height: calc(100vh - var(--v-layout-top));
          position: sticky;
          top: var(--v-layout-top);
        "
      >
        <v-list density="compact" style="width: 220px">
          <template v-for="dir in pages" :key="dir.name">
            <div
              class="font-weight-medium text-uppercase mt-3 mx-2 pt-3 pb-1"
              style="border-top: 1px solid rgba(var(--v-theme-on-surface), 0.3)"
            >
              {{ dir.name }}
            </div>
            <v-list-item
              v-for="page in dir.children"
              :key="page.path"
              :to="'/examples/' + page.path"
              :title="page.name"
              class="pl-8"
            >
            </v-list-item>
          </template>
        </v-list>
      </v-sheet>

      <v-navigation-drawer v-else v-model="drawer" temporary width="220">
        <v-list density="compact">
          <template v-for="dir in pages" :key="dir.name">
            <div
              class="font-weight-medium text-uppercase mt-3 mx-2 pt-3 pb-1"
              style="border-top: 1px solid rgba(var(--v-theme-on-surface), 0.3)"
            >
              {{ dir.name }}
            </div>
            <v-list-item
              v-for="page in dir.children"
              :key="page.path"
              :to="'/examples/' + page.path"
              :title="page.name"
              class="pl-8"
              @click="drawer = false"
            >
            </v-list-item>
          </template>
        </v-list>
      </v-navigation-drawer>

      <div class="pa-3 flex-grow-1">
        <v-btn
          v-if="mobile"
          icon="fa fa-bars"
          variant="text"
          size="small"
          class="mb-2"
          @click="drawer = !drawer"
        />
        <router-view v-slot="{ Component }">
          <div v-if="!Component">
            <div class="mt-10 mx-auto text-center">
              Select an item on the left
            </div>
          </div>
          <component :is="Component" />
        </router-view>
      </div>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";

const { mobile } = useDisplay();
const drawer = ref(true);

const examples = import.meta.glob("@/examples/**/*.vue");
const pages = computed(() => {
  const paths = Object.entries(examples).map((e) =>
    e[0].replace("/src/examples/", ""),
  );
  const directories = new Set(paths.map((e) => e.substring(0, e.indexOf("/"))));
  return [...directories.values()]
    .map((dir) => ({
      name: dir,
      children: paths
        .filter((e) => e.startsWith(dir + "/"))
        .map((x) => {
          const path = x.replace(".vue", "");
          return {
            path: path,
            name: path.replace(dir + "/", "").replace("-examples", ""),
          };
        })
        .sort((a, b) => a.name.localeCompare(b.name)),
    }))
    .sort((a, b) => a.name.localeCompare(b.name));
});
</script>

<style scoped></style>
