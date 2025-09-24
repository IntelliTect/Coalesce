import { createApp, defineComponent, h, nextTick, ref } from "vue";
import {
  createRouter,
  RouterView,
  createWebHistory,
  RouterLink,
  createMemoryHistory,
  type Router,
} from "vue-router";
import { bindToQueryString, type VueInstance } from "../src";
import { ListParameters } from "../src/api-client";
import { mount } from "@vue/test-utils";
import { delay } from "./test-utils";
import { reactive } from "vue";
import { Person, PersonCriteria } from "@test-targets/models.g";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

describe("bindToQueryString", () => {
  async function runTest(func: (v: VueInstance, router: Router) => void) {
    var router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: "/",
          component: defineComponent({
            created() {
              func(this, router);
            },
            render: () => h("div"),
          }),
        },
      ],
    });
    const app = mount(
      defineComponent({
        render() {
          return h(RouterView);
        },
      }),
      {
        global: { plugins: [router] },
        attachTo: document.body,
      },
    );

    await delay(10);
  }

  test("object+key", async () => {
    const dateRef = ref<Date>();
    await runTest(async (v, router) => {
      // Bind to object + key
      bindToQueryString(v, dateRef, "value", {
        parse(v) {
          return v ? new Date(v) : undefined;
        },
        stringify(v) {
          return v.toISOString();
        },
      });
      dateRef.value = new Date(123455667);
      await delay(1);
      expect(router.currentRoute.value.query.value).toBe(
        "1970-01-02T10:17:35.667Z",
      );
    });
  });

  test("direct bind to ref", async () => {
    const dateRef = ref<Date>();
    await runTest(async (v, router) => {
      // Direct bind to ref
      bindToQueryString(v, dateRef, {
        queryKey: "foo",
        parse(v) {
          return v ? new Date(v) : undefined;
        },
        stringify(v) {
          return v.toISOString();
        },
      });
      dateRef.value = new Date(123455667);
      await delay(1);
      expect(router.currentRoute.value.query.foo).toBe(
        "1970-01-02T10:17:35.667Z",
      );
    });
  });

  test("Direct bind to ref with queryKey shorthand", async () => {
    const stringRef = ref<string>();
    await runTest(async (v, router) => {
      // Direct bind to ref with queryKey shorthand
      bindToQueryString(v, stringRef, "foo");
      stringRef.value = "qwerty";
      await delay(1);
      expect(router.currentRoute.value.query.foo).toBe("qwerty");
    });
  });

  test("bad types", async () => {
    await runTest(async (v, router) => {
      //@ts-expect-error Missing queryKey in options object.
      () => bindToQueryString(v, dateRef, {});
      //@ts-expect-error Missing options or queryKey.
      () => bindToQueryString(v, dateRef);
    });
  });

  test("bound to primitive collection", async () => {
    const dataSource = reactive(
      new Person.DataSources.NamesStartingWithAWithCases(),
    );
    await runTest(async (v, router) => {
      bindToQueryString(v, dataSource, "allowedStatuses");
      dataSource.allowedStatuses = [1, 2];
      await delay(1);
      expect(router.currentRoute.value.query.allowedStatuses).toBe("[1,2]");

      router.push("/?allowedStatuses=2,3");
      await delay(1);
      expect(dataSource.allowedStatuses).toStrictEqual([2, 3]);

      router.push("/?allowedStatuses=");
      await delay(1);
      expect(dataSource.allowedStatuses).toStrictEqual([]);
    });
  });

  test("bound to object", async () => {
    const dataSource = reactive(new Person.DataSources.ParameterTestsSource());
    await runTest(async (v, router) => {
      bindToQueryString(v, dataSource, "personCriterion");
      dataSource.personCriterion = new PersonCriteria({
        name: "b&personCriterion=ob",
        personIds: [1, 2],
      });
      await delay(1);
      expect(router.currentRoute.value.query.personCriterion).toBe(
        `{"personIds":[1,2],"name":"b&personCriterion=ob"}`,
      );

      router.push(`/?personCriterion={"name":"bob2","personIds":[1,2,3]}`);
      await delay(1);
      expect(dataSource.personCriterion).toMatchObject(
        new PersonCriteria({
          name: "bob2",
          personIds: [1, 2, 3],
        }),
      );

      router.push("/?personCriterion=");
      await delay(1);
      expect(dataSource.personCriterion).toMatchObject(new PersonCriteria({}));
    });
  });

  test("does not put values on new route when changing routes", async () => {
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
      },
    );

    try {
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
    } finally {
      // Cleanup: reset route for other tests
      await router.push("/");
    }
  });

  test("sets query value before returning", async () => {
    let boundValueNewValue;
    var router = createRouter({
      history: createMemoryHistory(),
      routes: [
        {
          path: "/",
          component: defineComponent({ render: () => h("div") }),
        },
        {
          path: "/two",
          component: defineComponent({
            data() {
              return { boundValue: "" };
            },
            created() {
              bindToQueryString(this, this, "boundValue");
              boundValueNewValue = this.boundValue;
            },
            render: () => h("div"),
          }),
        },
      ],
    });

    const app = mount(
      defineComponent({
        render() {
          return h(RouterView);
        },
      }),
      {
        global: { plugins: [router] },
        attachTo: document.body,
      },
    );

    try {
      // wait for mount
      await delay(1);

      // Navigate to another page that uses binding
      await router.push("/two?boundValue=foo");

      // Value from the route should now be in boundValueNewValue.
      await delay(1);
      expect(boundValueNewValue).toBe("foo");
    } finally {
      // Cleanup: reset route for other tests
      await router.push("/");
    }
  });

  test("handles multiple bound values changing simultaneously", async () => {
    let setBoundValues: (v: string | null) => void;

    var router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: "/",
          component: defineComponent({
            data() {
              return {
                boundValue: "" as string | null,
                boundValue2: "" as string | null,
              };
            },
            created() {
              setBoundValues = (b) => (this.boundValue = this.boundValue2 = b);
              bindToQueryString(this, this, "boundValue");
              bindToQueryString(this, this, "boundValue2");
            },
            render: () => h("div"),
          }),
        },
      ],
    });

    const app = mount(
      defineComponent({
        render() {
          return h(RouterView);
        },
      }),
      {
        global: { plugins: [router] },
      },
    );

    // wait for mount
    await delay(1);

    // Change the bound value and wait for it to propagate into the querystring.
    setBoundValues!("qwerty");
    await delay(1);
    expect(router.currentRoute.value.query.boundValue).toBe("qwerty");
    expect(router.currentRoute.value.query.boundValue2).toBe("qwerty");

    // Change the bound value again:
    setBoundValues!(null);

    // Values should be gone:
    await delay(1);
    expect(router.currentRoute.value.query.boundValue).toBe(undefined);
    expect(router.currentRoute.value.query.boundValue2).toBe(undefined);
  });

  test("auto-detects ListParameters and parses numeric fields", async () => {
    const listParams = reactive(new ListParameters());

    await runTest(async (v, router) => {
      // Bind page and pageSize to query string without specifying parse functions
      bindToQueryString(v, listParams, "page");
      bindToQueryString(v, listParams, "pageSize");

      // Navigate to a URL with string values in query
      router.push("/?page=5&pageSize=25");
      await delay(1);

      // Values should be automatically parsed to numbers
      expect(listParams.page).toStrictEqual(5);
      expect(listParams.pageSize).toStrictEqual(25);

      // Test with invalid numeric values
      router.push("/?page=invalid&pageSize=notanumber");
      await delay(1);

      // Invalid values should revert to defaults
      expect(listParams.page).toStrictEqual(1);
      expect(listParams.pageSize).toStrictEqual(10);

      // Test setting values programmatically
      listParams.page = 3;
      listParams.pageSize = 50;
      await delay(1);

      expect(router.currentRoute.value.query.page).toBe("3");
      expect(router.currentRoute.value.query.pageSize).toBe("50");
    });
  });

  test.only("auto-detects ListViewModel $ properties with query key transformation and numeric parsing", async () => {
    // Create a mock ListViewModel-like object with $ properties
    const listViewModel = new ComplexModelListViewModel();

    await runTest(async (v, router) => {
      // Bind $ properties to query string without specifying options
      bindToQueryString(v, listViewModel, "$page");
      bindToQueryString(v, listViewModel, "$pageSize");
      bindToQueryString(v, listViewModel, "$search");

      // Navigate to a URL with values - query keys should drop the $ prefix
      router.push("/?page=7&pageSize=30&search=test");
      await delay(1);

      // Numeric values should be automatically parsed
      expect(listViewModel.$page).toStrictEqual(7);
      expect(listViewModel.$pageSize).toStrictEqual(30);
      expect(listViewModel.$search).toStrictEqual("test");

      // Test with invalid numeric values
      router.push("/?page=invalid&pageSize=notanumber&search=valid");
      await delay(1);

      // Invalid numeric values should revert to defaults, strings should work
      expect(listViewModel.$page).toStrictEqual(1);
      expect(listViewModel.$pageSize).toStrictEqual(10);
      expect(listViewModel.$search).toStrictEqual("valid");

      // Test setting values programmatically
      listViewModel.$page = 5;
      listViewModel.$pageSize = 100;
      listViewModel.$search = "programmatic";
      await delay(1);

      // Query should use keys without $ prefix
      expect(router.currentRoute.value.query.page).toBe("5");
      expect(router.currentRoute.value.query.pageSize).toBe("100");
      expect(router.currentRoute.value.query.search).toBe("programmatic");
    });
  });
});
