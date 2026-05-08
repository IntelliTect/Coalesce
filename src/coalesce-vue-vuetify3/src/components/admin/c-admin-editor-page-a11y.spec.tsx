import { flushPromises, mount, mockEndpoint } from "@test/util";
import { CAdminEditorPage } from "..";
import { ComplexModel } from "@test-targets/models.g";
import "@test-targets/viewmodels.g"; // registers ViewModel.typeLookup

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

describe("CAdminEditorPage accessibility (axe-core)", () => {
  beforeEach(() => {
    // Mock all FK/reference endpoints that ComplexModel's c-select fields will call
    mockEndpoint("/Test/list", emptyList);
    mockEndpoint("/EnumPk/list", emptyList);
    mockEndpoint("/Company/list", emptyList);
    mockEndpoint("/ComplexModel/list", emptyList);
    mockEndpoint("/ComplexModel/save", () => ({
      wasSuccessful: true,
      object: new ComplexModel({ complexModelId: 1, name: "Test Item" }),
    }));
  });

  test("passes axe-core checks for new item", async () => {
    const { run } = await import("axe-core");

    mount(() => <CAdminEditorPage type="ComplexModel" />, {
      attachTo: document.body,
    });
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });

  test("passes axe-core checks for existing item", async () => {
    const { run } = await import("axe-core");
    mockEndpoint("/ComplexModel/get", () => ({
      wasSuccessful: true,
      object: new ComplexModel({ complexModelId: 1, name: "Test Item" }),
    }));

    mount(() => <CAdminEditorPage type="ComplexModel" id="1" />, {
      attachTo: document.body,
    });
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
