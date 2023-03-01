<template>
  <transition-group
    class="c-loader-status c-loader-status--transition-group"
    name="c-loader-status-fade"
    mode="out-in"
    tag="div"
    :class="{
      'has-progress-placeholder': usePlaceholder,
    }"
  >
    <!-- Outer div is needed because a transition can't be a child of another transition  -->
    <div key="error">
      <v-expand-transition>
        <!-- This div is to reduce jank caused by padding/margins on v-alert -->
        <div v-if="errorMessages.length">
          <v-alert
            :modelValue="true"
            type="error"
            class="c-loader-status--errors"
          >
            <ul>
              <li
                v-for="message in errorMessages"
                class="c-loader-status--error-message"
                v-text="message"
              ></li>
            </ul>
          </v-alert>
        </div>
      </v-expand-transition>
    </div>

    <!-- This nested transition allows us to transition between 
        the progress loader and the placeholder independent of the 
        main outer transition between content/error/loaders -->
    <transition-group
      v-if="showLoading || usePlaceholder"
      key="loading"
      class="c-loader-status--transition-group c-loader-status--progress-group"
      name="c-loader-status-fade"
      mode="out-in"
      tag="div"
      style="position: relative"
      :style="{
        '--c-loader-status-progress-height': height + 'px',
      }"
    >
      <v-progress-linear
        v-if="showLoading"
        key="progress"
        class="c-loader-status--progress"
        indeterminate
        :height="height"
        :color="color"
      >
      </v-progress-linear>
      <div
        key="placeholder"
        v-else
        class="c-loader-status--progress-placeholder"
        :style="{ height: height + 'px' }"
      ></div>
    </transition-group>

    <div v-if="showContent" key="normal" class="c-loader-status--content">
      <slot></slot>
    </div>
  </transition-group>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { ItemApiState, ListApiState } from "coalesce-vue";

type AnyLoader = ItemApiState<any, any> | ListApiState<any, any>;
type AnyLoaderMaybe = AnyLoader | null | undefined;

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

export default defineComponent({
  name: "c-loader-status",

  props: {
    loaders: {
      required: true,
      type: Object as PropType<{
        [flags: string]: AnyLoaderMaybe | AnyLoaderMaybe[];
      }>,
    },

    /**
     * If the loader is loading when it already has a result,
     * keep the default slot visible.
     */
    progressPlaceholder: { required: false, type: Boolean, default: true },
    height: { required: false, type: [Number, String], default: 10 },
    color: { required: false, type: String, default: "primary" },
  },

  computed: {
    loaderFlags() {
      var ret = [];
      for (const flagsStr in this.loaders) {
        const flagsArr = flagsStr.split(" ");
        const flags: any = new Flags();
        for (const flagName in flags) {
          const kebabFlag = flagName.replace(
            /([A-Z])/g,
            (m) => "-" + m.toLowerCase()
          );

          if (flagsArr.includes(kebabFlag)) {
            flags[flagName] = true;
          } else if (flagsArr.includes("no-" + kebabFlag)) {
            flags[flagName] = false;
          }
        }

        let loaders = this.loaders[flagsStr];
        if (!Array.isArray(loaders)) {
          loaders = [loaders];
        }
        for (const loader of loaders) {
          if (loader) {
            ret.push([loader, flags as Flags] as const);
          }
        }
      }

      return ret;
    },

    anyFailed() {
      return this.loaderFlags.some((f) => f[0].wasSuccessful === false);
    },

    errorMessages() {
      return this.loaderFlags
        .filter((f) => f[0].wasSuccessful === false)
        .map((f) => f[0].message);
    },

    showLoading() {
      return this.loaderFlags.some((f) => {
        const [loader, flags] = f;
        if (flags.progress === false) return false;

        const isLoading = loader.isLoading;
        return (
          (flags.initialProgress &&
            isLoading &&
            loader.wasSuccessful == null) ||
          (flags.secondaryProgress && isLoading && loader.wasSuccessful != null)
        );
      });
    },

    usePlaceholder() {
      return (
        this.progressPlaceholder &&
        this.loaderFlags.some(
          (f) =>
            f[1].progress !== false &&
            f[1].secondaryProgress &&
            f[1].loadingContent
        )
      );
    },

    showContent() {
      return this.loaderFlags.every(([loader, flags]) => {
        if (!flags.loadingContent && loader.isLoading) {
          // loader is loading, and loading content is off.
          return false;
        }
        if (!flags.errorContent && loader.wasSuccessful === false) {
          // loader has an error, and error content is off
          return false;
        }
        if (
          // initial content is off, and either:
          !flags.initialContent &&
          // loader has not yet loaded
          (loader.wasSuccessful == null ||
            // or loader has loaded, but it errored and there's no current result
            // (implying it has never successfully loaded).
            (loader.wasSuccessful === false && !loader.hasResult))
        ) {
          return false;
        }
        return true;
      });
    },
  },
});
</script>

<style lang="scss">
.c-loader-status--transition-group {
  > * {
    transition-property: max-height, opacity;
    transition-duration: 0.1s, 0.2s;
    transition-timing-function: ease-in;
  }
  &:not(.has-progress-placeholder) {
    // If there's no progress placeholder, hide the progress bar immediately
    // so that it doesn't cause the content to jump as it finishes fading away.
    // Also animate the height to smooth out its entrance.
    // For example, test c-admin-method under various latency settings.
    .c-loader-status--progress-group {
      &.c-loader-status-fade-leave-from,
      &.c-loader-status-fade-leave-active {
        transition-duration: 0s;
        max-height: 0px;
      }

      max-height: var(--c-loader-status-progress-height);
      &.c-loader-status-fade-enter-from {
        max-height: 0px;
      }
      &.c-loader-status-fade-enter-active {
        overflow: hidden;
      }
    }
  }
}

.c-loader-status-fade-enter-from,
.c-loader-status-fade-leave-to {
  opacity: 0;
}
.c-loader-status-fade-leave-active {
  &.c-loader-status--progress-placeholder,
  &.c-loader-status--progress {
    position: absolute !important;
  }
}
.c-loader-status-fade-enter-from,
.c-loader-status-fade-enter-active {
  // Delay the fade-in of the progress bars.
  // 1) This prevents progress bars from flashing in during very fast connections.
  // 2) This prevents progress bars from appearing before content disappears (if no-loading-content)
  &.c-loader-status--progress-group,
  &.c-loader-status--progress-placeholder,
  &.c-loader-status--progress {
    transition-delay: 0.2s !important;
  }
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

  ul {
    padding-left: 24px;
  }
  .c-loader-status--error-message {
    white-space: pre-wrap;

    // Remove bulleting when there's only one error.
    &:only-child {
      list-style: none;
      margin-left: -20px;
    }
  }
}
</style>
