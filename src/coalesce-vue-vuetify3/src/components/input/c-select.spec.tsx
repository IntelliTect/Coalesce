import { Course, Grade, Student } from "@test/targets.models";
import { delay, getWrapper, mountApp, nextTick, nextTicks } from "@test/util";
import { mount, VueWrapper } from "@vue/test-utils";
import { AxiosRequestConfig } from "axios";
import { AxiosClient, AxiosItemResult, AxiosListResult } from "coalesce-vue";
import { Mock } from "vitest";
import { CSelect } from "..";

describe("CSelect", () => {
  var model = new Student({
    name: "bob",
  });

  const menuContents = () => getWrapper(".c-select__menu-content");

  const openMenu = async (wrapper: VueWrapper) => {
    await delay(1);
    await wrapper.find(".v-input__control").trigger("click");
    await delay(1);
    return menuContents();
  };

  const selectFirstResult = async (wrapper: VueWrapper) => {
    const overlay = await openMenu(wrapper);
    await overlay.find(".v-list-item").trigger("click");
  };

  let axiosMock: Mock;
  beforeEach(() => {
    axiosMock = AxiosClient.defaults.adapter = vitest
      .fn()
      .mockImplementation(async (config: AxiosRequestConfig) => {
        if (config.url == "/Courses/list")
          return {
            data: {
              wasSuccessful: true,
              list: [
                new Course({ courseId: 101, name: "foo 101" }),
                new Course({ courseId: 202, name: "bar 201" }),
              ],
              page: 1,
              pageCount: 1,
              pageSize: 10,
              totalCount: 2,
            },
            status: 200,
          } as AxiosListResult<Course>;

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

  test.each([
    { props: { model, for: "currentCourse" }, label: "Current Course" },
    { props: { model, for: "currentCourseId" }, label: "Current Course" },
    {
      props: { model, for: model.$metadata.props.currentCourse },
      label: "Current Course",
    },
    {
      props: { model, for: model.$metadata.props.currentCourseId },
      label: "Current Course",
    },
    { props: { for: "Course" }, label: "Course" },
  ])("metadata resolves - $props.for", async ({ props, label }) => {
    const wrapper = mountApp(CSelect, { props }).findComponent(CSelect);

    expect(wrapper.find("label").text()).toEqual(label);
    await nextTick();

    const overlay = await openMenu(wrapper);
    expect(overlay.text()).toContain("foo 101");
    expect(overlay.text()).toContain("bar 201");
  });

  describe("value binding", () => {
    test("keyValue fetches object from server", async () => {
      const wrapper = mountApp(() => (
        <CSelect for="Course" keyValue={303}></CSelect>
      ));
      await delay(1);
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(axiosMock).toHaveBeenCalledTimes(2);
    });

    test("model + FK-only fetches object from server", async () => {
      const student = new Student({ currentCourseId: 303 });
      const wrapper = mountApp(() => (
        <CSelect model={student} for="currentCourse"></CSelect>
      ));
      await delay(1);
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(axiosMock).toHaveBeenCalledTimes(2);
    });

    test("objectValue does not fetch object from server", async () => {
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          objectValue={new Course({ courseId: 303, name: "baz 303" })}
        ></CSelect>
      ));
      await delay(1);
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(axiosMock).toHaveBeenCalledTimes(1);
    });

    test("modelValue does not fetch object from server", async () => {
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          modelValue={new Course({ courseId: 303, name: "baz 303" })}
        ></CSelect>
      ));
      await delay(1);
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(axiosMock).toHaveBeenCalledTimes(1);
    });

    test("emits updates on selection", async () => {
      const onUpdateKey = vitest.fn();
      const onUpdateObject = vitest.fn();
      const onUpdateModel = vitest.fn();
      const model = new Course({ courseId: 303 });
      const wrapper = mountApp(() => (
        <CSelect
          for="Course"
          model={model}
          onUpdate:keyValue={onUpdateKey}
          onUpdate:objectValue={onUpdateObject}
          onUpdate:modelValue={onUpdateModel}
        ></CSelect>
      )).findComponent(CSelect);

      await selectFirstResult(wrapper);

      // Emits events
      expect(onUpdateKey).toHaveBeenCalledWith(101);
      expect(onUpdateObject).toHaveBeenCalledWith(
        new Course({ courseId: 101, name: "foo 101" })
      );
      expect(onUpdateModel).toHaveBeenCalledWith(
        new Course({ courseId: 101, name: "foo 101" })
      );

      // `model` prop not mutated because we're bound by type, not prop
      expect(model.courseId).toBe(303);

      // Menu closes after selection
      expect(wrapper.vm.menuOpen).toBe(false);
    });

    test("mutates model on selection when bound by model", async () => {
      const model = new Student({ currentCourseId: 303 });
      const wrapper = mountApp(() => (
        <CSelect model={model} for="currentCourse"></CSelect>
      )).findComponent(CSelect);

      await selectFirstResult(wrapper);

      // `model` prop mutated
      expect(model.currentCourseId).toBe(101);
      expect(model.currentCourse?.courseId).toBe(101);
    });
  });

  // test("BOILERPLATE", () => {
  //   const appWrapper = mountApp(CSelect, {});
  //   const wrapper = appWrapper.findComponent(CSelect);
  // });
});
