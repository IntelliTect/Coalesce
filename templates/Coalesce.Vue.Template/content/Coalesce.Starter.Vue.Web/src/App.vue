<template>
  <v-app id="vue-app">
    <v-app-bar color="primary" density="compact">
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <v-toolbar-title>
        <router-link to="/" style="color: inherit">
          Coalesce Vue Template
        </router-link>
      </v-toolbar-title>

      <!--#if Identity || DarkMode -->
      <v-menu bottom offset-y>
        <template #activator="{ props }">
          <v-list-item v-bind="props">
            <!--#if Identity -->
            <UserAvatar :user="$userInfo" />
            <!--#endif -->
            <!--#if (!Identity)
            Settings
            #endif -->
          </v-list-item>
        </template>
        <v-list min-width="300px">
          <!--#if Identity -->
          <v-list-item
            :title="$userInfo.fullName!"
            :subtitle="$userInfo.userName!"
          >
            <template #prepend>
              <UserAvatar :user="$userInfo" class="mr-2 ml-n1" />
            </template>
          </v-list-item>
          <v-divider class="mt-1" />
          <!--#endif -->

          <!--#if DarkMode -->
          <v-list-item prepend-icon="fas fa-moon">
            <v-switch
              label="Dark Mode"
              v-model="theme"
              true-value="dark"
              false-value="light"
              hide-details
              class="ml-2"
              density="compact"
            />
          </v-list-item>
          <!--#endif -->

          <!--#if Identity -->
          <v-divider />
          <v-list-item
            href="/Home/SignOut"
            prepend-icon="fa fa-sign-out"
            title="Log Out"
          />
          <!--#endif -->
        </v-list>
      </v-menu>
      <!--#endif -->
    </v-app-bar>
    <v-navigation-drawer v-model="drawer">
      <v-list>
        <v-list-item to="/" prepend-icon="fas fa-home" title="Home" />
        <v-list-item
          to="/widget"
          prepend-icon="fas fa-palette"
          title="Custom Page Example"
        />
        <v-list-item
          to="/admin"
          prepend-icon="fas fa-cogs"
          title="Admin Pages"
        />
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <!-- https://stackoverflow.com/questions/52847979/what-is-router-view-key-route-fullpath -->
      <router-view v-slot="{ Component, route }">
        <transition name="router-transition" mode="out-in" appear>
          <!--#if Identity -->
          <Forbidden
            v-if="isForbidden"
            :permissions="routeMeta?.permissions"
            key="$forbidden"
          />
          <component v-else :is="Component" :key="route.path" />
          <!--#endif -->
          <!--#if (!Identity)
          <component :is="Component" :key="route.path" />
          #endif -->
        </transition>
      </router-view>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
//#if Identity
import { Permission } from "./models.g";
import Forbidden from "./views/errors/Forbidden.vue";
//#endif
//#if DarkMode
import { useLocalStorage, usePreferredDark } from "@vueuse/core";
import { useTheme } from "vuetify";
//#endif

const drawer = ref<boolean | null>(null);

const router = useRouter();
const { userInfo } = useUser();
//#if DarkMode
const vuetifyTheme = useTheme();

const theme = useLocalStorage(
  "theme",
  usePreferredDark().value ? "dark" : "light",
);
watch(theme, (v) => (vuetifyTheme.global.name.value = v), { immediate: true });
//#endif

//#if Identity
const routeMeta = computed(() => {
  const route = router.currentRoute.value;
  return route?.meta as
    | {
        permissions?: Permission[];
      }
    | null
    | undefined;
});

const isForbidden = computed(() => {
  if (
    routeMeta.value?.permissions &&
    !userInfo.value?.permissions?.some(
      // @ts-expect-error indexing enum with arbitrary string
      (r) => routeMeta.value?.permissions?.includes(Permission[r as any]),
    )
  ) {
    return true;
  }

  return false;
});
//#endif
</script>

<style lang="scss">
.router-transition-enter-active,
.router-transition-leave-active {
  transition: 0.1s ease-out;
}

.router-transition-enter-from,
.router-transition-leave-to {
  opacity: 0;
}
</style>
