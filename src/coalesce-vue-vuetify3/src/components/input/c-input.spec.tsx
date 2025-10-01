import {
  delay,
  mockEndpoint,
  mount,
  mountApp,
  nextTick,
  openMenu,
} from "@test/util";
import { VForm, VListItem } from "vuetify/components";
import { CInput } from "..";
import {
  Case,
  CaseProduct,
  Company,
  ComplexModel,
  EnumPk,
  Statuses,
  Test,
} from "@test-targets/models.g";
import {
  CaseViewModel,
  ComplexModelViewModel,
} from "@test-targets/viewmodels.g";
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

  /* eslint-disable @typescript-eslint/no-unused-vars */
  // prettier-ignore
  test("types", () => {
    const model = new ComplexModel();
    const vm = new ComplexModelViewModel();
    const caseVm = new CaseViewModel();
    const ds = new Case.DataSources.AllOpenCases();

    () => <CInput model={model} for="color" onUpdate:modelValue={(v: string | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={model} for="color" onUpdate:modelValue={(v: number | null) => { }} />;

    () => <CInput model={vm} for="color" onUpdate:modelValue={(v: string | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="color" onUpdate:modelValue={(v: number | null) => { }} />;

    () => <CInput model={vm} for="dateTime" onUpdate:modelValue={(v: Date | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="dateTime" onUpdate:modelValue={(v: number | null) => { }} />;

    // () => <CInput model={vm} for="byteArrayProp" onUpdate:modelValue={(v: string) => { }} />;

    () => <CInput model={vm} for="singleTest" onUpdate:modelValue={(v: Test | null) => { }} />;
    () => <CInput model={vm} for="singleTestId" onUpdate:modelValue={(v: number | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="singleTest" onUpdate:modelValue={(v: string | null) => { }} />;

    () => <CInput model={vm} for="tests" onUpdate:modelValue={(v: Test[] | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="tests" onUpdate:modelValue={(v: string | null) => { }} />;

    () => <CInput model={vm} for="enumCollection" onUpdate:modelValue={(v: Statuses[] | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="enumCollection" onUpdate:modelValue={(v: string | null) => { }} />;

    () => <CInput model={vm} for="intCollection" onUpdate:modelValue={(v: number[] | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={vm} for="intCollection" onUpdate:modelValue={(v: string[] | null) => { }} />;

    // Many-to-many
    () => <CInput model={caseVm} for="caseProducts" onUpdate:modelValue={(v: CaseProduct[] | null) => { }} />;
    //@ts-expect-error wrong event handler type
    () => <CInput model={caseVm} for="caseProducts" onUpdate:modelValue={(v: number[] | null) => { }} />;

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

    () => <CInput model={ds} for="minDate" onUpdate:modelValue={(v: Date | null) => { }} />;
    //@ts-expect-error non-existent prop
    () => <CInput model={ds} for="badString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={ds} />;

    const caller = vm.methodWithManyParams;
    // Method caller args have fairly exhaustive cases here
    // for different types because ForSpec has unique handling
    // for caller args.
    () => <CInput model={caller} for="dateTime" onUpdate:modelValue={(v: Date | null) => { }} />;
    () => <CInput model={caller} for="model" onUpdate:modelValue={(v: Test | null) => { }} />;
    () => <CInput model={caller} for="strParam" onUpdate:modelValue={(v: string | null) => { }} />;
    () => <CInput model={caller} for="integer" onUpdate:modelValue={(v: number | null) => { }} />;
    () => <CInput model={caller} for="enumParam" onUpdate:modelValue={(v: Statuses | null) => { }} />;
    () => <CInput model={caller} for="boolParam" onUpdate:modelValue={(v: boolean | null) => { }} />;
    () => <CInput model={caller} for="file" onUpdate:modelValue={(v: File | null) => { }} />;
    () => <CInput model={caller} for="stringsParam" onUpdate:modelValue={(v: string[] | null) => { }} />;
    () => <CInput model={caller} for="enumsParam" onUpdate:modelValue={(v: Statuses[] | null) => { }} />;
    () => <CInput model={caller} for="modelCollection" onUpdate:modelValue={(v: Test[] | null) => { }} />;

    // @ts-expect-error wrong type
    () => <CInput model={caller} for="dateTime" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="model" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="strParam" onUpdate:modelValue={(v: number | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="integer" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="enumParam" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="boolParam" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="file" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="stringsParam" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="enumsParam" onUpdate:modelValue={(v: string | null) => { }} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="modelCollection" onUpdate:modelValue={(v: string | null) => { }} />;

    // While external types don't make an input for CInput,
    // they aren't technically invalid either because they at least fall back to c-display.
    () => <CInput model={caller} for="singleExternal" />;
    () => <CInput model={caller} for="collectionExternal" />;

    //@ts-expect-error non-existent prop
    () => <CInput model={caller} for="badString" />;
    //@ts-expect-error missing `for`
    () => <CInput model={caller} />;

    // ******
    // Vuetify props
    // ******
    () => <CInput model={model} for="color" variant="outlined" />;
    //@ts-expect-error bad prop value
    () => <CInput model={model} for="color" variant="bad-variant" />;


    // ******
    // Enum filter
    // ******
    () => <CInput model={vm} for="enumNullable" filter={(v) => v.description == "foo"} />;
    () => <CInput model={caller} for="enumParam" filter={(v) => v.description == "foo"} />;
    () => <CInput model={vm} for="enumCollection" filter={(v) => v.description == "foo"} />;
    () => <CInput model={caller} for="enumsParam" filter={(v) => v.description == "foo"} />;
    // @ts-expect-error wrong type
    () => <CInput model={caller} for="strParam" filter={(v) => v.description == "foo"} />;
    // @ts-expect-error wrong type
    () => <CInput model={vm} for="string" filter={(v) => v.description == "foo"} />;
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
    const model = new CaseViewModel({ status: Statuses.InProgress });
    const wrapper = mount(() => <CInput model={model} for="status" />);

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Status");
    expect(wrapper.text()).contains("In Progress");

    // Open the dropdown and select the 4th item.
    // Incredibly annoyingly, VSelect opens on mousedown, not on click.
    // It specifically passes openOnClick=false to VMenu and implements its own mousedown handler.
    await wrapper.find(".v-field").trigger("mousedown");
    await wrapper.findAllComponents(VListItem)[3].trigger("click");

    // The selected value should now be the 4th value of the enum
    expect(model.status).toBe(Statuses.ClosedNoSolution);
    expect(wrapper.text()).contains("Closed, No Solution");
  });

  test("enum filtered", async () => {
    const model = new CaseViewModel({ status: Statuses.InProgress });
    const wrapper = mount(() => (
      <CInput
        model={model}
        for="status"
        filter={(v) => v.value != Statuses.ClosedNoSolution}
      />
    ));

    // Open the dropdown and select the 4th item.
    await wrapper.find(".v-field").trigger("mousedown");
    await wrapper.findAllComponents(VListItem)[3].trigger("click");

    // The selected value should now be the 4th value of the list,
    // which is the 5th value of the enum because ClosedNoSolution was the 4th and we filtered it out.
    expect(model.status).toBe(Statuses.Cancelled);
    expect(wrapper.text()).contains("Cancelled");
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

  test("collection navigation doesn't produce c-select", () => {
    // Non many-to-many model collection navigation  props should not produce an input,
    // so that they can fall back on their default slot (e.g. for c-admin-editor)

    const model = new ComplexModelViewModel({});
    const wrapper = mountApp(() => (
      <CInput model={model} for="tests">
        <div>Default Slot Content</div>
      </CInput>
    )).findComponent(CInput);

    expect(wrapper.text()).toBe("Default Slot Content");
  });

  describe("rules", () => {
    test("implicit from metadata", async () => {
      const model = new ComplexModelViewModel({ guid: "foo" });
      const wrapper = mountApp(() => (
        <VForm>
          <CInput model={model} for="guid" />
        </VForm>
      ));

      await wrapper.findComponent(VForm).vm.validate();

      expect(wrapper.find(".v-input--error").exists()).toBeTruthy();
      expect(wrapper.find(".v-messages").text()).toEqual(
        "Guid does not match expected format.",
      );
    });

    test("date - missing", async () => {
      const model = new ComplexModelViewModel({});
      const wrapper = mountApp(() => (
        <VForm>
          <CInput
            model={model}
            for="dateTime"
            rules={[(v) => !!v || "Custom Rule Failure"]}
          />
        </VForm>
      ));

      await wrapper.findComponent(VForm).vm.validate();

      expect(wrapper.find(".v-input--error").exists()).toBeTruthy();
      expect(wrapper.find(".v-messages").text()).toEqual("Custom Rule Failure");
    });

    test("number - rule receives number, not string", async () => {
      const model = new ComplexModelViewModel({ intNullable: 7 });
      const rule = vitest.fn(
        (v: number | null | undefined) => v === 7 || "Custom Rule Failure",
      );
      const wrapper = mountApp(() => (
        <VForm>
          <CInput model={model} for="intNullable" rules={[rule]} />
        </VForm>
      ));

      await wrapper.findComponent(VForm).vm.validate();
      expect(wrapper.find(".v-input--error").exists()).toBeFalsy();

      wrapper.find("input").setValue("42");
      await wrapper.findComponent(VForm).vm.validate();

      expect(rule).toHaveBeenCalledWith(7);
      expect(rule).lastCalledWith(42);
      expect(rule).not.toHaveBeenCalledWith("7");
      expect(rule).not.toHaveBeenCalledWith("42");
    });

    test("string - empty string becomes null", async () => {
      const model = new ComplexModelViewModel({ name: "initial value" });
      const wrapper = mount(() => <CInput model={model} for="name" />);

      // Verify initial state
      expect(model.name).toBe("initial value");
      expect(wrapper.find("input").element.value).toBe("initial value");

      // Clear the input - this should set the value to null, not empty string
      await wrapper.find("input").setValue("");
      expect(model.name).toBe(null);

      // Type something new
      await wrapper.find("input").setValue("new value");
      expect(model.name).toBe("new value");

      // Clear again - should go back to null
      await wrapper.find("input").setValue("");
      expect(model.name).toBe(null);
    });

    test("number - empty string becomes null", async () => {
      const model = new ComplexModelViewModel({ intNullable: 42 });
      const wrapper = mount(() => <CInput model={model} for="intNullable" />);

      // Verify initial state
      expect(model.intNullable).toBe(42);
      expect(wrapper.find("input").element.value).toBe("42");

      // Clear the input - this should set the value to null, not 0
      await wrapper.find("input").setValue("");
      expect(model.intNullable).toBe(null);

      // Type a new number
      await wrapper.find("input").setValue("123");
      expect(model.intNullable).toBe(123);

      // Clear again - should go back to null, not 0
      await wrapper.find("input").setValue("");
      expect(model.intNullable).toBe(null);
    });
  });
});

async function selectFirstResult(wrapper: VueWrapper) {
  const overlay = await openMenu(wrapper);
  await overlay.find(".v-list-item").trigger("click");
}
