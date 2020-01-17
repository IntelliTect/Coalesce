<template>
  <transition-group 
    class="loader2 loader2-transition-group" 
    name="loader2-fade" mode="out-in" tag="div" 
    :class="{
      'has-progress-placeholder': usePlaceholder
    }">
    <div
      v-if="errorMessages.length" 
      key="error"
    >
      <v-alert :value="true" type="error" >
        <ul>
          <li v-for="message in errorMessages">
            {{message}}
          </li>
        </ul>
      </v-alert>
    </div>
    
    <!-- This nested transition allows us to transition between 
        the progress loader and the placeholder independent of the 
        main outer transition between content/error/loaders -->
    <transition-group 
      class="loader2-transition-group"
      key="loading"
      v-if="showLoading || usePlaceholder"
      name="loader2-fade" mode="out-in" tag="div"
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
      class="loader2-content"
    >
      <slot></slot>
    </div>
  </transition-group>
</template>

<script lang="ts">
import Vue from 'vue'
import { Component, Prop } from 'vue-property-decorator'
import { ApiState, ItemApiState, ListApiState } from 'coalesce-vue';

type AnyLoader = ItemApiState<any, any, any> | ListApiState<any, any, any>
class Flags {
  progress: boolean | null = null;
  initialProgress = true;
  secondaryProgress = true;
  loadingContent = true;
  errorContent = true;
  initialContent = true;
}

// Playing around with a more flexible version of loader-status to see how it feels.

@Component({name: 'loader-status-2'})
export default class extends Vue {

  /**
   * Loaders (docs forthcoming)
   */
  @Prop({required: true, type: Object })
  loaders!: { [flags: string]: AnyLoader[] };

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
          m => '-' + m.toLowerCase());

        if (flagsArr.includes(kebabFlag)) {
          flags[flagName] = true;
        } else if (flagsArr.includes('no-' + kebabFlag)) {
          flags[flagName] = false;
        }
      }

      const loaders = this.loaders[flagsStr];
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
  .loader2-transition-group {
    > * {
      flex-shrink: 0;
      transition: all .2s, opacity .2s ease-in .2s;
    }
  }

  .loader2-fade-enter, .loader2-fade-leave-to {
    opacity: 0;
  }
  .loader2-fade-leave-active {
    position: absolute;
    transition-delay: 0s !important;
  }

  .loader2 {
    position: relative;
    display: flex;
    flex-direction: column;
    
    .loader2-content {
      flex-grow: 1;
      flex-shrink: 1;

      // overflow:hidden breaks nested elements that use position: sticky.
      // My removal of this maybe? broke something somewhere that needs this.

      // overflow: hidden;
    }
  }
</style>
