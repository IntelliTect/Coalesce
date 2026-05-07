import { mountApp, openMenu, flushPromises, mockEndpoint } from "@test/util";
import { CSelect } from "..";
import { Test } from "@test-targets/models.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CSelect accessibility (axe-core)", () => {
  beforeEach(() => {
    mockEndpoint("/Test/list", () => ({
      wasSuccessful: true,
      list: [
        new Test({ testId: 101, testName: "foo 101" }),
        new Test({ testId: 202, testName: "bar 202" }),
      ],
      page: 1,
      pageCount: 1,
      pageSize: 10,
      totalCount: 2,
    }));
  });

  test("passes axe-core checks when closed", async () => {
    const { run } = await import("axe-core");
    mountApp(() => <CSelect for="Test"></CSelect>);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when open (single)", async () => {
    const { run } = await import("axe-core");
    const wrapper = mountApp(() => <CSelect for="Test"></CSelect>);
    await openMenu(wrapper.findComponent(CSelect));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when open (multiple)", async () => {
    const { run } = await import("axe-core");
    const wrapper = mountApp(() => <CSelect for="Test" multiple></CSelect>);
    await openMenu(wrapper.findComponent(CSelect));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
