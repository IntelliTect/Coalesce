import { Grade, Student } from "@test/targets.models";
import { mount } from "@test/util";
import { CDisplay } from "..";

describe("CDisplay", () => {
  var model = new Student({
    name: "bob",
    grade: Grade.Freshman,
    password: "secretValue",
    email: "bob@college.edu",
    phone: "123-123-1234",
    color: "#ff0000",
    notes: "multiline\n\nstring",
  });

  test(":model", () => {
    const wrapper = mount(CDisplay, {
      props: { model },
    });

    expect(wrapper.text()).toContain("bob");
  });

  test(":model for=propString", () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "grade" },
    });

    expect(wrapper.text()).toContain("Freshman");
  });

  test(":model for=metaObject", () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: model.$metadata.props.grade },
    });

    expect(wrapper.text()).toContain("Freshman");
  });

  test(":model for=qualifiedString", () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "Student.grade" },
    });

    expect(wrapper.text()).toContain("Freshman");
  });

  test(":value date", () => {
    const wrapper = mount(CDisplay, {
      props: { value: new Date(1990, 0, 2, 3, 4, 5) },
    });

    expect(wrapper.text()).toContain("1/2/1990 3:04:05 AM");
  });

  test(":modelValue date", () => {
    const wrapper = mount(CDisplay, {
      props: {
        modelValue: new Date(1990, 0, 2, 3, 4, 5),
        format: "yyyy-MM-dd hh:mm:ss a",
      },
    });

    expect(wrapper.text()).toContain("1990-01-02 03:04:05 AM");
  });

  test("password", async () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "password" },
    });

    expect(wrapper.text()).toContain("••••••••");
    expect(wrapper.text()).not.toContain("secretValue");

    await wrapper.find("[role=button]").trigger("click");

    expect(wrapper.text()).not.toContain("••••••••");
    expect(wrapper.text()).toContain("secretValue");
  });

  test("email", async () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "email" },
    });

    expect(wrapper.text()).toContain(model.email);
    expect(wrapper.find("a[href]").attributes()["href"]).toEqual(
      "mailto:" + model.email
    );
  });

  test("phone", async () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "phone" },
    });

    expect(wrapper.text()).toContain(model.phone);
    expect(wrapper.find("a[href]").attributes()["href"]).toEqual(
      "tel:" + model.phone
    );
  });

  test.each([
    [true, "✓"],
    [false, "✗"],
  ])("boolean %s", async (value, display) => {
    model.isEnrolled = value;
    const wrapper = mount(CDisplay, {
      props: { model, for: "isEnrolled" },
    });

    expect(wrapper.text()).toContain(display);
  });

  test("color", async () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "color" },
    });

    expect(wrapper.text()).toContain(model.color);
    expect(
      wrapper.find(".c-display--color-swatch").attributes()["style"]
    ).toEqual(
      // Note: this assertion subject to nuances of jsdom,
      // which seems to normalize colors into `rgb` format.
      "background-color: rgb(255, 0, 0);"
    );
  });

  test("multiline", async () => {
    const wrapper = mount(CDisplay, {
      props: { model, for: "notes" },
    });

    expect(wrapper.text()).toContain(model.notes);
    expect(wrapper.attributes()["style"]).toEqual("white-space: pre-wrap;");
  });
});
