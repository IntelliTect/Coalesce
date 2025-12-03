import { flushPromises, getWrapper, mockEndpoint, mountApp } from "@test/util";
import { watch } from "vue";
import { CListFilters } from "..";
import {
  ComplexModelListViewModel,
  DateOnlyPkListViewModel,
} from "@test-targets/viewmodels.g";
import { DateOnlyPk } from "@test-targets/models.g";

describe("CListFilters", () => {
  function setupListAndWatcher(initialFilter?: Record<string, any>) {
    const list = new ComplexModelListViewModel();

    if (initialFilter) {
      Object.assign(list.$filter, initialFilter);
    }

    const watchTracker = vitest.fn();
    watch(() => list.$params, watchTracker, { deep: true });

    return { list, watchTracker };
  }

  async function mountFiltersComponent(list: ComplexModelListViewModel) {
    const wrapper = mountApp(() => (
      <CListFilters list={list} columnSelection></CListFilters>
    ));
    await flushPromises();
    return wrapper;
  }

  async function openPropertyFilter(wrapper: any, propertyName: string) {
    // Open the filters menu by clicking the filter button
    const filterButton = wrapper.find(".c-list-filters");
    await filterButton.trigger("click");
    await flushPromises();

    // Find and click on the filter button for the specified property
    const propFilterButton = getWrapper(".v-overlay__content")
      .findAll(".v-list-item")
      .find((item) => item.text().includes(propertyName))!
      .find(".fa-filter")
      .element.closest("button");
    expect(propFilterButton).toBeTruthy();

    propFilterButton!.click();
    await flushPromises();
  }

  async function openNameFilter(wrapper: any) {
    return openPropertyFilter(wrapper, "Name");
  }
  test("doesn't mutate list.$params without user interaction", async () => {
    // There was a bug where c-list-filters was initializing list.$params.filters
    // (if it wasn't set) on mount, which was then incorrectly triggering list autoload.
    const { list, watchTracker } = setupListAndWatcher();

    // Mount the component without any user action. list.$params should be unchanged.
    const _wrapper = mountApp(() => <CListFilters list={list}></CListFilters>);
    await flushPromises();
    expect(watchTracker).toBeCalledTimes(0);
  });

  test("triggers list.$params watcher when filter is changed through UI", async () => {
    const { list, watchTracker } = setupListAndWatcher();

    // Mount the component
    const wrapper = await mountFiltersComponent(list);
    expect(watchTracker).toBeCalledTimes(0);

    await openNameFilter(wrapper);

    // Find a text input within the filter component
    const textInput = getWrapper(".c-list-filter--prop-menu").find("input");
    expect(textInput.exists()).toBe(true);

    await textInput.setValue("test value");
    await flushPromises();

    // Verify that the params watcher was triggered
    expect(watchTracker).toBeCalledTimes(1);
    expect(list.$filter.name).toBe("test value");
  });

  test("sets null filter when 'No Value' button is clicked", async () => {
    const { list, watchTracker } = setupListAndWatcher();

    // Mount the component
    const wrapper = await mountFiltersComponent(list);

    await openNameFilter(wrapper);

    // Find and click the "No Value" button
    const noValueButton = getWrapper(".c-list-filter--prop-menu")
      .findAll("button")
      .find((btn) => btn.text().includes("No Value"));
    expect(noValueButton?.exists()).toBe(true);

    await noValueButton!.trigger("click");
    await flushPromises();

    // Verify that the params watcher was triggered and filter is set to null
    expect(watchTracker).toBeCalledTimes(1);
    expect(list.$filter.name).toBe(null);
  });

  test("removes filter when 'Remove' button is clicked", async () => {
    const { list, watchTracker } = setupListAndWatcher({
      name: "initial value",
    });

    // Mount the component
    const wrapper = await mountFiltersComponent(list);

    await openNameFilter(wrapper);

    // Find and click the "Remove" button
    const removeButton = getWrapper(".c-list-filter--prop-menu")
      .findAll("button")
      .find((btn) => btn.text().includes("Remove"));
    expect(removeButton?.exists()).toBe(true);

    await removeButton!.trigger("click");
    await flushPromises();

    // Verify that the filter was removed
    expect(watchTracker).toBeCalledTimes(1);
    expect(list.$filter.name).toBeUndefined();
  });

  test("sets empty string filter when 'Has Value' button is clicked after null", async () => {
    const { list, watchTracker } = setupListAndWatcher({ name: null });

    // Mount the component
    const wrapper = await mountFiltersComponent(list);

    // Verify initial filter is set to null
    expect(list.$filter.name).toBe(null);

    await openNameFilter(wrapper);

    // Find and click the "Has Value" button
    const hasValueButton = getWrapper(".c-list-filter--prop-menu")
      .findAll("button")
      .find((btn) => btn.text().includes("Has Value"));
    expect(hasValueButton?.exists()).toBe(true);

    await hasValueButton!.trigger("click");
    await flushPromises();

    // Verify that the params watcher was triggered and filter is set to empty string
    expect(watchTracker).toBeCalledTimes(1);
    expect(list.$filter.name).toBe("");
  });

  test("supports multiselect filtering for DateOnly primary keys", async () => {
    // DateOnly primary keys support multiselect (comma-separated values) filtering

    const items = [
      new DateOnlyPk({ dateOnlyPkId: new Date(2024, 0, 15), name: "Item 1" }),
      new DateOnlyPk({ dateOnlyPkId: new Date(2024, 1, 20), name: "Item 2" }),
      new DateOnlyPk({ dateOnlyPkId: new Date(2024, 2, 25), name: "Item 3" }),
    ];

    // Mock the DateOnlyPk list endpoint
    mockEndpoint("/DateOnlyPk/list", () => ({
      wasSuccessful: true,
      list: items,
    }));

    const list = new DateOnlyPkListViewModel();
    const watchTracker = vitest.fn();
    watch(() => list.$params, watchTracker, { deep: true });

    // Mount the component
    const wrapper = mountApp(() => (
      <CListFilters list={list} columnSelection></CListFilters>
    ));
    await flushPromises();
    expect(watchTracker).toBeCalledTimes(0);

    // Open the filter for the Date Only Pk Id property
    await openPropertyFilter(wrapper, "Date Only Pk Id");

    // Verify that the filter menu shows a c-select component (for multiselect)
    const filterMenu = getWrapper(".c-list-filter--prop-menu");
    expect(filterMenu.exists()).toBe(true);

    // Find the c-select component within the filter
    const selectComponent = filterMenu.find(".c-select");
    expect(selectComponent.exists()).toBe(true);

    // Actually select multiple date values through the UI by opening the c-select menu
    const selectInput = selectComponent.find("input");
    await selectInput.trigger("focus");
    await selectInput.trigger("click");

    // Find all overlays and get the last one (which should be the c-select menu)
    const overlays = getWrapper("body").findAll(".v-overlay__content");
    const selectMenu = overlays.at(-1)!;

    // The menu should show the available DateOnlyPk items by their display name
    expect(selectMenu.text()).toContain("Item 1");
    expect(selectMenu.text()).toContain("Item 2");
    expect(selectMenu.text()).toContain("Item 3");

    // Click on the list items to select them
    const listItems = selectMenu.findAll(".v-list-item");

    // Select Item 1
    await listItems.find((i) => i.text().includes("Item 1"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("2024-01-15");

    // Select Item 2
    await listItems.find((i) => i.text().includes("Item 2"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("2024-01-15,2024-02-20");

    // Select Item 3
    await listItems.find((i) => i.text().includes("Item 3"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("2024-01-15,2024-02-20,2024-03-25");

    // Deselect Item 2 (middle item)
    await listItems.find((i) => i.text().includes("Item 2"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("2024-01-15,2024-03-25");

    // Deselect Item 1 (first item)
    await listItems.find((i) => i.text().includes("Item 1"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("2024-03-25");

    // Deselect Item 3 (last remaining item)
    await listItems.find((i) => i.text().includes("Item 3"))!.trigger("click");
    expect(list.$filter.dateOnlyPkId).toBe("");
  });
});
