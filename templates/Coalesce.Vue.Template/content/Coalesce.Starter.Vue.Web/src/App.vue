<template>
  <v-app id="vue-app">
    <v-app-bar color="primary" density="compact">
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <v-toolbar-title>
        <router-link to="/" style="color: inherit">
          Coalesce.Starter.Vue
          <!--#if Tenancy -->
          &mdash; {{ $userInfo.tenantName }}
          <!--#endif  -->
        </router-link>
      </v-toolbar-title>

      <!--#if (Identity || DarkMode) -->
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
            title="My Profile"
            :subtitle="$userInfo.userName!"
            :to="`/user/${$userInfo.id}`"
          >
            <template #prepend>
              <UserAvatar :user="$userInfo" class="mr-2 ml-n1" />
            </template>
          </v-list-item>
          <v-divider class="mt-1" />
          <!--#endif -->

          <!--#if DarkMode -->
          <v-list-item prepend-icon="fa fa-moon">
            <v-switch
              v-model="theme"
              label="Dark Mode"
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
          <!--#if Tenancy -->
          <v-list-item
            href="/SelectTenant"
            prepend-icon="fa fa-building"
            title="Switch Organization"
          />
          <!--#endif -->
          <v-list-item
            href="/SignOut"
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
        <v-list-item to="/" prepend-icon="fa fa-home" title="Home" />
        <!--#if ExampleModel -->
        <v-list-item
          to="/widget"
          prepend-icon="fa fa-palette"
          title="Custom Page Example"
        />
        <!--#endif -->
        <v-divider></v-divider>
        <!--#if Identity -->
        <v-list-item
          v-if="$can(Permission.UserAdmin)"
          to="/admin/User"
          prepend-icon="fa fa-users"
          title="Users"
        />
        <v-list-item
          v-if="$can(Permission.UserAdmin)"
          to="/admin/Role"
          prepend-icon="fa fa-id-card"
          title="Roles"
        />
        <v-list-item
          v-if="$can(Permission.Admin)"
          to="/admin"
          prepend-icon="fa fa-cogs"
          title="Admin"
        />
        <!--#else
        <v-list-item to="/admin" prepend-icon="fa fa-cogs" title="Admin" />
        #endif -->

        <!--#if Tenancy -->
        <v-divider></v-divider>
        <v-list-item
          v-if="$userInfo.roles?.includes('GlobalAdmin')"
          to="/admin/Tenant?dataSource=GlobalAdminSource"
          prepend-icon="fa fa-building"
          title="All Tenants"
        />
        <!--#endif -->
      </v-list>

      <div
        class="position-absolute left-0 bottom-0 px-1 text-caption text-grey text-center"
      >
        {{ buildDate }}
      </div>
    </v-navigation-drawer>

    <v-main>
      <!-- https://stackoverflow.com/questions/52847979/what-is-router-view-key-route-fullpath -->
      <router-view v-slot="{ Component, route }">
        <transition name="router-transition" mode="out-in" appear>
          <!--#if Identity -->
          <Forbidden
            v-if="isForbidden"
            key="$forbidden"
            :permissions="routeMeta?.permissions"
          />
          <component :is="Component" v-else :key="route.path" />
          <!--#endif -->
          <!--#if (!Identity)
          <component :is="Component" :key="route.path" />
          #endif -->
        </transition>
      </router-view>
      <!--#if AIChat -->
      <AIChat title="AI Assistant" endpoint="chatAgent" class="right-0" />
      <!--#endif -->
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
import { format } from "date-fns-tz";

const drawer = ref<boolean | null>(null);

//#if Identity
const router = useRouter();
const { userInfo } = useUser();
//#endif
//#if DarkMode
const vuetifyTheme = useTheme();

const theme = useLocalStorage(
  "theme",
  usePreferredDark().value ? "dark" : "light",
);
watch(theme, (v) => vuetifyTheme.change(v), { immediate: true });
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

const buildDate = computed(() => {
  if (!BUILD_DATE) return "";
  return "build " + format(BUILD_DATE, "yyyy-MM-dd hh:mm a z");
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
