import { mount } from "@test/util";
import { CDisplay } from "..";
import { Case, ComplexModel, Statuses } from "@test-targets/models.g";
import {
  CaseViewModel,
  CompanyViewModel,
  ComplexModelViewModel,
  PersonViewModel,
} from "@test-targets/viewmodels.g";
import { Model } from "coalesce-vue";

describe("CDisplay", () => {
  const model = new ComplexModel({
    name: "bob",
    enumNullable: Statuses.InProgress,
    color: "#ff0000",
  });

  test("types", () => {
    const model = new ComplexModel();
    const vm = new ComplexModelViewModel();
    const ds = new Case.DataSources.AllOpenCases();

    () => <CDisplay model={vm} />;
    () => <CDisplay model={vm} for="color" />;
    () => <CDisplay model={vm} for="byteArrayProp" />;
    () => <CDisplay model={vm} for="singleTest" />;
    () => <CDisplay model={vm} for="tests" />;
    //@ts-expect-error non-existent prop
    () => <CDisplay model={vm} for="_anyString" />;

    // Possibly null model:
    () => <CDisplay model={vm.singleTest} />;
    () => <CDisplay model={vm.singleTest} for="testName" />;
    //@ts-expect-error non-existent prop
    () => <CDisplay model={vm.singleTest} for="asdf" />;

    () => <CDisplay model={vm as any} for="_anyString" />;
    () => <CDisplay model={model as Model} for="_anyString" />;
    () => <CDisplay model={model as Model} for={vm.$metadata.props.color} />;

    () => <CDisplay model={ds} for="minDate" />;
    //@ts-expect-error non-existent prop
    () => <CDisplay model={ds} for="badString" />;

    const caller = vm.methodWithManyParams;
    // Method caller args have fairly exhaustive cases here
    // for different types because ForSpec has unique handling
    // for caller args.
    () => <CDisplay model={caller} for="dateTime" />;
    () => <CDisplay model={caller} for="model" />;
    () => <CDisplay model={caller} for="strParam" />;
    () => <CDisplay model={caller} for="integer" />;
    () => <CDisplay model={caller} for="enumParam" />;
    () => <CDisplay model={caller} for="boolParam" />;
    () => <CDisplay model={caller} for="file" />;
    () => <CDisplay model={caller} for="stringsParam" />;
    () => <CDisplay model={caller} for="collectionExternal" />;
    //@ts-expect-error non-existent prop
    () => <CDisplay model={caller} for="badString" />;
  });

  test(":model", () => {
    const wrapper = mount(() => <CDisplay model={model} />);

    expect(wrapper.text()).toContain("bob");
  });

  test(":model for=propString", () => {
    const wrapper = mount(() => <CDisplay model={model} for="enumNullable" />);

    expect(wrapper.text()).toContain("In Progress");
  });

  test(":model for=metaObject", () => {
    const wrapper = mount(() => (
      <CDisplay model={model} for={model.$metadata.props.enumNullable} />
    ));

    expect(wrapper.text()).toContain("In Progress");
  });

  test(":model for=qualifiedString", () => {
    // This syntax is now deprecated and no longer supported by types
    // (hence the cast to any), but is still supported at runtime for now.
    const wrapper = mount(() => (
      <CDisplay model={model} for={"ComplexModel.enumNullable" as any} />
    ));

    expect(wrapper.text()).toContain("In Progress");
  });

  test(":value date", () => {
    const wrapper = mount(() => (
      <CDisplay value={new Date(1990, 0, 2, 3, 4, 5)} />
    ));

    expect(wrapper.text()).toContain("1/2/1990 3:04:05 AM");
  });

  test(":modelValue date", () => {
    const wrapper = mount(() => (
      <CDisplay
        modelValue={new Date(1990, 0, 2, 3, 4, 5)}
        format="yyyy-MM-dd hh:mm:ss a"
      />
    ));

    expect(wrapper.text()).toContain("1990-01-02 03:04:05 AM");
  });

  test("password", async () => {
    const model = new PersonViewModel({ secretPhrase: "Hunter2" });
    const wrapper = mount(() => <CDisplay model={model} for="secretPhrase" />);

    expect(wrapper.text()).toContain("••••••••");
    expect(wrapper.text()).not.toContain("Hunter2");

    await wrapper.find("[role=button]").trigger("click");

    expect(wrapper.text()).not.toContain("••••••••");
    expect(wrapper.text()).toContain("Hunter2");
  });

  test("email", async () => {
    const model = new PersonViewModel({ email: "bob@example.com" });
    const wrapper = mount(() => <CDisplay model={model} for="email" />);

    expect(wrapper.text()).toContain(model.email);
    expect(wrapper.find("a[href]").attributes()["href"]).toEqual(
      "mailto:" + model.email,
    );
  });

  test("phone", async () => {
    const model = new CompanyViewModel({ phone: "15095555555" });
    const wrapper = mount(() => <CDisplay model={model} for="phone" />);

    expect(wrapper.text()).toContain(model.phone);
    expect(wrapper.find("a[href]").attributes()["href"]).toEqual(
      "tel:" + model.phone,
    );
  });

  test.each([
    [true, "✓"],
    [false, "✗"],
  ])("boolean %s", async (value, display) => {
    model.isActive = value;
    const wrapper = mount(() => <CDisplay model={model} for="isActive" />);

    expect(wrapper.text()).toContain(display);
  });

  test("color", async () => {
    const wrapper = mount(() => <CDisplay model={model} for="color" />);

    expect(wrapper.text()).toContain(model.color);
    expect(
      wrapper.find(".c-display--color-swatch").attributes()["style"],
    ).toEqual(
      // Note: this assertion subject to nuances of jsdom,
      // which seems to normalize colors into `rgb` format.
      "background-color: rgb(255, 0, 0);",
    );
  });

  test("multiline", async () => {
    const model = new CaseViewModel({ description: "foo\nbar" });
    const wrapper = mount(() => <CDisplay model={model} for="description" />);

    expect(wrapper.text()).toContain(model.description);
    expect(wrapper.attributes()["style"]).toEqual("white-space: pre-wrap;");
  });
});
