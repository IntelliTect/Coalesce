import { delay, mockEndpoint, mount, mountApp, nextTick, openMenu } from "@test/util";
import { VListItem } from "vuetify/components";
import { CInput } from "..";
import { Case, Company, ComplexModel, Statuses, Test } from "@test-targets/models.g";
import { CaseViewModel, ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { Model } from "coalesce-vue";
import { VueWrapper } from "@vue/test-utils";

describe("CInput", () => {
  let model: ComplexModelViewModel;
  beforeEach(() => {
    model = new ComplexModelViewModel({
      name: "bob",
      enumNullable: Statuses.InProgress,
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
    () => <CInput model={vm} for="enumCollection" />;
    () => <CInput model={vm} for="intCollection" />;
    //@ts-expect-error non-existent prop
    () => <CInput model={vm} for="_anyString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={vm} />;

    // Against models that might be null
    () => <CInput model={vm.referenceNavigation} for="color" />;
    //@ts-expect-error non-existent prop
    () => <CInput model={vm.referenceNavigation} for="_anyString" />;

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
    () => <CInput model={caller} for="enumsParam" />;

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
    "ComplexModel.name" as "name",
    new ComplexModel().$metadata.props.name,
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
    const model = new CaseViewModel({status: Statuses.InProgress})
    const wrapper = mount(() => <CInput model={model} for="status" />);

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Status");
    expect(wrapper.text()).contains("In Progress");

    // Open the dropdown and select the third item.
    // Incredibly annoyingly, VSelect opens on mousedown, not on click.
    // It specifically passes openOnClick=false to VMenu and implements its own mousedown handler.
    await wrapper.find(".v-field").trigger("mousedown");
    await wrapper.findAllComponents(VListItem)[3].trigger("click");

    // The selected value should now be the 4th value of the enum
    expect(model.status).toBe(Statuses.ClosedNoSolution);
    expect(wrapper.text()).contains("Closed, No Solution");
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CInput model={model.methodWithManyParams} for="dateTime" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Date Time");

    // Set a value, and look for the value
    model.methodWithManyParams.args.dateTime = new Date("2023-08-16T01:02:03Z");
    await delay(10);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(10);
    expect(model.methodWithManyParams.args.dateTime.getFullYear()).toBe(2017);
  });

  test.each([true, false])("bool (checkbox: %s)", async (checkbox) => {
    const wrapper = mount(() => (
      <CInput model={model} for="isActive" checkbox={checkbox} />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Is Active");

    // Click the input
    await nextTick();
    await wrapper.find("input").setValue(true);
    await wrapper.find("input").trigger("input"); // workaround https://github.com/vuejs/test-utils/issues/2014
    await nextTick();

    // Value should now be true
    expect(model.isActive).toBe(true);
  });

  test("model via v-model", async () => {
    // Arrange
    const onUpdate = vitest.fn();

    const model = new ComplexModelViewModel({});
    model.methodWithManyParams.args.model = new Test({
      testId: 100,
      testName: "acme corp",
    });

    const newItem = new Test({ testId: 101, testName: "intellitect" });
    mockEndpoint("/Test/list", () => ({
      wasSuccessful: true,
      list: [newItem],
    }));

    const wrapper = mountApp(() => (
      <CInput
        for={model.$metadata.methods.methodWithManyParams.params.model}
        modelValue={model.methodWithManyParams.args.model}
        onUpdate:modelValue={onUpdate}
      ></CInput>
    )).findComponent(CInput);

    // Act
    expect(wrapper.text()).toContain("acme corp");
    await selectFirstResult(wrapper);

    // Assert: Emits event
    expect(onUpdate).toHaveBeenCalledWith(newItem);
  });

  test("date via v-model", async () => {
    // Arrange
    const onUpdate = vitest.fn();

    const model = new ComplexModelViewModel({});
    model.methodWithManyParams.args.dateTime = new Date("2023-08-16T01:02:03Z");

    const wrapper = mountApp(() => (
      <CInput
        for={model.$metadata.methods.methodWithManyParams.params.dateTime}
        modelValue={model.methodWithManyParams.args.dateTime}
        onUpdate:modelValue={onUpdate}
      ></CInput>
    )).findComponent(CInput);

    // Initial `modelValue` should pass through to c-datetime-picker:
    expect(wrapper.find("input").element.value).contains("2023");

    // Changing the date should emit the new value through `onUpdate`
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);

    expect(onUpdate).toHaveBeenCalledWith(new Date("1/3/2017"));
  });
});

async function selectFirstResult(wrapper: VueWrapper) {
  const overlay = await openMenu(wrapper);
  await overlay.find(".v-list-item").trigger("click");
}
