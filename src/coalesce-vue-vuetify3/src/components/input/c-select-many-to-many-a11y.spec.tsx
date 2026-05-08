import { mountApp, flushPromises, mockEndpoint } from "@test/util";
import { CSelectManyToMany } from "..";
import { Product } from "@test-targets/models.g";
import { CaseViewModel } from "@test-targets/viewmodels.g";

// Rules to disable that are caused by Vuetify internals, not our code:
// - region: Vuetify renders overlays outside landmark regions
const axeOptions = {
  rules: {
    region: { enabled: false },
  },
};

describe("CSelectManyToMany accessibility (axe-core)", () => {
  beforeEach(() => {
    mockEndpoint("/Product/list", () => ({
      wasSuccessful: true,
      list: [
        new Product({ productId: 1, name: "Widget A" }),
        new Product({ productId: 2, name: "Widget B" }),
      ],
      page: 1,
      pageCount: 1,
      pageSize: 10,
      totalCount: 2,
    }));
  });

  test("passes axe-core checks when closed", async () => {
    const { run } = await import("axe-core");
    const model = new CaseViewModel();
    mountApp(() => <CSelectManyToMany model={model} for="caseProducts" />);
    await flushPromises();

    const results = await run(document.body, axeOptions);
    expect(results.violations).toEqual([]);
  });
});
