/// <reference path="../node_modules/vitest/globals.d.ts" />

import { defineComponent, h, nextTick } from "vue";
import {
  createRouter,
  RouterView,
  createWebHistory,
  RouterLink,
} from "vue-router";
import { bindToQueryString } from "../src";
import { mount } from "@vue/test-utils";
import { delay } from "../test/test-utils";

describe("bindToQueryString", () => {
  test("does not put values on new route when changing routes", async () => {
    // TO VALIDATE IF THE UNDERLYING PROBLEM IS STILL A PROBLEM:
    // Comment the code in bindToQueryString that checks for `isUnmounted` and does a $nextTick.

    let changeBoundValue: Function;
    var router = createRouter({
      history: createWebHistory(), // Note: memory history doesn't reproduce the bug.
      routes: [
        {
          path: "/",
          component: defineComponent({
            data() {
              return { boundValue: "asdf" };
            },
            created() {
              changeBoundValue = () => (this.boundValue = "qwerty");
              bindToQueryString(this, this, "boundValue");
            },
            render: () => h("div"),
          }),
        },
        {
          path: "/two",
          component: defineComponent({ render: () => h("div") }),
        },
      ],
    });

    const app = mount(
      defineComponent({
        render() {
          return h("div", [
            // IMPORTANT: The original bug only happens when there is a dependency on the current route
            // in at least two different places that are outside of the router-view mounted component.
            // This is a very likely and common case in any real world app
            // (e.g.nav buttons that highlight the button for the current page).
            // I have no idea why this is the case. It just is. Yes, its really weird.
            // Here, we have an immediate dependency:
            router.currentRoute.value.path,
            // And a dependency nested inside a component.
            // Doesn't matter if you have more than one RouterLink here, the behavior is the same
            h(RouterLink, { to: "/" }, () => "link to home"),
            h(RouterView),
          ]);
        },
      }),
      {
        global: { plugins: [router] },
        attachTo: document.body,
      }
    );

    // wait for mount
    await delay(1);

    // Change the bound value and wait for it to propagate into the querystring.
    changeBoundValue!();
    await delay(1);
    expect(router.currentRoute.value.query.boundValue).toBe("qwerty");

    // Navigate to another page, and wait for the querystring to update.
    await router.push("/two");

    // Value from the previous page should not be in the querystring.
    await delay(1);
    expect(router.currentRoute.value.query.boundValue).toBe(undefined);
  });
});
