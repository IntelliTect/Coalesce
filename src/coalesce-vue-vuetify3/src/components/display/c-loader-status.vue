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
    <div key="messages">
      <v-expand-transition>
        <!-- This div is to reduce jank caused by padding/margins on v-alert -->
        <div v-if="errorMessages.length || successMessages.length">
          <v-alert
            :modelValue="true"
            :type="errorMessages.length ? 'error' : 'success'"
            :title="title"
            :density="density"
            :class="[
              'c-loader-status--messages',
              errorMessages.length
                ? 'c-loader-status--errors'
                : 'c-loader-status--success',
            ]"
          >
            <template v-if="$slots.prepend" #prepend>
              <slot name="prepend"></slot>
            </template>
            <ul>
              <li
                v-for="(message, i) in errorMessages"
                :key="'error-message-' + i"
                class="c-loader-status--message c-loader-status--error-message"
                v-text="message"
              ></li>
              <li
                v-for="(message, i) in successMessages"
                :key="'success-message-' + i"
                class="c-loader-status--message c-loader-status--success-message"
                v-text="message"
              ></li>
            </ul>
            <template v-if="$slots.append || retryLoaders.length" #append>
              <slot name="append"></slot>
              <v-btn
                v-if="retryLoaders.length"
                variant="outlined"
                class="ml-2"
                prepend-icon="$loading"
                @click="handleRetry"
              >
                Retry
              </v-btn>
            </template>
          </v-alert>
        </div>
      </v-expand-transition>
    </div>

    <!-- This nested transition allows us to transition between 
        the progress loader and the placeholder independent of the 
        main outer transition between content/error/loaders -->
    <transition-group
      key="loading"
      class="c-loader-status--transition-group c-loader-status--progress-group"
      :class="{ absolute: progressAbsolute }"
      name="c-loader-status-fade"
      mode="out-in"
      tag="div"
      :style="{
        '--c-loader-status-progress-height':
          (usePlaceholder || showLoading ? height : 0) + 'px',
      }"
    >
      <v-progress-linear
        v-if="showLoading"
        key="progress"
        class="c-loader-status--progress"
        indeterminate
        :height="height"
        :color="color"
        :aria-label="ariaLabel"
      >
      </v-progress-linear>
    </transition-group>

    <div v-if="showContent" key="normal" class="c-loader-status--content">
      <slot></slot>
    </div>
  </transition-group>
</template>

<script lang="ts">
class Flags {
  progress = true;
  "initial-progress" = true;
  "secondary-progress" = true;
  "loading-content" = true;
  "error-content" = true;
  "initial-content" = true;
  "show-success" = false;
  "show-retry" = false;
}
</script>

<script lang="ts" setup>
import { computed, camelize } from "vue";
import { ApiState, ItemApiState, ListApiState } from "coalesce-vue";

type AnyLoader = ItemApiState<any, any> | ListApiState<any, any>;

type AnyLoaderMaybe =
  // Intersection with `Function` makes vue emit correct runtime props.
  // eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
  (AnyLoader & Function) | null | undefined | false;

// Since the component props are the most general level of flags
// and all flags default to true, include only the props that will
// affect the default, which are the no versions.
type CamelFlags =
  | "noProgress"
  | "noInitialProgress"
  | "noSecondaryProgress"
  | "noLoadingContent"
  | "noErrorContent"
  | "noInitialContent";

type CamelYesFlags = "showSuccess" | "showRetry";

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

defineOptions({
  name: "c-loader-status",
});

defineSlots<{
  default(): any;
  prepend(): any;
  append(): any;
}>();

const props = withDefaults(
  defineProps<
    {
      loaders:
        | {
            [flags in FlagsString]?: AnyLoaderMaybe | AnyLoaderMaybe[];
          }
        | AnyLoaderMaybe
        | AnyLoaderMaybe[];

      /**
       * Whether space is reserved for the progress bar even when
       * the progress visible is not active. Default true.
       */
      progressPlaceholder?: boolean;

      /** Positions the progress bar absolutely. This can be useful in compact interfaces
       * where extra space for the progress bar is undesirable, allowing the
       * progress bar to potentially overlap content while active. */
      progressAbsolute?: boolean;

      /** The height of the progress bar in pixels */
      height?: number;
      /** The color of the progress bar */
      color?: string;
      /** Title to display in the alert */
      title?: string;
      /** Density of the alert (compact, comfortable, default) */
      density?: "compact" | "comfortable" | "default";
      /** Accessible label for the progress bar */
      ariaLabel?: string;
    } & { [K in CamelFlags]?: boolean } & { [K in CamelYesFlags]?: boolean }
  >(),
  {
    progressPlaceholder: true,
    height: 10,
    color: "primary",
    // Prevent our flags from defaulting to `false`. We need them to default to `undefined`
    // so we can detect if they were specified at all.
    noProgress: undefined,
    noInitialProgress: undefined,
    noSecondaryProgress: undefined,
    noLoadingContent: undefined,
    noErrorContent: undefined,
    noInitialContent: undefined,
    showSuccess: undefined,
    showRetry: undefined,
  },
);

