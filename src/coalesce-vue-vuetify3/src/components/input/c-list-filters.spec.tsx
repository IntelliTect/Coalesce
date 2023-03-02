import { StudentListViewModel } from "@test/targets.viewmodels";
import { flushPromises, mountApp } from "@test/util";
import { watch } from "vue";
import { CListFilters } from "..";

describe("CListFilters", () => {
  test("doesn't mutate list.$params without user interaction", async () => {
    // There was a bug where c-list-filters was initializing list.$params.filters
    // (if it wasn't set) on mount, which was then incorrectly triggering list autoload.
    const list = new StudentListViewModel();

    const watchTracker = vitest.fn();
    watch(() => list.$params, watchTracker, { deep: true });

    // Mount the component without any user action. list.$params should be unchanged.
    const wrapper = mountApp(() => <CListFilters list={list}></CListFilters>);
    await flushPromises();
    expect(watchTracker).toBeCalledTimes(0);

    // Now change a filter and observe that the params watcher triggers.
    const filters = wrapper.findComponent(CListFilters);
    filters.vm.filters.find((f) => f.key == "name")!.value = "asd";
    await flushPromises();
    expect(watchTracker).toBeCalledTimes(1);
  });
});
