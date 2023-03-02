import { createCoalesceVuetify } from "@/install";
import {
  mount,
  DOMWrapper,
  createWrapperError,
  enableAutoUnmount,
} from "@vue/test-utils";
import { ArgumentsType } from "vitest";
import { defineComponent, h } from "vue";

import { createVuetify } from "vuetify";
import * as components from "vuetify/components";
import * as directives from "vuetify/directives";
import $metadata from "./targets.metadata";

global.ResizeObserver ??= class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
};
global.cancelIdleCallback = function () {};

enableAutoUnmount(afterEach);

const vuetify = createVuetify({ components, directives });
const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
});

const mountVuetify = function (
  component: ArgumentsType<typeof mount>[0],
  options: ArgumentsType<typeof mount>[1]
) {
  const wrapper = mount(component, {
    ...options,
    global: {
      plugins: [vuetify, coalesceVuetify],
    },
  });

  return wrapper;
} as typeof mount;

const mountApp = function (
  component: ArgumentsType<typeof mount>[0],
  options: ArgumentsType<typeof mount>[1]
) {
  const appWrapper = mount(
    defineComponent({
      render() {
        return h(components.VApp, () => [
          h(component as any, {
            ...options?.props,
            ...options?.attrs,
          }),
        ]);
      },
    }),
    {
      attachTo: document.body,
      global: {
        plugins: [vuetify, coalesceVuetify],
      },
    }
  );

  return appWrapper;
} as typeof mount;

export function getWrapper(selector = ".v-overlay-container") {
  const el = document.querySelector(selector);
  if (el) return new DOMWrapper(el);
  else return createWrapperError<DOMWrapper<Element>>("DOMWrapper");
}

export async function delay(ms: number) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

export { nextTick } from "vue";
export { flushPromises } from "@vue/test-utils";

export { mountVuetify as mount, mountApp };
