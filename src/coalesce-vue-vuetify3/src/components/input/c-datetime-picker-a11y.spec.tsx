import { mountApp, openMenu, flushPromises } from "@test/util";
import { CDatetimePicker } from "..";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CDatetimePicker accessibility (axe-core)", () => {
  test("passes axe-core checks when closed (no value)", async () => {
    const { run } = await import("axe-core");
    mountApp(() => <CDatetimePicker label="Pick a date" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when closed (with value)", async () => {
    const { run } = await import("axe-core");
    const date = new Date("2023-08-16T01:02:03Z");
    mountApp(() => <CDatetimePicker modelValue={date} label="Pick a date" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when bound to model", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({});
    model.dateTime = new Date("2023-08-16T01:02:03Z");
    mountApp(() => <CDatetimePicker model={model} for="dateTime" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks when picker menu is open", async () => {
    const { run } = await import("axe-core");
    const date = new Date("2023-08-16T01:02:03Z");
    const wrapper = mountApp(() => (
      <CDatetimePicker modelValue={date} label="Pick a date" />
    ));
    await openMenu(wrapper.findComponent(CDatetimePicker));
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks for date-only", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({});
    model.systemDateOnly = new Date("2023-08-16");
    mountApp(() => <CDatetimePicker model={model} for="systemDateOnly" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
