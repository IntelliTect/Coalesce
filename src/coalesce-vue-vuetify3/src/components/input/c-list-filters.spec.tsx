import { flushPromises, getWrapper, mountApp } from "@test/util";
import { watch } from "vue";
import { CListFilters } from "..";
import { ComplexModelListViewModel } from "@test-targets/viewmodels.g";

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

  async function openNameFilter(wrapper: any) {
    // Open the filters menu by clicking the filter button
    const filterButton = wrapper.find(".c-list-filters");
    await filterButton.trigger("click");
    await flushPromises();

    // Find and click on the filter button for the "Name" property
    const nameFilterButton = getWrapper(".v-overlay__content")
      .findAll(".v-list-item")
      .find((item) => item.text().includes("Name"))!
      .find(".fa-filter")
      .element.closest("button");
    expect(nameFilterButton).toBeTruthy();

    nameFilterButton!.click();
    await flushPromises();
  }
  test("doesn't mutate list.$params without user interaction", async () => {
    // There was a bug where c-list-filters was initializing list.$params.filters
    // (if it wasn't set) on mount, which was then incorrectly triggering list autoload.
    const { list, watchTracker } = setupListAndWatcher();

    // Mount the component without any user action. list.$params should be unchanged.
    const wrapper = mountApp(() => <CListFilters list={list}></CListFilters>);
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
});
