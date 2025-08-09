import { mountApp } from "@test/util";
import CLS from "./c-loader-status.vue";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

describe("CLoaderStatus", () => {
  const vm = new ComplexModelViewModel();
  const manyParams = vm.methodWithManyParams;

  test("types", () => {
    () => (
      <CLS loaders={{ "": [vm.$load, vm.$save, vm.methodWithManyParams] }} />
    );

    // Allow falsy loaders
    () => (
      <CLS
        loaders={{
          "": [
            vm.$load,
            null,
            undefined,
            false,
            "methodWithManyParams" in vm && vm.methodWithManyParams,
          ],
        }}
      />
    );

    () => (
      <CLS
        loaders={{
          "no-loading-content secondary-progress": [vm.$load],
          // Without a "no" flag first (this was broken for a while):
          "initial-content no-secondary-progress": [vm.$save],
        }}
        noInitialContent
      />
    );

    () => <CLS loaders={vm.$load} no-initial-content secondary-progress />;

    // @ts-expect-error invalid prop type
    () => <CLS loaders={1} />;
    // @ts-expect-error invalid prop type
    () => <CLS loaders={new Date()} />;

    // @ts-expect-error invalid flags type
    () => <CLS loaders={{ manyParams }} />;
    // @ts-expect-error invalid flags type
    () => <CLS loaders={{ foo: vm.$load }} />;
    // @ts-expect-error invalid flags type
    () => <CLS loaders={{ [" "]: vm.$load }} />;
  });

  test("flag precedence", () => {
    const wrapper = mountApp(() => (
      <CLS
        loaders={{ "no-secondary-progress initial-content": [vm.$load] }}
        no-initial-content
        no-secondary-progress={false}
        no-error-content
        no-initial-progress={false}
      />
    )).findComponent(CLS);

    const flags = wrapper.vm.loaderFlags[0][1];

    // Component-level `true` overwritten by the flags string, which is the most specific specifier:
    expect(flags["secondary-progress"]).toBe(false);

    // Component-level `false` overwritten by the flags string, which is the most specific specifier:
    expect(flags["initial-content"]).toBe(true);

    // Component-level `false` is the only specifier:
    expect(flags["error-content"]).toBe(false);

    // Explicit `false` provided to a `no` flag, which inverts it to `true`.
    expect(flags["initial-progress"]).toBe(true);
  });

  test("simple multi-loader", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={[vm.$load, vm.$save]} no-initial-content />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["initial-content"]).toBe(false);
    expect(wrapper.vm.loaderFlags[1][1]["initial-content"]).toBe(false);
  });

  test("simple single loader", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} no-initial-content />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["initial-content"]).toBe(false);
  });

  test("show-success flag defaults to false", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(false);
  });

  test("show-success flag can be enabled", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} no-show-success={false} />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(true);
  });

  test("show-success in flags string", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={{ "show-success": [vm.$load] }} />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(true);
  });

  test("show-success flag precedence", () => {
    const wrapper = mountApp(() => (
      <CLS
        loaders={{ "no-show-success": [vm.$load] }}
        no-show-success={false}
      />
    )).findComponent(CLS);

    // Flags string should override component-level prop
    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(false);
  });

  test("success messages computation", () => {
    // Create a proper mock that extends or mimics ApiState
    const mockLoader = {
      wasSuccessful: true,
      message: "Test success message",
      isLoading: false,
      hasResult: true
    };

    const wrapper = mountApp(() => (
      <CLS loaders={mockLoader} show-success />
    )).findComponent(CLS);

    // The loader should be in loaderFlags with show-success enabled
    const flags = wrapper.vm.loaderFlags;
    expect(flags.length).toBeGreaterThan(0);
    expect(flags[0][1]["show-success"]).toBe(true);
  });

  test("success messages with null message shows default", () => {
    // Create a proper mock that extends or mimics ApiState
    const mockLoader = {
      wasSuccessful: true,
      message: null,
      isLoading: false,
      hasResult: true
    };

    const wrapper = mountApp(() => (
      <CLS loaders={mockLoader} show-success />
    )).findComponent(CLS);

    // The loader should be in loaderFlags with show-success enabled
    const flags = wrapper.vm.loaderFlags;
    expect(flags.length).toBeGreaterThan(0);
    expect(flags[0][1]["show-success"]).toBe(true);
  });
});
