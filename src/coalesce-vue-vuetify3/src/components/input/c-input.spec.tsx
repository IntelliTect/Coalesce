import { Grade, Student } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import { delay, mount, nextTick } from "@test/util";
import { VListItem } from "vuetify/components";
import { CInput } from "..";
import { Case, ComplexModel } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { h } from "vue";
import { Model } from "coalesce-vue";

describe("CInput", () => {
  let model: StudentViewModel;
  beforeEach(() => {
    model = new StudentViewModel({
      name: "bob",
      grade: Grade.Freshman,
      password: "secretValue",
      email: "bob@college.edu",
      phone: "123-123-1234",
      color: "#ff0000",
      notes: "multiline\n\nstring",
    });
  });

  test("types", () => {
    const model = new ComplexModel();
    const vm = new ComplexModelViewModel();
    const ds = new Case.DataSources.AllOpenCases();

    () => <CInput model={model} for="color" />;
    () => <CInput model={vm} for="color" />;
    () => <CInput model={vm} for="byteArrayProp" />;
    () => <CInput model={vm} for="singleTest" />;
    () => <CInput model={vm} for="tests" />;
    //@ts-expect-error non-existent prop
    () => <CInput model={vm} for="_anyString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={vm} />;

    () => <CInput model={vm as any} for="_anyString" />;
    () => <CInput model={model as Model} for="_anyString" />;
    () => <CInput model={model as Model} for={vm.$metadata.props.color} />;

    () => <CInput model={ds} for="minDate" />;
    //@ts-expect-error non-existent prop
    () => <CInput model={ds} for="badString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={ds} />;

    const caller = vm.methodWithManyParams;
    // Method caller args have fairly exhaustive cases here
    // for different types because ForSpec has unique handling
    // for caller args.
    () => <CInput model={caller} for="dateTime" />;
    () => <CInput model={caller} for="model" />;
    () => <CInput model={caller} for="strParam" />;
    () => <CInput model={caller} for="integer" />;
    () => <CInput model={caller} for="enumParam" />;
    () => <CInput model={caller} for="boolParam" />;
    () => <CInput model={caller} for="file" />;
    () => <CInput model={caller} for="stringsParam" />;

    // While external types don't make an input for CInput,
    // they aren't technically invalid either because they at least fall back to c-display.
    () => <CInput model={caller} for="singleExternal" />;
    () => <CInput model={caller} for="collectionExternal" />;

    //@ts-expect-error non-existent prop
    () => <CInput model={caller} for="badString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={caller} />;
  });

  test.each([
    "name",
    // "Type.prop" w/ `model` is deprecated, no longer supported by types but still supported at runtime */
    "Student.name" as "name",
    new Student().$metadata.props.name,
  ] as const)("metadata resolves - %s", async (forSpec) => {
    const wrapper = mount(() => <CInput model={model} for={forSpec} />);

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Name");
    expect(wrapper.find("input").element.value).toBe("bob");

    // Perform input
    await wrapper.find("input").setValue("steve");
    expect(model.name).toBe("steve");
  });

  test("enum", async () => {
    const wrapper = mount(() => <CInput model={model} for="grade" />);

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Grade");
    expect(wrapper.text()).contains("Freshman");

    // Open the dropdown and select the third item.
    // Incredibly annoyingly, VSelect opens on mousedown, not on click.
    // It specifically passes openOnClick=false to VMenu and implements its own mousedown handler.
    await wrapper.find(".v-field").trigger("mousedown");
    await wrapper.findAllComponents(VListItem)[2].trigger("click");

    // The selected value should now be Junior, the third value of the enum
    expect(model.grade).toBe(Grade.Junior);
    expect(wrapper.text()).contains("Junior");
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CInput model={model.manyParams} for="startDate" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Start Date");

    // Set a value, and look for the value
    model.manyParams.args.startDate = new Date("2023-08-16T01:02:03Z");
    await delay(10);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(10);
    expect(model.manyParams.args.startDate.getFullYear()).toBe(2017);
  });

  test.each([true, false])("bool (checkbox: %s)", async (checkbox) => {
    const wrapper = mount(() => (
      <CInput model={model} for="isEnrolled" checkbox={checkbox} />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Is Enrolled");

    // Click the input
    await nextTick();
    await wrapper.find("input").setValue(true);
    await wrapper.find("input").trigger("input"); // workaround https://github.com/vuejs/test-utils/issues/2014
    await nextTick();

    // Value should now be true
    expect(model.isEnrolled).toBe(true);
  });
});
