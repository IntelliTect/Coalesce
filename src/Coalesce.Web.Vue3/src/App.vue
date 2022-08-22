<template>
  <v-app>
    <v-app-bar
    >

      <v-toolbar-title style="line-height: 1">
        <router-link to="/" class="white--text" >
          Coalesce Vue Demo
        </router-link>
      </v-toolbar-title>

      <!-- <div class="nav-items"> -->

        <v-btn variant="text" to="/">Home</v-btn>
        <v-btn variant="text" to="/test">Test</v-btn>
        <v-btn variant="text" href="/coalesce-security">Security Overview</v-btn>

        <v-menu offset-y>
          <template #activator="{props}">
            <v-btn variant="text" v-bind="props">
              Dropdown Menu
              <i class="fa fa-caret-down pl-1"></i>
            </v-btn>
          </template>
          <v-list>
            <v-list-item to="/"><v-list-item-title>Home</v-list-item-title></v-list-item>
          </v-list>
        </v-menu>
      <!-- </div> -->
    </v-app-bar>

    <v-main>
      <router-view v-slot="{ Component }">
        <transition name="router-transition" mode="out-in" @enter="routerViewOnEnter" appear>
          <!-- https://stackoverflow.com/questions/52847979/what-is-router-view-key-route-fullpath -->
          <component ref="routerView" :is="Component" :key="$route.path" />
        </transition>
      </router-view>
    </v-main>
  </v-app>
</template>

<script lang="ts">
import { Base, Component } from 'vue-facing-decorator';

@Component({
  components: { }
})
export default class App extends Base {
  routeComponent: any = null;

  get routeMeta() {
    if (!this.$route || this.$route.name === null) return null;

    return this.$route.meta;
  }
  routerViewOnEnter() {
    this.routeComponent = this.$refs.routerView
  }

  created() {
    const baseTitle = document.title
    this.$watch(
      () => (this.routeComponent as any)?.pageTitle,
      (n: string | null | undefined) => {
        if (n) {
          document.title = n + " - " + baseTitle
        } else {
          document.title = baseTitle
        }
      },
      { immediate: true }
    )
  }
}

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