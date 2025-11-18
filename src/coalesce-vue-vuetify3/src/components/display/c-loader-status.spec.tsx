import { mountApp, mockEndpoint, flushPromises } from "@test/util";
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
    const wrapper = mountApp(() => <CLS loaders={vm.$load} />).findComponent(
      CLS,
    );

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(false);
  });

  test("show-success flag can be enabled", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} show-success />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(true);
  });

  test("show-success in flags string", () => {
    const wrapper = mountApp(() => (
      <CLS loaders={{ "show-success": [vm.$load] }} />
    )).findComponent(CLS);

    expect(wrapper.vm.loaderFlags[0][1]["show-success"]).toBe(true);
  });

  test("success messages computation", () => {
    // Set up the loader in a successful state with a custom message
    vm.$load.wasSuccessful = true;
    vm.$load.message = "Test success message";

    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} show-success />
    )).findComponent(CLS);

    // Verify the success message is displayed in the rendered text
    expect(wrapper.text()).toContain("Test success message");
  });

  test("success messages with null message shows default", () => {
    // Set up the loader in a successful state with null message
    vm.$save.wasSuccessful = true;
    vm.$save.message = null;

    const wrapper = mountApp(() => (
      <CLS loaders={vm.$save} show-success />
    )).findComponent(CLS);

    // Verify the default "Success" message is displayed
    expect(wrapper.text()).toContain("Success");
  });

  test("void-returning endpoint with no-initial-content shows content after success then failure", async () => {
    // This tests the fix for void-returning endpoints where hasResult should be true
    // when the endpoint succeeds, even though result is null.
    const vm = new ComplexModelViewModel();
    vm.$primaryKey = 1;
    const voidCaller = vm.methodWithOptionalEnumParam;

    const mockSuccess = mockEndpoint(
      vm.$metadata.methods.methodWithOptionalEnumParam,
      () => ({
        wasSuccessful: true,
        // Void endpoints don't have an 'object' property in the response
      }),
    );

    const wrapper = mountApp(() => (
      <CLS loaders={voidCaller} no-initial-content>
        <div>Content</div>
      </CLS>
    ));

    // Initially, content should be hidden (wasSuccessful is null)
    expect(wrapper.text()).not.toContain("Content");

    // Call the void endpoint successfully
    await voidCaller();
    await flushPromises();

    // After success, content should be visible
    expect(voidCaller.wasSuccessful).toBe(true);
    expect(voidCaller.result).toBeNull();
    expect(voidCaller._hasLoaded.value).toBe(true);
    expect(wrapper.text()).toContain("Content");

    mockSuccess.destroy();

    // Now mock a failure
    const mockFailure = mockEndpoint(
      vm.$metadata.methods.methodWithOptionalEnumParam,
      () => ({
        wasSuccessful: false,
        message: "Error occurred",
      }),
    );

    // Call the endpoint again, this time it fails
    await expect(voidCaller()).rejects.toThrow();
    await flushPromises();

    // Content should still be visible because hasLoaded is true
    // (the endpoint has successfully loaded before, even though this call failed)
    expect(voidCaller.wasSuccessful).toBe(false);
    expect(voidCaller._hasLoaded.value).toBe(true);
    expect(wrapper.text()).toContain("Content");

    mockFailure.destroy();
  });
});
