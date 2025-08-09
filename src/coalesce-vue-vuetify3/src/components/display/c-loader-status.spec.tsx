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

  test("show-success prop defaults to false", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} />
    )).findComponent(CLS);

    expect(wrapper.props("showSuccess")).toBe(false);
  });

  test("show-success prop can be set to true", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} show-success />
    )).findComponent(CLS);

    expect(wrapper.props("showSuccess")).toBe(true);
  });

  test("success messages are empty when show-success is false", () => {
    // Mock a successful loader with a message
    const mockLoader = {
      wasSuccessful: true,
      message: "Operation completed successfully",
      isLoading: false,
      hasResult: true
    };

    const wrapper = mountApp(() => (
      <CLS loaders={[mockLoader as any]} show-success={false} />
    )).findComponent(CLS);

    expect(wrapper.vm.successMessages).toEqual([]);
  });

  test("success messages are shown when show-success is true and loader is successful", () => {
    // Mock a successful loader with a message
    const mockLoader = {
      wasSuccessful: true,
      message: "Operation completed successfully",
      isLoading: false,
      hasResult: true
    };

    const wrapper = mountApp(() => (
      <CLS loaders={[mockLoader as any]} show-success />
    )).findComponent(CLS);

    expect(wrapper.vm.successMessages).toEqual(["Operation completed successfully"]);
  });

  test("success messages are empty when loader is successful but has no message", () => {
    // Mock a successful loader without a message
    const mockLoader = {
      wasSuccessful: true,
      message: null,
      isLoading: false,
      hasResult: true
    };

    const wrapper = mountApp(() => (
      <CLS loaders={[mockLoader as any]} show-success />
    )).findComponent(CLS);

    expect(wrapper.vm.successMessages).toEqual([]);
  });
});
