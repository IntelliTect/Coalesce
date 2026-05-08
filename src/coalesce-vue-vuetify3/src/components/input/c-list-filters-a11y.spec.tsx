import { mountApp, flushPromises, getWrapper } from "@test/util";
import { CListFilters } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CListFilters accessibility (axe-core)", () => {
  test("passes axe-core checks when closed", async () => {
    const { run } = await import("axe-core");
    const list = new ComplexModelListViewModel();
    mountApp(() => <CListFilters list={list} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when open", async () => {
    const { run } = await import("axe-core");
    const list = new ComplexModelListViewModel();
    const wrapper = mountApp(() => (
      <CListFilters list={list} columnSelection />
    ));
    await flushPromises();

    // Open the filters menu
    await wrapper.find(".c-list-filters").trigger("click");
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
