import { mountApp, flushPromises, mockEndpoint } from "@test/util";
import { CTable } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CTable accessibility (axe-core)", () => {
  test("passes axe-core checks with data", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", () => ({
      wasSuccessful: true,
      list: [
        { complexModelId: 1, name: "Item 1" },
        { complexModelId: 2, name: "Item 2" },
      ],
      page: 1,
      pageSize: 10,
      pageCount: 1,
      totalCount: 2,
    }));

    const list = new ComplexModelListViewModel();
    await list.$load();
    mountApp(() => <CTable list={list} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks with empty data", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", () => ({
      wasSuccessful: true,
      list: [],
      page: 1,
      pageSize: 10,
      pageCount: 0,
      totalCount: 0,
    }));

    const list = new ComplexModelListViewModel();
    await list.$load();
    mountApp(() => <CTable list={list} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks with specific props", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", () => ({
      wasSuccessful: true,
      list: [
        { complexModelId: 1, name: "Item 1", isActive: true },
        { complexModelId: 2, name: "Item 2", isActive: false },
      ],
      page: 1,
      pageSize: 10,
      pageCount: 1,
      totalCount: 2,
    }));

    const list = new ComplexModelListViewModel();
    await list.$load();
    mountApp(() => <CTable list={list} props={["name", "isActive"]} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
