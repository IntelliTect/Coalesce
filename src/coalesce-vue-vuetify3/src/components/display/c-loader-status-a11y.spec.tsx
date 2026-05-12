import { mountApp, flushPromises } from "@test/util";
import CLS from "./c-loader-status.vue";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CLoaderStatus accessibility (axe-core)", () => {
  test("initial state passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const vm = new ComplexModelViewModel();
    mountApp(() => (
      <CLS loaders={vm.$load}>
        <div>Content</div>
      </CLS>
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("loading state passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const vm = new ComplexModelViewModel();
    vm.$load.isLoading = true;
    mountApp(() => (
      <CLS loaders={vm.$load}>
        <div>Content</div>
      </CLS>
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("error state passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const vm = new ComplexModelViewModel();
    vm.$load.wasSuccessful = false;
    vm.$load.message = "Something went wrong";
    mountApp(() => (
      <CLS loaders={vm.$load}>
        <div>Content</div>
      </CLS>
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("success state passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const vm = new ComplexModelViewModel();
    vm.$save.wasSuccessful = true;
    vm.$save.message = "Saved successfully";
    mountApp(() => (
      <CLS loaders={vm.$save} show-success>
        <div>Content</div>
      </CLS>
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("multiple loaders passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const vm = new ComplexModelViewModel();
    mountApp(() => (
      <CLS loaders={[vm.$load, vm.$save]}>
        <div>Content</div>
      </CLS>
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
