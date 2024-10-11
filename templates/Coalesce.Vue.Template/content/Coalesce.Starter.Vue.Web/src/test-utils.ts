import { createCoalesceVuetify } from "coalesce-vue-vuetify3";
import { enableAutoUnmount, mount } from "@vue/test-utils";

import { createVuetify } from "vuetify";
import $metadata from "@/metadata.g";
import router from "@/router";

// Automatically teardown components after each test. Especially necessary
// with Vuetify, which will attach dialogs and such to document.body.
enableAutoUnmount(afterEach);

const vuetify = createVuetify({});
const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
});

type ArgumentsType<T> = T extends (...args: infer U) => any ? U : never;

const mountComponent = function (
  component: ArgumentsType<typeof mount>[0],
  options: ArgumentsType<typeof mount>[1],
) {
  return mount(component, {
    ...options,
    attachTo: document.body,
    global: {
      plugins: [vuetify, coalesceVuetify, router],
    },
  } as ArgumentsType<typeof mount>[1]);
} as typeof mount;

export { nextTick } from "vue";
export { flushPromises } from "@vue/test-utils";
export { mockEndpoint } from "coalesce-vue/lib/test-utils";
export { mountComponent as mount };
