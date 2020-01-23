<template>

  <v-app id="vue-app">
    <v-app-bar
      app
      color="primary"
      dark dense
      clipped-left
    >

      <v-toolbar-title style="line-height: 1">
        <router-link to="/" class="white--text" >
          Coalesce Vue Demo
        </router-link>
      </v-toolbar-title>

      <div class="nav-items">

        <v-btn text to="/">Home</v-btn>

        <v-menu offset-y>
          <template #activator="{on}">
            <v-btn text v-on="on">
              Dropdown Menu
              <i class="fa fa-caret-down pl-1"></i>
            </v-btn>
          </template>
          <v-list>
            <v-list-item to="/"><v-list-item-title>Home</v-list-item-title></v-list-item>
          </v-list>
        </v-menu>
      </div>
    </v-app-bar>

    <v-content>
      <c-input-props-provider outlined dense>
        <transition name="router-transition" mode="out-in" @enter="routerViewOnEnter" appear>
          <!-- https://stackoverflow.com/questions/52847979/what-is-router-view-key-route-fullpath -->
          <router-view ref="routerView" :key="$route.path" />
        </transition>
      </c-input-props-provider>
    </v-content>
  </v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component({
  components: { }
})
export default class App extends Vue {
  routeComponent: Vue | null = null;

  get routeMeta() {
    if (!this.$route || this.$route.name === null) return null;

    return this.$route.meta;
  }
  routerViewOnEnter() {
    this.routeComponent = this.$refs.routerView as Vue
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
  #vue-app {
  }

  .v-app-bar {
    padding-top: 0;
    padding-bottom: 0;

    .nav-items {
      flex-grow: 1;
      display: flex;
      overflow-x: auto;
    }
  }


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