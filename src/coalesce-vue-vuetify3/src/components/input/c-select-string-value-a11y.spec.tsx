import { mountApp, flushPromises, mockEndpoint } from "@test/util";
import { CSelectStringValue } from "..";
import { PersonViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CSelectStringValue accessibility (axe-core)", () => {
  beforeEach(() => {
    mockEndpoint(
      new PersonViewModel().$metadata.methods.namesStartingWith,
      () => ({
        wasSuccessful: true,
        object: ["Alice", "Amanda"],
      }),
    );
  });

  test("passes axe-core checks when empty", async () => {
    const { run } = await import("axe-core");
    const vm = new PersonViewModel();
    mountApp(() => (
      <CSelectStringValue model={vm} for="name" method="namesStartingWith" />
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks with value", async () => {
    const { run } = await import("axe-core");
    const vm = new PersonViewModel({ name: "Alice" });
    mountApp(() => (
      <CSelectStringValue model={vm} for="name" method="namesStartingWith" />
    ));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
