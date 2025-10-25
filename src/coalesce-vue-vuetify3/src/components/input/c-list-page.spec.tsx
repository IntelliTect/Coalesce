import { flushPromises, mount } from "@test/util";
import { CListPage } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";
import { mockEndpoint } from "coalesce-vue/lib/test-utils";

describe("CListPage", () => {
  test("regular list with pageCount", async () => {
    const list = new ComplexModelListViewModel();

    // Mock the API response with pagination info
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

    // Load the list to populate page count
    await list.$load();

    const wrapper = mount(() => <CListPage list={list} />);
    await flushPromises();

    // Should show "Page X of 5"
    expect(wrapper.text()).toContain("Page");
    expect(wrapper.text()).toContain("of 5");

    // Should have a page number input
    const input = wrapper.find('input[type="number"]');
    expect(input.exists()).toBe(true);
    expect((input.element as HTMLInputElement).value).toBe("1");

    // Input should have a max attribute set to the page count
    expect(input.attributes("max")).toBe("5");

    // Previous button should be disabled on first page
    const buttons = wrapper.findAll("button");
    const prevButton = buttons[0];
    expect(prevButton.attributes("disabled")).toBeDefined();

    // Next button should be enabled
    const nextButton = buttons[1];
    expect(nextButton.attributes("disabled")).toBeUndefined();

    // Click next button
    await nextButton.trigger("click");
    await flushPromises();

    // Page should increment
    expect(list.$page).toBe(2);

    // Manually change the input value
    await input.setValue("4");
    await flushPromises();

    // Page should update to the new value
    expect(list.$page).toBe(4);
    expect((input.element as HTMLInputElement).value).toBe("4");
  });

  test("list with counting turned off (pageCount = -1)", async () => {
    const list = new ComplexModelListViewModel();
    // Disable counting by setting noCount to true
    list.$params.noCount = true;

    // Mock the API response without page count
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

    // Load the list
    await list.$load();

    const wrapper = mount(() => <CListPage list={list} />);
    await flushPromises();

    // Should show "Page X" without "of N"
    expect(wrapper.text()).toContain("Page");
    expect(wrapper.text()).not.toContain("of");

    // Should have a page number input
    const input = wrapper.find('input[type="number"]');
    expect(input.exists()).toBe(true);
    expect((input.element as HTMLInputElement).value).toBe("1");

    // Input should NOT have a max attribute when pageCount is -1
    expect(input.attributes("max")).toBeUndefined();

    // Previous button should be disabled on first page
    const buttons = wrapper.findAll("button");
    const prevButton = buttons[0];
    expect(prevButton.attributes("disabled")).toBeDefined();

    // Next button should be enabled (since we don't know if there's a next page)
    const nextButton = buttons[1];
    expect(nextButton.attributes("disabled")).toBeUndefined();

    // Click next button
    await nextButton.trigger("click");
    await flushPromises();

    // Page should increment
    expect(list.$page).toBe(2);

    // Manually change the input value
    await input.setValue("10");
    await flushPromises();

    // Page should update to the new value even without a max limit
    expect(list.$page).toBe(10);
    expect((input.element as HTMLInputElement).value).toBe("10");
  });
});
