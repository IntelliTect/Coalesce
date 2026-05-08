import { flushPromises, mount, mockEndpoint } from "@test/util";
import { CAdminTablePage } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

const emptyList = () => ({
  wasSuccessful: true,
  list: [],
  page: 1,
  pageCount: 0,
  pageSize: 10,
  totalCount: 0,
});

describe("CAdminTablePage accessibility (axe-core)", () => {
  beforeEach(() => {
    // Mock all FK/reference endpoints that ComplexModel's c-select fields will call
    mockEndpoint("/Test/list", emptyList);
    mockEndpoint("/EnumPk/list", emptyList);
    mockEndpoint("/Company/list", emptyList);
  });

  test("passes axe-core checks with empty list", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/list", emptyList);

    const list = new ComplexModelListViewModel();
    await list.$load();
    mount(() => <CAdminTablePage list={list} />, {
      attachTo: document.body,
    });
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

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
    mount(() => <CAdminTablePage list={list} />, {
      attachTo: document.body,
    });
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
