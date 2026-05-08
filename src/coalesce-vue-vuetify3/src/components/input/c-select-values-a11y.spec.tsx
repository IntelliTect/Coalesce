import { mountApp, flushPromises } from "@test/util";
import { CSelectValues } from "..";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CSelectValues accessibility (axe-core)", () => {
  test("passes axe-core checks when empty", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel();
    mountApp(() => (
      <CSelectValues model={model} for="mutablePrimitiveCollection" />
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks with values", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel();
    model.mutablePrimitiveCollection = ["tag1", "tag2", "tag3"];
    mountApp(() => (
      <CSelectValues model={model} for="mutablePrimitiveCollection" />
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
