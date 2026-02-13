import { flushPromises, mount } from "@test/util";
import { CListPageSize } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

describe("CListPageSize", () => {
  test("renders with default items and has accessible label", async () => {
    const list = new ComplexModelListViewModel();

    const wrapper = mount(() => <CListPageSize list={list} />);
    await flushPromises();

    // Should show "Page Size" text
    expect(wrapper.text()).toContain("Page Size");

    // Should have a select element with dropdown class
    const selectWrapper = wrapper.find(".c-list-page-size--dropdown");
    expect(selectWrapper.exists()).toBe(true);

    // The aria-label should be present on the select or its input element
    const ariaLabel = 
      selectWrapper.attributes("aria-label") || 
      wrapper.find("input").attributes("aria-label");
    expect(ariaLabel).toBe("Page Size");
  });

  test("renders with custom items", async () => {
    const list = new ComplexModelListViewModel();
    const customItems = [5, 15, 50];

    const wrapper = mount(() => (
      <CListPageSize list={list} items={customItems} />
    ));
    await flushPromises();

    // The aria-label should be present on the select wrapper or input
    const selectWrapper = wrapper.find(".c-list-page-size--dropdown");
    const ariaLabel = 
      selectWrapper.attributes("aria-label") || 
      wrapper.find("input").attributes("aria-label");
    expect(ariaLabel).toBe("Page Size");
  });
});
