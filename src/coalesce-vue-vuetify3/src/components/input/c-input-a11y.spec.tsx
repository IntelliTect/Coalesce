import { mountApp, flushPromises, mockEndpoint } from "@test/util";
import { CInput } from "..";
import { Statuses, Test } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CInput accessibility (axe-core)", () => {
  test("string input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ name: "bob" });
    mountApp(() => <CInput model={model} for="name" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("number input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ intNullable: 42 });
    mountApp(() => <CInput model={model} for="intNullable" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("boolean switch input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ isActive: true });
    mountApp(() => <CInput model={model} for="isActive" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("boolean checkbox input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ isActive: true });
    mountApp(() => <CInput model={model} for="isActive" checkbox />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("enum input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({
      enumNullable: Statuses.InProgress,
    });
    mountApp(() => <CInput model={model} for="enumNullable" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("date input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({});
    model.dateTime = new Date("2023-08-16T01:02:03Z");
    mountApp(() => <CInput model={model} for="dateTime" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("model/FK input passes axe-core checks", async () => {
    const { run } = await import("axe-core");
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
    const model = new ComplexModelViewModel({});
    mountApp(() => <CInput model={model} for="singleTest" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
