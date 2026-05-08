import { mount } from "@test/util";
import { CDisplay } from "..";
import { ComplexModel, Statuses } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CDisplay accessibility (axe-core)", () => {
  test("string value passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ name: "bob" });
    mount(() => <CDisplay model={model} for="name" />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("enum value passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModel({ enumNullable: Statuses.InProgress });
    mount(() => <CDisplay model={model} for="enumNullable" />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("boolean value passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ isActive: true });
    mount(() => <CDisplay model={model} for="isActive" />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("date value passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({});
    model.dateTime = new Date("2023-08-16T01:02:03Z");
    mount(() => <CDisplay model={model} for="dateTime" />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("color value passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModel({ color: "#ff0000" });
    mount(() => <CDisplay model={model} for="color" />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("whole model passes axe-core checks", async () => {
    const { run } = await import("axe-core");
    const model = new ComplexModelViewModel({ name: "bob" });
    mount(() => <CDisplay model={model} />);

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