const baselineFlags = computed(() => {
  const flags = new Flags();
  for (const key in flags) {
    const noFlag = camelize(("no-" + key) as NoFlags) as Camelize<NoFlags>;
    const yesFlag = camelize(key as YesFlags) as Camelize<YesFlags>;

    const noFlagValue = (props as any)[noFlag];
    const yesFlagValue = (props as any)[yesFlag];

    if (noFlagValue != null) {
      flags[key as YesFlags] = !noFlagValue;
    } else if (yesFlagValue != null) {
      flags[key as YesFlags] = yesFlagValue;
    }
  }

  return Object.freeze(flags);
});

const loaderFlags = computed(() => {
  const ret = [];

  let loaders = props.loaders;
  if (Array.isArray(loaders)) {
    loaders = { "": loaders };
  } else if (!loaders) {
    // An attempt to pass a single loader that was falsy.
    loaders = { "": [] };
  } else if (loaders instanceof ApiState) {
    loaders = { "": [loaders] };
  }

  for (const flagsStr in loaders) {
    let loadersForFlags = loaders[flagsStr as keyof typeof loaders];
    if (!Array.isArray(loadersForFlags)) {
      loadersForFlags = [loadersForFlags];
    }

    let flags: Flags = baselineFlags.value;

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
});

const errorMessages = computed(() => {
  return loaderFlags.value
    .filter((f) => f[0].wasSuccessful === false)
    .map((f) => f[0].message);
});

const successMessages = computed(() => {
  return loaderFlags.value
    .filter((f) => f[0].wasSuccessful === true && f[1]["show-success"])
    .map((f) => f[0].message || "Success");
});

const retryLoaders = computed(() => {
  return loaderFlags.value
    .filter((f) => f[0].wasSuccessful === false && f[1]["show-retry"])
    .map((f) => f[0]);
});

const handleRetry = () => {
  retryLoaders.value.forEach((loader) => {
    if (typeof (loader as any).invokeWithArgs === "function") {
      (loader as any).invokeWithArgs();
    } else {
      loader.invoke();
    }
  });
};

const showLoading = computed(() => {
  return loaderFlags.value.some((f) => {
    const [loader, flags] = f;
    if (!flags.progress) return false;

    const isLoading = loader.isLoading;
    return (
      (flags["initial-progress"] &&
        isLoading &&
        loader.wasSuccessful == null) ||
      (flags["secondary-progress"] && isLoading && loader.wasSuccessful != null)
    );
  });
});

const usePlaceholder = computed(() => {
  return (
    props.progressPlaceholder &&
    loaderFlags.value.some(
      (f) =>
        // Obviously don't need a progress placeholder if progress is off:
        f[1].progress &&
        // If secondary progress is off, the progress bar will never be shown
        // once any content has been obtained, so there's no need to keep a placeholder.
        f[1]["secondary-progress"] &&
        // If loading content was off then the progressbar will just take the place of the content,
        // so a placeholder is only needed if loading-content is on.
        f[1]["loading-content"],
    )
  );
});

const showContent = computed(() => {
  return loaderFlags.value.every(([loader, flags]) => {
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
});

// For testing:
defineExpose({ loaderFlags });
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
      max-height: var(--c-loader-status-progress-height);

      &.c-loader-status-fade-leave-from,
      &.c-loader-status-fade-leave-active {
        transition-duration: 0s;
        max-height: 0px;
      }

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
  &.c-loader-status--progress {
    transition-delay: 0.2s !important;
  }
}

.c-loader-status {
  position: relative;
  display: flex;
  flex-direction: column;
  font-weight: 400;

  .c-loader-status--progress-group {
    transition-property: max-height, min-height;
    transition-duration: 0.1s;
    transition-timing-function: ease-in;
    min-height: var(--c-loader-status-progress-height);
    max-height: var(--c-loader-status-progress-height);
    position: relative;

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

  .c-loader-status--messages {
    ul {
      padding-left: 20px;
    }
  }

  // Common styles for all messages
  .c-loader-status--message {
    white-space: pre-wrap;

    // Remove bulleting when there's only one message.
    &:only-child {
      list-style: none;
      margin-left: -20px;
    }
  }
}
</style>
