<template>
  <transition-group 
    class="c-loader-status c-loader-status--transition-group" 
    name="c-loader-status-fade" mode="out-in" tag="div" 
    :class="{
      'has-progress-placeholder': usePlaceholder
    }"
  >
    <div
      v-if="errorMessages.length" 
      key="error"
    >
      <v-alert :value="true" type="error" >
        <ul>
          <li 
            v-for="message in errorMessages" 
            class="c-loader-status--error-message" 
            v-text="message"></li>
        </ul>
      </v-alert>
    </div>
    
    <!-- This nested transition allows us to transition between 
        the progress loader and the placeholder independent of the 
        main outer transition between content/error/loaders -->
    <transition-group 
      class="c-loader-status--transition-group"
      key="loading"
      v-if="showLoading || usePlaceholder"
      name="c-loader-status-fade" mode="out-in" tag="div"
    >
      <v-progress-linear 
        key="progress"
        v-if="showLoading"
        indeterminate :height="height">
      </v-progress-linear>
      <div 
        key="placeholder"
        v-else
        :style="{height: height + 'px'}"
      ></div>
    </transition-group>

    <div 
      v-if="showContent"  
      key="normal"
      class="c-loader-status--content"
    >
      <slot></slot>
    </div>
  </transition-group>
</template>

<script lang="ts">
import Vue from 'vue'
import { Component, Prop } from 'vue-property-decorator'
import { ApiState, ItemApiState, ListApiState } from 'coalesce-vue';

type AnyLoader = ItemApiState<any, any> | ListApiState<any, any>
class Flags {
  progress: boolean | null = null;
  initialProgress = true;
  secondaryProgress = true;
  loadingContent = true;
  errorContent = true;
  initialContent = true;
}


/*
  TODO: This component could use a bit of a rewrite (again).
  Leave the existing component as it is for backwards compat - 
  instead, make a new one with the following changes:
    - New name: c-caller-status 
    - Rename prop loaders => callers
    - Allow defining flags as props on the components, preferably directly as flags like <c-caller-status :callers="[list.$load]" no-initial-progress />.
      - This is a new syntax in addition to the current flags dictionary syntax, which needs to be preserved to support advanced use cases.


*/

@Component({name: 'c-loader-status'})
export default class extends Vue {

  /**
   * Loaders (docs forthcoming)
   */
  @Prop({required: true, type: Object })
  loaders!: { [flags: string]: AnyLoader | AnyLoader[] };

  /**
   * If the loader is loading when it already has a result,
   * keep the default slot visible.
   */
  @Prop({required: false, type: Boolean, default: true})
  progressPlaceholder!: boolean;

  @Prop({required: false, type: [Number, String], default: 10})
  height!: number | string;

  get loaderFlags() {
    var ret = []
    for (const flagsStr in this.loaders) {
      const flagsArr = flagsStr.split(" ");
      const flags: any = new Flags;
      for (const flagName in flags) {
        const kebabFlag = flagName.replace(
          /([A-Z])/g, 
          m => '-' + m.toLowerCase()
        );

        if (flagsArr.includes(kebabFlag)) {
          flags[flagName] = true;
        } else if (flagsArr.includes('no-' + kebabFlag)) {
          flags[flagName] = false;
        }
      }

      let loaders = this.loaders[flagsStr];
      if (!Array.isArray(loaders)) {
        loaders = [ loaders ]
      }
      for (const loader of loaders) {
        ret.push([loader, flags as Flags] as const)
      }
    }

    return ret;
  }

  get anyFailed() {
    return this.loaderFlags.some(f => f[0].wasSuccessful === false)
  }

  get errorMessages() {
    return this.loaderFlags
      .filter(f => f[0].wasSuccessful === false)
      .map(f => f[0].message)
  }

  get showLoading() {
    return this.loaderFlags.some(f => {
      const [loader, flags] = f;
      if (flags.progress === false) return false;

      const isLoading = loader.isLoading;
      return (
          (flags.initialProgress && isLoading && loader.wasSuccessful == null)
        || (flags.secondaryProgress && isLoading && loader.wasSuccessful != null)
      )
    })
  }

  get usePlaceholder() {
    return this.progressPlaceholder && this.loaderFlags.some(f => 
      (f[1].progress !== false && f[1].secondaryProgress) && f[1].loadingContent
    )
  }

  get showContent() {
    
    return this.loaderFlags.every(([loader, flags]) => {
      if (loader.isLoading && !flags.loadingContent) {
        // loader is loading, and loading content is off.
        return false
      }
      if (loader.wasSuccessful === false && !flags.errorContent) {
        // loader has an error, and error content is off
        return false
      }
      if (loader.wasSuccessful == null && !flags.initialContent) {
        // loader has not yet loaded, and initial content is off
        return false
      }
      return true;
    })
  }
}
</script>

<style lang="scss">
  .c-loader-status--transition-group {
    > * {
      flex-shrink: 0;
      transition: all .2s, opacity .2s ease-in .2s;
    }
  }

  .c-loader-status-fade-enter, .c-loader-status-fade-leave-to {
    opacity: 0;
  }
  .c-loader-status-fade-leave-active {
    // Important because vuetify specifies position:relative on .v-progress-linear
    position: absolute !important; 
    transition-delay: 0s !important;
  }

  .c-loader-status {
    position: relative;
    display: flex;
    flex-direction: column;
    font-weight: 400;
    
    .c-loader-status--content {
      flex-grow: 1;
      flex-shrink: 1;

      // Prevents small block-level content from losing its parent width 
      // when the wrapper becomes positioned absolutely during the transition.
      width: 100%;

      // overflow:hidden breaks nested elements that use position: sticky.
      // My removal of this maybe? broke something somewhere that needs this.

      // overflow: hidden;
    }

    .c-loader-status--error-message {
      white-space: pre-wrap;
    }
  }
</style>
