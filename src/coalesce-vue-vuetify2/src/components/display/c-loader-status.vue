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
    <v-alert
      v-if="errorMessages.length"
      key="error"
      :value="true"
      type="error"
      class="c-loader-status--errors"
    >
      <ul>
        <li
          v-for="(message, i) in errorMessages"
          :key="'message-' + i"
          class="c-loader-status--error-message"
          v-text="message"
        ></li>
      </ul>
    </v-alert>

    <v-alert
      v-if="successMessages.length"
      key="success"
      :value="true"
      type="success"
      class="c-loader-status--success"
    >
      <ul>
        <li
          v-for="(message, i) in successMessages"
          :key="'success-message-' + i"
          class="c-loader-status--success-message"
          v-text="message"
        ></li>
      </ul>
    </v-alert>

    <!-- This nested transition allows us to transition between 
        the progress loader and the placeholder independent of the 
        main outer transition between content/error/loaders -->
    <transition-group
      class="c-loader-status--transition-group c-loader-status--progress-group"
      :class="{ absolute: progressAbsolute }"
      key="loading"
      v-if="showLoading || usePlaceholder"
      name="c-loader-status-fade"
      mode="out-in"
      tag="div"
    >
      <v-progress-linear
        key="progress"
        v-if="showLoading"
        indeterminate
        :height="height"
      >
      </v-progress-linear>
      <div key="placeholder" v-else :style="{ height: height + 'px' }"></div>
    </transition-group>

    <div v-if="showContent" key="normal" class="c-loader-status--content">
      <slot></slot>
    </div>
  </transition-group>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { ApiState, ItemApiState, ListApiState } from "coalesce-vue";

type AnyLoader = ItemApiState<any, any> | ListApiState<any, any>;
type AnyLoaderMaybe = AnyLoader | null | undefined;

type YesFlags = keyof Flags;
type NoFlags = `no-${YesFlags}`;

type FlagsString =
  | NoFlags
  | YesFlags
  | ""
  | `${NoFlags} ${string}`
  | `${YesFlags} ${string}`;

type Camelize<S extends string> = S extends `${infer F}-${infer R}`
  ? `${F}${Capitalize<Camelize<R>>}`
  : S;

class Flags {
  progress = true;
  "initial-progress" = true;
  "secondary-progress" = true;
  "loading-content" = true;
  "error-content" = true;
  "initial-content" = true;
}

const camelizeRE = /-(\w)/g;
const camelize = (str: string): string => {
  return str.replace(camelizeRE, (_, c) => (c ? c.toUpperCase() : ""));
};

/*
  TODO: This component could use a bit of a rewrite (again).
  Leave the existing component as it is for backwards compat - 
  instead, make a new one with the following changes:
    - New name: c-caller-status 
    - Rename prop loaders => callers
    - Allow defining flags as props on the components, preferably directly as flags like <c-caller-status :callers="[list.$load]" no-initial-progress />.
      - This is a new syntax in addition to the current flags dictionary syntax, which needs to be preserved to support advanced use cases.
*/

type LoadersProp =
  | {
      [flags in FlagsString]?: AnyLoaderMaybe | AnyLoaderMaybe[];
    }
  | AnyLoaderMaybe
  | AnyLoaderMaybe[];

