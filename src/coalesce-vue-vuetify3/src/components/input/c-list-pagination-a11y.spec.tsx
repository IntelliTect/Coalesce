import { mount, flushPromises, mockEndpoint } from "@test/util";
import { CListPagination } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CListPagination accessibility (axe-core)", () => {
  test("passes axe-core checks with pagination", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", () => ({
      wasSuccessful: true,
      list: [
        { complexModelId: 1, name: "Item 1" },
        { complexModelId: 2, name: "Item 2" },
      ],
      page: 1,
      pageSize: 10,
      pageCount: 5,
      totalCount: 50,
    }));

    const list = new ComplexModelListViewModel();
    await list.$load();
    mount(() => <CListPagination list={list} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks without page count", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", () => ({
      wasSuccessful: true,
      list: [
        { complexModelId: 1, name: "Item 1" },
        { complexModelId: 2, name: "Item 2" },
      ],
      page: 1,
      pageSize: 10,
      pageCount: -1,
      totalCount: -1,
    }));

    const list = new ComplexModelListViewModel();
    list.$params.noCount = true;
    await list.$load();
    mount(() => <CListPagination list={list} />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
