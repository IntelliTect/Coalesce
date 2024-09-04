import { CAdminEditorPage, CAdminTablePage } from "@/index";
import { createCoalesceVuetify } from "@/install";
import {
  mount,
  DOMWrapper,
  createWrapperError,
  enableAutoUnmount,
  ComponentMountingOptions,
  VueWrapper,
  flushPromises,
} from "@vue/test-utils";
import { ArgumentsType } from "vitest";
import {
  defineComponent,
  h,
  ComponentPublicInstance,
  DefineComponent,
  ComponentInstance,
} from "vue";
import type {
  ComponentExposed,
  ComponentProps,
} from "vue-component-type-helpers";
import { createRouter, createWebHistory } from "vue-router";

import { createVuetify } from "vuetify";
import * as components from "vuetify/components";
import * as directives from "vuetify/directives";
import $metadata from "@test-targets/metadata.g";

global.matchMedia ??= function (str: string) {
  return {
    matches: false,
    media: str,
    addEventListener() {},
    dispatchEvent() {
      return false;
    },
    onchange() {},
    removeEventListener() {},
    addListener() {},
    removeListener() {},
  };
};
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

// HACK, pending release of https://github.com/vuejs/test-utils/pull/2242.
// Adapted from node_modules/@vue/test-utils/dist/mount.d.ts
type ComponentData<T> = T extends {
  data?(...args: any): infer D;
}
  ? D
  : {};
declare function betterMount<
  T,
  // BEGIN MODIFICATION
  C = T extends (props: infer Props, ...args: any) => any
    ? DefineComponent<
        Props extends Readonly<(infer PropNames)[]> | (infer PropNames)[]
          ? {
              [key in PropNames extends string ? PropNames : string]?: any;
            }
          : Props
      >
    : // END MODIFICATION
    T extends ((...args: any) => any) | (new (...args: any) => any)
    ? T
    : T extends {
        props?: infer Props;
      }
    ? DefineComponent<
        Props extends Readonly<(infer PropNames)[]> | (infer PropNames)[]
          ? {
              [key in PropNames extends string ? PropNames : string]?: any;
            }
          : Props
      >
    : DefineComponent,
  P extends ComponentProps<C> = ComponentProps<C>
>(
  originalComponent: T,
  options?: ComponentMountingOptions<T>
): VueWrapper<
  ComponentProps<C> & ComponentData<C> & ComponentExposed<C>,
  ComponentPublicInstance<
    ComponentProps<C>,
    ComponentData<C> & ComponentExposed<C> & Omit<P, keyof ComponentProps<C>>
  >
>;
// END HACK

// HACK: https://github.com/vuejs/language-tools/issues/3206#issuecomment-1624541884
export type BetterComponentInstance<T> = T extends new (
  ...args: any[]
) => infer R
  ? R
  : T extends (...args: any[]) => infer R
  ? R extends { __ctx?: infer K }
    ? Exclude<K, void> extends { expose: (...args: infer K2) => void }
      ? K2[0] & ComponentInstance<T>
      : any
    : any
  : any;

// HACK: https://github.com/vuejs/test-utils/issues/2254
declare module "@vue/test-utils" {
  interface BaseWrapper<ElementType extends Node> {
    findComponent<T extends (...args: any) => any>(
      selector: T
    ): VueWrapper<
      T extends (props: infer Props, ...args: any) => infer C
        ? BetterComponentInstance<T>
        : ComponentPublicInstance
    >;
  }
}

declare module "vue/jsx-runtime" {
  namespace JSX {
    export interface IntrinsicAttributes {
      // Make tsx shut up about extra attributes on elements, which is a perfectly valid thing.
      [name: string]: any;
    }
  }
}

const mountVuetify = function (
  component: ArgumentsType<typeof betterMount>[0],
  options: ArgumentsType<typeof betterMount>[1]
) {
  const wrapper = mount(component, {
    ...options,
    global: {
      plugins: [
        vuetify,
        coalesceVuetify,
        createRouter({
          history: createWebHistory(),
          routes: [
            {
              path: "/",
              component: async () => h("div"),
            },
            {
              path: "/admin/:type",
              name: "coalesce-admin-list",
              component: CAdminTablePage,
              props: (route) => ({ ...route.params, color: "primary" }),
            },
            {
              path: "/admin/:type/item/:id?",
              name: "coalesce-admin-item",
              component: CAdminEditorPage,
              props: (route) => ({ ...route.params, color: "primary" }),
            },
          ],
        }),
      ],
    },
  });

  return wrapper;
} as typeof betterMount;

const mountApp = function (
  component: ArgumentsType<typeof betterMount>[0],
  options: ArgumentsType<typeof betterMount>[1]
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
} as typeof betterMount;

export function getWrapper(selector = ".v-overlay-container") {
  const el = document.querySelector(selector);
  if (el) return new DOMWrapper(el);
  else return createWrapperError<DOMWrapper<Element>>("DOMWrapper");
}

export async function delay(ms: number) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

export async function openMenu(wrapper: VueWrapper) {
  await flushPromises();
  await wrapper.find(".v-field").trigger("click");
  await flushPromises();
  return getWrapper(".v-overlay__content");
}

export { nextTick } from "vue";
export { flushPromises } from "@vue/test-utils";
export { mockEndpoint } from "coalesce-vue/lib/test-utils";

export { mountVuetify as mount, mountApp };
