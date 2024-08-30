<template>
  <v-app id="vue-app">
    <v-app-bar color="primary" density="compact">
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <v-toolbar-title>
        <router-link to="/" style="color: inherit">
          Coalesce Vue Template
        </router-link>
      </v-toolbar-title>

      <v-menu bottom offset-y>
        <template #activator="{ props }">
          <v-list-item v-bind="props" style="overflow: visible">
            <div class="d-flex align-center">
              <user-avatar :user="$userInfo" style="font-size: 1.5em" />
              {{ $userInfo.userName }}
              <span class="caret ml-1"></span>
            </div>
          </v-list-item>
        </template>
        <v-list min-width="300px">
          <v-list-item :title="$userInfo.userName!">
            <template #prepend>
              <user-avatar
                :user="$userInfo"
                style="font-size: 2.1em"
                class="mr-6 ml-n1"
              />
            </template>
          </v-list-item>
          <v-divider class="mt-1" />
          <v-list-item
            to="/my-tasks"
            prepend-icon="fa fa-list-check"
            title="My Tasks"
          />
          <v-list-item
            to="/preferences"
            prepend-icon="fa fa-cog"
            title="Preferences"
          />
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

          <v-divider />
          <v-list-item
            href="/Home/SignOut"
            prepend-icon="fa fa-sign-out"
            title="Log Out"
          />
        </v-list>
      </v-menu>
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
          <Forbidden
            v-if="isForbidden"
            :permissions="routeMeta?.permissions"
            key="$forbidden"
          />
          <component :is="Component" :key="route.path" />
        </transition>
      </router-view>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { useLocalStorage, usePreferredDark } from "@vueuse/core";
import { Permission } from "./models.g";
import Forbidden from "./views/errors/Forbidden.vue";

const drawer = ref<boolean | null>(null);

const router = useRouter();
const { userInfo } = useUser();

const theme = useLocalStorage(
  "theme",
  usePreferredDark().value ? "dark" : "light",
);

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
