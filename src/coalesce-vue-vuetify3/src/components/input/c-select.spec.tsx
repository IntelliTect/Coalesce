import { Course, Student } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import {
  getWrapper,
  mountApp,
  nextTick,
  flushPromises,
  BetterComponentInstance,
} from "@test/util";
import { VueWrapper } from "@vue/test-utils";
import { AxiosRequestConfig } from "axios";
import {
  AnyArgCaller,
  AxiosClient,
  AxiosItemResult,
  AxiosListResult,
  Model,
  ViewModel,
} from "coalesce-vue";
import { Mock } from "vitest";
import { FunctionalComponent, ref } from "vue";
import { VForm } from "vuetify/components";
import { CSelect } from "..";

import { ComplexModel, Case } from "@test/models.g";
import { ComplexModelViewModel, CaseViewModel } from "@test/viewmodels.g";
import { h } from "vue";

describe("CSelect", () => {
  let model = new Student({
    name: "bob",
  });
  let axiosMock: Mock;
  beforeEach(() => {
    model = new Student({
      name: "bob",
    });

    axiosMock = AxiosClient.defaults.adapter = vitest
      .fn()
      .mockImplementation(async (config: AxiosRequestConfig) => {
        if (config.url == "/Courses/list") {
          const items = [
            new Course({ courseId: 101, name: "foo 101" }),
            new Course({ courseId: 202, name: "bar 202" }),
          ].filter(
            (c, i) =>
              (!config.params.search ||
                c.name?.startsWith(config.params.search)) &&
              (!config.params.pageSize || i < config.params.pageSize)
          );
          return {
            data: {
              wasSuccessful: true,
              list: items,
              page: 1,
              pageCount: 1,
              pageSize: 10,
              totalCount: items.length,
            },
            status: 200,
          } as AxiosListResult<Course>;
        }
        if (config.url == "/Courses/get/303")
          return {
            data: {
              wasSuccessful: true,
              object: new Course({ courseId: 303, name: "baz 303" }),
            },
            status: 200,
          } as AxiosItemResult<Course>;
      });
  });

  test("types", () => {
    const model = new ComplexModel({
      name: "bob",
    });
    const vm = new ComplexModelViewModel();
    const selectedAny = ref<any>();

    const anyString: string = "foo";
    const genericModel: Model = model;

    // Binding to FK or ref nav on a ViewModel:
    () => <CSelect model={vm} for="singleTest" />;
    () => <CSelect model={vm} for="singleTestId" />;
    () => <CSelect model={vm} for={vm.$metadata.props.singleTest} />;

    // Binding to plain Models:
    () => <CSelect model={model} for="singleTest" />;
    () => <CSelect model={model} for="singleTestId" />;
    //@ts-expect-error wrong type of property
    () => <CSelect model={model} for="name" />;
    //@ts-expect-error wrong type of property
    () => <CSelect model={vm} for={vm.$metadata.props.name} />;

    // Untyped bindings:
    () => <CSelect model={genericModel} for={anyString} />;
    () => <CSelect model={model as any} for={anyString} />;

    // Binding with for + v-model
    () => (
      <CSelect
        for="Test"
        modelValue={selectedAny}
        update:modelValue={() => {}}
      />
    );
    () => (
      <CSelect
        for={vm.$metadata.props.singleTest}
        modelValue={selectedAny}
        update:modelValue={() => {}}
      />
    );

    // Binding to method parameters
    () => <CSelect model={vm.methodWithManyParams} for="model" />;
    () => (
      <CSelect
        model={vm.methodWithManyParams}
        for={vm.$metadata.methods.methodWithManyParams.params.model}
      />
    );
    //@ts-expect-error non-object method parameter
    () => <CSelect model={vm.methodWithManyParams} for="integer" />;

    () => (
      <CSelect model={vm.methodWithManyParams as AnyArgCaller} for="model" />
    );
    // This has to be valid when we don't have a known type for the caller
    () => (
      <CSelect
        model={vm.methodWithManyParams as AnyArgCaller}
        for="specificString"
      />
    );

    //@ts-expect-error invalid model type
    () => <CSelect model={123} for="num" />;
    //@ts-expect-error invalid for type
    () => <CSelect model={vm} for={123} />;
  });

  test.each([
    {
      props: { model, for: "currentCourse" },
      label: "Current Course",
      hint: "The course that the student is currently attending.",
    },
    {
      props: { model, for: "currentCourseId" },
      label: "Current Course",
      hint: "The course that the student is currently attending.",
    },
    {
      props: { model, for: model.$metadata.props.currentCourse },
      label: "Current Course",
      hint: "The course that the student is currently attending.",
    },
    {
      props: { model, for: model.$metadata.props.currentCourseId },
      label: "Current Course",
      hint: "The course that the student is currently attending.",
    },
    { props: { for: "Course" }, label: "Course", hint: "" },
  ])("metadata resolves - $props.for", async ({ props, label, hint }) => {
    // Arrange/Act
    const wrapper = mountApp(CSelect, { props }).findComponent(CSelect);

    // Assert: resting state of the component
    expect(wrapper.find("label").text()).toEqual(label);
    expect(wrapper.find(".v-messages").text()).toEqual(hint);
    // Only automatically clearable when bound by a non-required navigation/fk, which is none of these test cases
    expect(wrapper.find(".v-field__clearable").exists()).toBeFalsy();
    await nextTick();

    // Act/Assert: Open menu and ensure options from server are listed
    const overlay = await openMenu(wrapper);
    expect(overlay.text()).toContain("foo 101");
    expect(overlay.text()).toContain("bar 202");
  });

  describe("user-provided props override defaults", () => {
    test("label", async () => {
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          label="custom label"
          hint="custom hint"
          persistent-hint
          clearable
        ></CSelect>
      ));

      expect(wrapper.find("label").text()).toEqual("custom label");
      expect(wrapper.find(".v-messages").text()).toEqual("custom hint");
      expect(wrapper.find(".v-field__clearable").exists()).toBeTruthy();
    });
  });

  describe("validation", () => {
    async function assertValidation(
      TargetComponent: FunctionalComponent,
      message: string
    ) {
      const wrapper = mountApp(() => (
        <VForm>
          <TargetComponent></TargetComponent>
        </VForm>
      ));

      await wrapper.findComponent(VForm).vm.validate();

      expect(wrapper.find(".v-input--error").exists()).toBeTruthy();
      expect(wrapper.find(".v-messages").text()).toEqual(message);
    }

    describe.each([
      { p: "currentCourse", d: "navigation" },
      { p: "currentCourseId", d: "fk" },
    ] as const)("when bound by $d", ({ p: propName }) => {
      test("pulls validation from fk metadata", async () => {
        await assertValidation(
          () => <CSelect model={model} for={propName}></CSelect>,
          "Current Course is required."
        );
      });

      test("pulls validation from ViewModel instance", async () => {
        const model = new StudentViewModel();
        model.$addRule(
          "currentCourseId",
          "required",
          (v: any) => !!v || "Custom rule from VM."
        );
        await assertValidation(
          () => <CSelect model={model} for={propName}></CSelect>,
          "Custom rule from VM."
        );
      });

      test("overrides via rules prop", async () => {
        await assertValidation(
          () => (
            <CSelect
              model={model}
              for={propName}
              rules={[(v: any) => !!v || "Custom rule"]}
            ></CSelect>
          ),
          "Custom rule"
        );
      });
    });
  });

  describe("value binding", () => {
    test("keyValue fetches object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect for="Course" keyValue={303}></CSelect>
      ));
      await flushPromises();

      // Assert - the name of the selected option should be fetched from the server
      // and show as the selected item.
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(axiosMock).toHaveBeenCalledTimes(2);
    });

    test("model + FK-only fetches object from server", async () => {
      // Arrange/Act
      const student = new Student({ currentCourseId: 303 });
      const wrapper = mountApp(() => (
        <CSelect model={student} for="currentCourse"></CSelect>
      ));
      await flushPromises();

      // Assert - the name of the selected option should be fetched from the server
      // and show as the selected item.
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(axiosMock).toHaveBeenCalledTimes(2);
    });

    test("objectValue does not fetch object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          objectValue={new Course({ courseId: 303, name: "baz 303" })}
        ></CSelect>
      ));
      await flushPromises();

      // Assert
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(axiosMock).toHaveBeenCalledTimes(1);
    });

    test("modelValue does not fetch object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          modelValue={new Course({ courseId: 303, name: "baz 303" })}
        ></CSelect>
      ));
      await flushPromises();

      // Assert
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(axiosMock).toHaveBeenCalledTimes(1);
    });

    test("emits updates on selection", async () => {
      // Arrange
      const onUpdateKey = vitest.fn();
      const onUpdateObject = vitest.fn();
      const onUpdateModel = vitest.fn();
      const model = new Course({ courseId: 303 });

      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          onUpdate:keyValue={onUpdateKey}
          onUpdate:objectValue={onUpdateObject}
          onUpdate:modelValue={onUpdateModel}
        ></CSelect>
      )).findComponent(CSelect<Course>);

      // Act
      await selectFirstResult(wrapper);

      // Assert: Emits events
      expect(onUpdateKey).toHaveBeenCalledWith(101);
      expect(onUpdateObject).toHaveBeenCalledWith(
        new Course({ courseId: 101, name: "foo 101" })
      );
      expect(onUpdateModel).toHaveBeenCalledWith(
        new Course({ courseId: 101, name: "foo 101" })
      );

      // Assert: `model` prop not mutated because we're bound by type, not prop
      expect(model.courseId).toBe(303);

      // Assert: Menu closes after selection
      expect(wrapper.vm.menuOpen).toBe(false);
    });

    test("mutates model on selection when bound by model", async () => {
      // Arrange
      const model = new Student({ currentCourseId: 303 });
      const wrapper = mountApp(() => (
        <CSelect model={model} for="currentCourse"></CSelect>
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert: `model` prop mutated
      expect(model.currentCourseId).toBe(101);
      expect(model.currentCourse?.courseId).toBe(101);
    });

    test("mutates model on selection when bound by apicaller arg", async () => {
      const model = new StudentViewModel({});
      const wrapper = mountApp(() => (
        <CSelect model={model.manyParams} for="model" />
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert: arg prop mutated
      expect(model.manyParams.args.model?.courseId).toBe(101);
    });
  });

  describe("interaction", () => {
    test("typing while focused opens search", async () => {
      const wrapper = mountApp(() => (
        <CSelect model={model} for="currentCourse"></CSelect>
      )).findComponent(CSelect<any>);

      await flushPromises();

      // Find the main input, focus it (as if it was tabbed to by the user)
      const mainInput = wrapper.find("input");
      mainInput.element.focus();

      // Type a character
      await mainInput.setValue("f");
      expect(wrapper.vm.search).toBe("f");
      expect(wrapper.vm.menuOpen).toBe(true);

      // Input should have been transferred to the dropdown's search field
      expect(mainInput.element.value).toBeFalsy();
      const menuWrapper = menuContents();
      const menuInput = menuWrapper.find("input");
      expect(menuInput.element.value).toBe("f");

      // Focus should be transferred to the dropdown's search field
      expect(menuInput.element).toBe(document.activeElement);

      // Only one item matches our search criteria (foo 101, not bar 202)
      await flushPromises();
      expect(menuWrapper.findAll(".v-list-item").length).toBe(1);
    });

    test("list is keyboard navigable", async () => {
      const wrapper = mountApp(() => (
        <CSelect model={model} for="currentCourse" clearable></CSelect>
      )).findComponent(CSelect<Student>);

      const mainInput = wrapper.find("input");

      // Open the menu
      mainInput.trigger("keydown.enter");
      expect(wrapper.vm.menuOpen).toBeTruthy();

      await flushPromises();
      const menuWrapper = menuContents();
      const menuInput = menuWrapper.find("input");

      // Close it with escape
      await mainInput.trigger("keydown.esc");
      expect(wrapper.vm.menuOpen).toBeFalsy();

      // Open it again
      await mainInput.trigger("keydown.space");
      expect(wrapper.vm.menuOpen).toBeTruthy();

      // Navigate to the second item
      await menuInput.trigger("keydown.down");
      // Pick it
      await menuInput.trigger("keydown.enter");

      expect(menuWrapper.find(".v-list-item--active").text()).toBe("bar 202");
      expect(model.currentCourseId).toBe(202); // second result in the list
      expect(wrapper.vm.menuOpen).toBeFalsy();

      // Clear the selection with delete
      await mainInput.trigger("keydown.delete");
      expect(model.currentCourseId).toBeNull();
    });

    describe.each(["disabled", "readonly"])("%s", (prop) => {
      async function assertNonInteractive(
        wrapper: VueWrapper<BetterComponentInstance<typeof CSelect<Student>>>
      ) {
        // Clearable is ignored when disabled/readonly
        expect(wrapper.find(".v-field__clearable").exists()).toBeFalsy();
        // Main input that grabs search query when focused is disabled/readonly
        expect(wrapper.find("input").attributes()[prop]).not.toBeUndefined();

        // Clicking the component doesn't open the menu
        const menuContents = await openMenu(wrapper);
        expect(menuContents.exists()).toBeFalsy();
        expect(wrapper.vm.menuOpen).toBeFalsy();

        // Can't open with keyboard inputs
        await wrapper.find("input").trigger("keydown.enter");
        expect(wrapper.vm.menuOpen).toBeFalsy();
      }

      test("noninteractive via VForm", async () => {
        model.currentCourse = new Course({ courseId: 101 });
        const wrapper = mountApp(() => (
          <VForm {...{ [prop]: true }}>
            <CSelect model={model} for="currentCourse" clearable></CSelect>
          </VForm>
        )).findComponent(CSelect<Student>);

        assertNonInteractive(wrapper);
      });
      test("noninteractive via direct", () => {
        model.currentCourse = new Course({ courseId: 101 });
        const wrapper = mountApp(() => (
          <CSelect
            model={model}
            for="currentCourse"
            clearable
            {...{ [prop]: true }}
          ></CSelect>
        )).findComponent(CSelect<Student>);

        assertNonInteractive(wrapper);
      });
    });
  });

  test("preselect first", async () => {
    mountApp(() => (
      <CSelect model={model} for="currentCourse" preselect-first></CSelect>
    )).findComponent(CSelect);
    await flushPromises();
    expect(model.currentCourseId).toBe(101);
  });

  test.each([
    { results: 1, expected: 101 },
    { results: 2, expected: null },
  ])(
    "preselect single with $results results",
    async ({ results, expected }) => {
      mountApp(() => (
        <CSelect
          model={model}
          for="currentCourse"
          preselect-single
          params={{ pageSize: results }}
        ></CSelect>
      )).findComponent(CSelect);
      await flushPromises();
      expect(model.currentCourseId).toBe(expected);
    }
  );

  test("create", async () => {
    const wrapper = mountApp(() => (
      <CSelect
        model={model}
        for="currentCourse"
        create={{
          getLabel(search: string) {
            if (search == "f") return "new thing";
            else return false;
          },
          async getItem(search: string, label: string) {
            return new Course({ name: label });
          },
        }}
      ></CSelect>
    )).findComponent(CSelect);
    await flushPromises();

    // our label function is predicated on a search of "f",
    // so no create item option expected yet.
    const menuWrapper = await openMenu(wrapper);
    expect(menuWrapper.text()).not.toContain("new thing");

    // Set our search value to fulfil getLabel's predicate
    await menuWrapper.find("input").setValue("f");

    // Select the new item entry.
    const createItem = menuWrapper.find(".c-select__create-item");
    expect(createItem.text()).toContain("new thing");
    await createItem.trigger("click");

    // Assert
    expect(model.currentCourse?.name).toBe("new thing");
  });

  // test("BOILERPLATE", () => {
  //   const appWrapper = mountApp(CSelect, {});
  //   const wrapper = appWrapper.findComponent(CSelect);
  // });
});

const menuContents = () => getWrapper(".c-select__menu-content");

const openMenu = async (wrapper: VueWrapper) => {
  await flushPromises();
  await wrapper.find(".v-input__control").trigger("click");
  await flushPromises();
  return menuContents();
};

const selectFirstResult = async (wrapper: VueWrapper) => {
  const overlay = await openMenu(wrapper);
  await overlay.find(".v-list-item").trigger("click");
};