export default defineComponent({
  name: "c-loader-status",

  props: {
    loaders: {
      required: true,
      type: Object as PropType<LoadersProp>,
    },

    /**
     * If the loader is loading when it already has a result,
     * keep the default slot visible.
     */
    progressPlaceholder: { required: false, type: Boolean, default: true },

    /** Positions the progress bar absolutely. This can be useful in compact interfaces
     * where extra space for the progress bar is undesirable, allowing the
     * progress bar to potentially overlap content while active. */
    progressAbsolute: { required: false, type: Boolean, default: false },

    height: { required: false, type: [Number, String], default: 10 },

    /** Show a success alert with message for successful calls. Defaults to false. */
    showSuccess: { required: false, type: Boolean, default: false },

    noProgress: { required: false, type: Boolean, default: null },
    noInitialProgress: { required: false, type: Boolean, default: null },
    noSecondaryProgress: { required: false, type: Boolean, default: null },
    noLoadingContent: { required: false, type: Boolean, default: null },
    noErrorContent: { required: false, type: Boolean, default: null },
    noInitialContent: { required: false, type: Boolean, default: null },
  },

  computed: {
    baselineFlags() {
      const flags = new Flags();
      for (const key in flags) {
        const noFlag = camelize(("no-" + key) as NoFlags) as Camelize<NoFlags>;

        const flagValue = this[noFlag];
        if (flagValue != null) flags[key as YesFlags] = !flagValue;
      }

      return Object.freeze(flags);
    },

    loaderFlags() {
      var ret = [];

      let loaders: LoadersProp = this.loaders;
      if (Array.isArray(loaders)) {
        loaders = { "": loaders };
      } else if (loaders == null) {
        // An attempt to pass a single loader that was nullish.
        loaders = { "": [] };
      } else if (loaders instanceof ApiState) {
        loaders = { "": [loaders] };
      }

      for (const flagsStr in loaders) {
        let loadersForFlags = loaders[flagsStr as keyof typeof loaders];
        if (!Array.isArray(loadersForFlags)) {
          loadersForFlags = [loadersForFlags];
        }

        let flags: Flags = this.baselineFlags;

        // Parse flags out of the object key for usages of the form:
        // <CLS :loaders="{"no-loading-content secondary-progress": [vm.$load]}" />
        if (flagsStr != "") {
          const flagsArr = flagsStr.split(" ");
          flags = { ...flags };

          if (flagsArr.length) {
            for (const flagName in flags) {
              if (flagsArr.includes(flagName)) {
                flags[flagName as YesFlags] = true;
              } else if (flagsArr.includes("no-" + flagName)) {
                flags[flagName as YesFlags] = false;
              }
            }
          }
        }

        for (const loader of loadersForFlags) {
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

    successMessages() {
      if (!this.showSuccess) return [];
      return this.loaderFlags
        .filter((f) => f[0].wasSuccessful === true && f[0].message)
        .map((f) => f[0].message);
    },

    showLoading() {
      return this.loaderFlags.some((f) => {
        const [loader, flags] = f;
        if (flags.progress === false) return false;

        const isLoading = loader.isLoading;
        return (
          (flags["initial-progress"] &&
            isLoading &&
            loader.wasSuccessful == null) ||
          (flags["secondary-progress"] &&
            isLoading &&
            loader.wasSuccessful != null)
        );
      });
    },

    usePlaceholder() {
      return (
        this.progressPlaceholder &&
        this.loaderFlags.some(
          (f) =>
            f[1].progress !== false &&
            f[1]["secondary-progress"] &&
            f[1]["loading-content"]
        )
      );
    },

    showContent() {
      return this.loaderFlags.every(([loader, flags]) => {
        if (!flags["loading-content"] && loader.isLoading) {
          // loader is loading, and loading content is off.
          return false;
        }
        if (!flags["error-content"] && loader.wasSuccessful === false) {
          // loader has an error, and error content is off
          return false;
        }
        if (
          // initial content is off, and either:
          !flags["initial-content"] &&
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
    flex-shrink: 0;
    transition: all 0.2s, opacity 0.2s ease-in 0.2s;
  }
}

.c-loader-status-fade-enter,
.c-loader-status-fade-leave-to {
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

  .c-loader-status--progress-group {
    &.absolute {
      // Make the progress bar on c-loader-status overlap
      // so it doesn't add extra whitespace to the top of the row.
      position: absolute !important;
      width: 100%;
    }
  }

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

    // Remove bulleting when there's only one error.
    &:only-child {
      list-style: none;
      margin-left: -20px;
    }
  }

  .c-loader-status--success-message {
    white-space: pre-wrap;

    // Remove bulleting when there's only one success message.
    &:only-child {
      list-style: none;
      margin-left: -20px;
    }
  }
}
</style>
