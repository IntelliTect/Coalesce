<template>
  <v-app>
    <v-app-bar>
      <v-toolbar-title style="line-height: 1">
        <router-link to="/" class="white--text">
          Coalesce Vue 3 Playground
        </router-link>
      </v-toolbar-title>

      <!-- <div class="nav-items"> -->
      <v-switch
        label="Dark Mode"
        v-model="darkMode"
        hide-details
        class="ml-2"
        density="compact"
      />

      <v-btn variant="text" to="/">Home</v-btn>
      <v-btn variant="text" to="/test">Test</v-btn>
      <v-btn variant="text" to="/test-setup">Test2</v-btn>
      <v-btn variant="text" to="/audit-logs">Audit</v-btn>
      <v-btn variant="text" to="/swagger">OpenAPI</v-btn>
      <v-btn variant="text" href="/coalesce-security">Security Overview</v-btn>

      <v-menu offset-y>
        <template #activator="{ props }">
          <v-btn variant="text" v-bind="props">
            Admin tables
            <i class="fa fa-caret-down pl-1"></i>
          </v-btn>
        </template>
        <v-list>
          <v-list-item
            v-for="t in Object.values($metadata.types).filter(
              (t) => t.type == 'model'
            )"
            :to="'/admin/' + t.name"
            :title="t.displayName"
          />
        </v-list>
      </v-menu>
      <!-- </div> -->
    </v-app-bar>

    <v-main>
      <router-view v-slot="{ Component }">
        <transition name="router-transition" mode="out-in" appear>
          <!-- https://stackoverflow.com/questions/52847979/what-is-router-view-key-route-fullpath -->
          <component ref="routerView" :is="Component" :key="$route.path" />
        </transition>
      </router-view>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useTheme } from "vuetify";
import $metadata from "@/metadata.g";

const theme = useTheme();
const darkMode = computed({
  get() {
    return theme.global.name.value == "dark";
  },
  set(v) {
    theme.global.name.value = v ? "dark" : "light";
    localStorage.setItem("dark", v.toString());
  },
});
</script>

<style lang="scss">
.router-transition-enter-active,
.router-transition-leave-active {
  // transition: 0.2s cubic-bezier(0.25, 0.8, 0.5, 1);
  transition: 0.1s ease-out;
}
.router-transition-move {
  transition: transform 0.4s;
}
.router-transition-enter,
.router-transition-leave-to {
  opacity: 0;
  // transform: translateY(5px);
}
</style>
