import { mountApp, mockEndpoint, flushPromises } from "@test/util";
import CLS from "./c-loader-status.vue";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { AxiosError } from "axios";

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

  const endpoints = [
    {
      name: "void-returning",
      methodName: "methodWithOptionalEnumParam" as const,
      mockSuccessResponse: () => ({
        wasSuccessful: true,
        // Void endpoints don't have an 'object' property in the response
      }),
    },
    {
      name: "string-returning",
      methodName: "methodWithOptionalParams" as const,
      mockSuccessResponse: () => ({
        wasSuccessful: true,
        object: "test string result",
      }),
    },
    {
      name: "object-returning",
      methodName: "methodWithManyParams" as const,
      mockSuccessResponse: () => ({
        wasSuccessful: true,
        object: { externalParentId: 42, name: "Test Parent" },
      }),
    },
    {
      name: "ListResult-returning",
      methodName: "returnsListResult" as const,
      mockSuccessResponse: () => ({
        wasSuccessful: true,
        list: [],
      }),
    },
  ] as const;

  describe.each(endpoints)(
    "$name endpoint",
    function ({ methodName, mockSuccessResponse }) {
      test("no-initial-content shows content after success then Network Error", async () => {
        // This tests that hasResult is properly set when an endpoint succeeds,
        // regardless of the return type (void, primitive, object, ItemResult).
        const vm = new ComplexModelViewModel();
        vm.$primaryKey = 1;
        const caller = vm[methodName];

        const mockSuccess = mockEndpoint(
          vm.$metadata.methods[methodName],
          mockSuccessResponse,
        );

        const wrapper = mountApp(() => (
          <CLS loaders={caller} no-initial-content>
            <div>Content</div>
          </CLS>
        ));

        // Initially, content should be hidden (wasSuccessful is null)
        expect(wrapper.text()).not.toContain("Content");

        // Call the endpoint successfully
        await caller();
        await flushPromises();

        // After success, content should be visible
        expect(caller.wasSuccessful).toBe(true);
        expect(caller.hasResult).toBe(true);
        expect(wrapper.text()).toContain("Content");

        mockSuccess.destroy();

        // Now mock a failure
        const mockFailure = mockEndpoint(
          vm.$metadata.methods[methodName],
          () => {
            throw new AxiosError("Network Error");
          },
        );

        // Call the endpoint again, this time it fails
        await expect(caller()).rejects.toThrow();
        await flushPromises();

        // Content should still be visible because Network errors do not wipe a caller's result.
        // This is tested for because its how Coalesce has always worked,
        // and would cause all kinds of subtile behavior changes if it ever stopped doing that.
        expect(caller.wasSuccessful).toBe(false);
        expect(caller.hasResult).toBe(true);
        expect(wrapper.text()).toContain("Content");

        mockFailure.destroy();
      });

      test("no-initial-content hides content after explicit failure", async () => {
        // This tests that hasResult is properly set when an endpoint succeeds,
        // regardless of the return type (void, primitive, object, ItemResult).
        const vm = new ComplexModelViewModel();
        vm.$primaryKey = 1;
        const caller = vm[methodName];

        const mockSuccess = mockEndpoint(
          vm.$metadata.methods[methodName],
          mockSuccessResponse,
        );

        const wrapper = mountApp(() => (
          <CLS loaders={caller} no-initial-content>
            <div>Content</div>
          </CLS>
        ));

        // Initially, content should be hidden (wasSuccessful is null)
        expect(wrapper.text()).not.toContain("Content");

        // Call the endpoint successfully
        await caller();
        await flushPromises();

        // After success, content should be visible
        expect(caller.wasSuccessful).toBe(true);
        expect(caller.hasResult).toBe(true);
        expect(wrapper.text()).toContain("Content");

        mockSuccess.destroy();

        // Now mock a failure
        const mockFailure = mockEndpoint(
          vm.$metadata.methods[methodName],
          () => ({
            wasSuccessful: false,
            message: "Explicit failure",
          }),
        );

        // Call the endpoint again, this time it fails
        await expect(caller()).rejects.toThrow();
        await flushPromises();

        // Since no-initial-status is based on `hasResult`,
        // and our result has been wiped by an explicit server error, the content should hide.
        expect(caller.wasSuccessful).toBe(false);
        expect(caller.hasResult).toBe(false);
        expect(caller.result).toBeFalsy();
        expect(wrapper.text()).not.toContain("Content");

        mockFailure.destroy();
      });
    },
  );

  test("aria-label is applied to progress bar", async () => {
    const vm = new ComplexModelViewModel();
    vm.$primaryKey = 1;

    const wrapper = mountApp(() => (
      <CLS loaders={vm.$load} aria-label="Loading progress">
        <div>Content</div>
      </CLS>
    ));

    // Manually set the loader to a loading state to trigger the progress bar
    vm.$load.isLoading = true;
    
    // Wait for next tick so the loading state is reflected in the DOM
    await wrapper.vm.$nextTick();

    // Find the progress bar
    const progressBar = wrapper.find(".c-loader-status--progress");

    // Verify the aria-label is present on the progress bar
    expect(progressBar.exists()).toBe(true);
    expect(progressBar.attributes("aria-label")).toBe("Loading progress");
  });
});
