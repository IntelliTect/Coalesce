/* eslint-disable vue/one-component-per-file */

import { defineComponent, h, ref } from "vue";
import {
  createRouter,
  RouterView,
  createWebHistory,
  RouterLink,
  createMemoryHistory,
  type Router,
} from "vue-router";
import {
  bindToQueryString,
  useBindListParametersToQueryString,
  type VueInstance,
} from "../src";
import { ListParameters } from "../src/api-client";
import { mount } from "@vue/test-utils";
import { delay } from "./test-utils";
import { reactive } from "vue";
import { Person, PersonCriteria } from "@test-targets/models.g";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

describe("bindToQueryString", () => {
  let router: Router;
  let app: any;

  async function runTest(func: (v: VueInstance) => void) {
    router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: "/",
          component: defineComponent({
            created() {
              func(this);
            },
            render: () => h("div"),
          }),
        },
      ],
    });

    app = mount(
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

    router.push("/");
    await delay(10);
  }

  afterEach(async () => {
    if (router) {
      await router.push("/");
    }
    if (app) {
      app.unmount();
    }
  });

  test("object+key", async () => {
    const dateRef = ref<Date>();
    await runTest(async (v) => {
      // Bind to object + key
      bindToQueryString(v, dateRef, "value", {
        parse(v) {
          return v ? new Date(v) : undefined;
        },
        stringify(v) {
          return v.toISOString();
        },
      });
    });

    dateRef.value = new Date(123455667);
    await delay(1);
    expect(router.currentRoute.value.query.value).toBe(
      "1970-01-02T10:17:35.667Z",
    );
  });

  test("direct bind to ref", async () => {
    const dateRef = ref<Date>();
    await runTest(async (v) => {
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
    });

    dateRef.value = new Date(123455667);
    await delay(1);
    expect(router.currentRoute.value.query.foo).toBe(
      "1970-01-02T10:17:35.667Z",
    );
  });

  test("Direct bind to ref with queryKey shorthand", async () => {
    const stringRef = ref<string>();
    await runTest(async (v) => {
      // Direct bind to ref with queryKey shorthand
      bindToQueryString(v, stringRef, "foo");
    });

    stringRef.value = "qwerty";
    await delay(1);
    expect(router.currentRoute.value.query.foo).toBe("qwerty");
  });

  test("bad types", async () => {
    await runTest(async (v) => {
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
    await runTest(async (v) => {
      bindToQueryString(v, dataSource, "allowedStatuses");
    });

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

  test("bound to object", async () => {
    const dataSource = reactive(new Person.DataSources.ParameterTestsSource());
    await runTest(async (v) => {
      bindToQueryString(v, dataSource, "personCriterion");
    });

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

  test("does not put values on new route when changing routes", async () => {
    let changeBoundValue: () => void;
    const router = createRouter({
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

    const _app = mount(
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
    const router = createRouter({
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

    const _app = mount(
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

    const router = createRouter({
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

    const _app = mount(
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

    await runTest(async (v) => {
      // Bind page and pageSize to query string without specifying parse functions
      bindToQueryString(v, listParams, "page");
      bindToQueryString(v, listParams, "pageSize");
    });

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

  test("auto-detects ListViewModel $ properties with query key transformation and numeric parsing", async () => {
    // Create a mock ListViewModel-like object with $ properties
    const listViewModel = new ComplexModelListViewModel();

    await runTest(async (v) => {
      // Bind $ properties to query string without specifying options
      bindToQueryString(v, listViewModel, "$page");
      bindToQueryString(v, listViewModel, "$pageSize");
      bindToQueryString(v, listViewModel, "$search");
    });

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

describe("useBindListParametersToQueryString", () => {
  let router: Router;
  let listViewModel: ComplexModelListViewModel;
  let app: any;

  async function runComposableTest(
    initLoc = "/",
    setupFunc?: (vm: ComplexModelListViewModel) => any,
  ) {
    let setupResult: any;

    router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: "/",
          component: defineComponent({
            setup() {
              listViewModel = new ComplexModelListViewModel();
              if (setupFunc) {
                setupResult = setupFunc(listViewModel);
              }
              return { listViewModel };
            },
            render: () => h("div"),
          }),
        },
      ],
    });

    app = mount(
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

    router.push(initLoc);
    await delay(10);

    return { listViewModel, router, setupResult };
  }

  afterEach(async () => {
    if (router) {
      await router.push("/");
    }
    if (app) {
      app.unmount();
    }
  });

  test("automatically sets up parameter binding", async () => {
    const { listViewModel, router, setupResult } = await runComposableTest(
      "/",
      (listViewModel) => {
        // Call the composable - this is the only thing that should happen in setup
        return useBindListParametersToQueryString(listViewModel);
      },
    );

    const cleanup = setupResult;

    // Change parameters programmatically
    listViewModel.$page = 3;
    listViewModel.$pageSize = 25;
    listViewModel.$search = "test query";

    await delay(1);

    // Should update query string
    expect(router.currentRoute.value.query.page).toBe("3");
    expect(router.currentRoute.value.query.pageSize).toBe("25");
    expect(router.currentRoute.value.query.search).toBe("test query");

    // Navigate with query parameters
    await router.push("/?page=5&pageSize=50&search=new search");
    await delay(1);

    // Should update view model parameters
    expect(listViewModel.$page).toBe(5);
    expect(listViewModel.$pageSize).toBe(50);
    expect(listViewModel.$search).toBe("new search");

    // Cleanup should stop watchers
    cleanup();
  });

  test("handles baseline parameters correctly", async () => {
    const { listViewModel, router, setupResult } = await runComposableTest(
      "/",
      (listViewModel) => {
        // Set up initial state
        listViewModel.$page = 1; // default value
        listViewModel.$pageSize = 10; // default value
        listViewModel.$search = null;

        return useBindListParametersToQueryString(listViewModel);
      },
    );

    const cleanup = setupResult;

    await delay(1);

    // Default values should not appear in query string
    expect(router.currentRoute.value.query.page).toBeUndefined();
    expect(router.currentRoute.value.query.pageSize).toBeUndefined();
    expect(router.currentRoute.value.query.search).toBeUndefined();

    // Change to non-default values
    listViewModel.$page = 2;
    listViewModel.$pageSize = 20;

    await delay(1);

    // Non-default values should appear in query string
    expect(router.currentRoute.value.query.page).toBe("2");
    expect(router.currentRoute.value.query.pageSize).toBe("20");

    // Change back to default values
    listViewModel.$page = 1;
    listViewModel.$pageSize = 10;

    await delay(1);

    // Should remove from query string when back to defaults
    expect(router.currentRoute.value.query.page).toBeUndefined();
    expect(router.currentRoute.value.query.pageSize).toBeUndefined();

    cleanup();
  });

  test("preserves existing query parameters", async () => {
    const { listViewModel, router, setupResult } = await runComposableTest(
      "/?existingParam=value&anotherParam=test",
      (listViewModel) => {
        return useBindListParametersToQueryString(listViewModel);
      },
    );

    expect(router.currentRoute.value.query.existingParam).toBe("value");
    expect(router.currentRoute.value.query.anotherParam).toBe("test");

    const cleanup = setupResult;

    // Change list parameters
    listViewModel.$page = 3;

    await delay(1);

    // Should preserve existing params while adding new ones
    console.log(router.currentRoute.value.query);
    expect(router.currentRoute.value.query.existingParam).toBe("value");
    expect(router.currentRoute.value.query.anotherParam).toBe("test");
    expect(router.currentRoute.value.query.page).toBe("3");

    cleanup();
  });

  test("handles filter parameters", async () => {
    const { listViewModel, router, setupResult } = await runComposableTest(
      "/",
      (listViewModel) => {
        return useBindListParametersToQueryString(listViewModel);
      },
    );

    const cleanup = setupResult;

    // Set filter parameters
    listViewModel.$filter = {
      name: "John",
      age: 25,
      isActive: true,
    };

    await delay(1);

    // Should serialize filter parameters with filter. prefix
    expect(router.currentRoute.value.query["filter.name"]).toBe("John");
    expect(router.currentRoute.value.query["filter.age"]).toBe("25");
    expect(router.currentRoute.value.query["filter.isActive"]).toBe("true");

    // Navigate with filter query params
    await router.push("/?filter.name=Jane&filter.age=30&filter.isActive=false");
    await delay(1);

    // Should update filter object
    expect(listViewModel.$filter.name).toBe("Jane");
    expect(listViewModel.$filter.age).toBe("30"); // Note: comes back as string from query
    expect(listViewModel.$filter.isActive).toBe("false"); // Note: comes back as string from query

    cleanup();
  });

  test("handles complex filter and data source parameters", async () => {
    const { listViewModel, router, setupResult } = await runComposableTest(
      "/",
      (listViewModel) => {
        return useBindListParametersToQueryString(listViewModel);
      },
    );

    const cleanup = setupResult;

    // Set up complex parameters
    listViewModel.$includes = "details,related";
    listViewModel.$orderBy = "name";
    listViewModel.$orderByDescending = "createdDate";

    await delay(1);

    expect(router.currentRoute.value.query.includes).toBe("details,related");
    expect(router.currentRoute.value.query.orderBy).toBe("name");
    expect(router.currentRoute.value.query.orderByDescending).toBe(
      "createdDate",
    );

    // Update from query string
    await router.push(
      "/?includes=basic&orderBy=id&orderByDescending=modifiedDate",
    );
    await delay(1);

    expect(listViewModel.$includes).toBe("basic");
    expect(listViewModel.$orderBy).toBe("id");
    expect(listViewModel.$orderByDescending).toBe("modifiedDate");

    cleanup();
  });
});
