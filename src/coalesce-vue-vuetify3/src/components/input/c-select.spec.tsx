import {
  getWrapper,
  mountApp,
  nextTick,
  flushPromises,
  BetterComponentInstance,
  delay,
  mockEndpoint,
  openMenu,
} from "@test/util";
import { VueWrapper } from "@vue/test-utils";
import {
  AnyArgCaller,
  ForeignKeyProperty,
  Model,
  ModelReferenceNavigationProperty,
} from "coalesce-vue";
import { Mock } from "vitest";
import { FunctionalComponent, ref } from "vue";
import { VForm } from "vuetify/components";
import { CSelect } from "..";

import {
  Case,
  Company,
  ComplexModel,
  EnumPkId,
  Person,
  Test,
} from "@test-targets/models.g";
import {
  CaseViewModel,
  ComplexModelViewModel,
  PersonViewModel,
  TestViewModel,
} from "@test-targets/viewmodels.g";

describe("CSelect", () => {
  let model = new ComplexModel({
    name: "bob",
  });

  let listMock: Mock, get303Mock: Mock;

  beforeEach(() => {
    model = new ComplexModel({
      name: "bob",
    });

    listMock = mockEndpoint(
      "/Test/list",
      vitest.fn((config) => {
        const items = [
          new Test({ testId: 101, testName: "foo 101" }),
          new Test({ testId: 202, testName: "bar 202" }),
        ].filter(
          (c, i) =>
            (!config.params.search ||
              c.testName?.startsWith(config.params.search)) &&
            (!config.params.pageSize || i < config.params.pageSize),
        );
        return {
          wasSuccessful: true,
          list: items,
          page: 1,
          pageCount: 1,
          pageSize: 10,
          totalCount: items.length,
        };
      }),
    );

    get303Mock = mockEndpoint(
      "/Test/get/303",
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: new Test({ testId: 303, testName: "baz 303" }),
      })),
    );
  });

  // prettier-ignore
  test("types", () => {
    const model = new ComplexModel({
      name: "bob",
    });
    const complexVm = new ComplexModelViewModel();
    const testVm = new TestViewModel();

    const anyString: string = "foo";
    const genericModel: Model = model;

    function receivesTestModel(model: Test | null) {}
    function receivesTestModels(model: Test[] | null) {}
    function receivesComplexModel(model: ComplexModel | null) {}
    function receivesNumber(model: number | null) {}
    function receivesNumbers(model: number[] | null) {}

    // Binding to FK or ref nav on a ViewModel:
    () => <CSelect model={complexVm} for="singleTest" />;
    () => (<CSelect model={complexVm} for="singleTest" onUpdate:modelValue={receivesTestModel} />);
    () => (<CSelect model={complexVm} for="singleTest" onUpdate:objectValue={receivesTestModel} />);
    // @ts-expect-error wrong event handler type.
    () => (<CSelect model={complexVm} for="singleTest" onUpdate:objectValue={receivesComplexModel} />);
    // @ts-expect-error wrong event handler type.
    () => (<CSelect model={complexVm} for="singleTest" onUpdate:keyValue={receivesComplexModel} />);

    () => <CSelect model={complexVm} for="singleTestId" />;
    () => <CSelect model={complexVm} for="singleTestId" onUpdate:keyValue={receivesNumber} />;
    // @ts-expect-error wrong event handler type.
    () => <CSelect model={complexVm} for="singleTestId" onUpdate:modelValue={receivesTestModel} />;

    () => <CSelect model={complexVm} for="singleTest" create={{
      getLabel(search: string, items: Test[]) { return items.length == 0 ? search : false },
      async getItem(search: string, label: string) { return new Test({ testName: label }) }
    }}/>;

    () => <CSelect model={complexVm} for={complexVm.$metadata.props.singleTest} />;

    // Against models that might be null
    () => <CSelect model={complexVm.referenceNavigation} for="referenceNavigation" />;

    // Binding to collection navigation
    () => <CSelect model={new Person} for="casesAssigned" />;

    // Unfortunately, after adding support for binding to non-many-to-many collection navigation props,
    // there doesn't seem to be any practical way to exclude manyToMany props anymore with TS.
    // This is still a runtime error, but we can't enforce it with types.
    // // @ts-expect-error Cannot bind to many-to-many
    // () => <CSelect model={new Case} for="caseProducts" />;

    // Binding to plain Models:
    () => <CSelect model={model} for="singleTest" />;
    () => <CSelect model={model} for="singleTestId" />;
    //@ts-expect-error wrong type of property
    () => <CSelect model={model} for="name" />;
    //@ts-expect-error wrong type of property
    () => <CSelect model={complexVm} for={complexVm.$metadata.props.name} />;

    // Untyped bindings:
    () => <CSelect model={genericModel} for={anyString} />;
    () => <CSelect model={model as any} for={anyString} />;
    
    //@ts-expect-error invalid `model` type
    () => <CSelect model={123} for="num" />;
    //@ts-expect-error invalid `for` type
    () => <CSelect model={complexVm} for={123} />;



    // ********
    // Multiple
    // ********
    () => (<CSelect for="Test" multiple modelValue={[testVm]}
      onUpdate:modelValue={receivesTestModels}
      onUpdate:objectValue={receivesTestModels}
      onUpdate:keyValue={receivesNumbers}
      create={{
        getLabel(search: string, items: Test[]) { return items.length == 0 ? search : false },
        async getItem(search: string, label: string) { return new Test({ testName: label }) }
      }}
    />);
    
    // Explicit `multiple` for object collection
    () => (<CSelect model={complexVm.methodWithManyParams} for="modelCollection" multiple modelValue={[testVm]} onUpdate:modelValue={receivesTestModels} />);
    // Implicit `multiple` for object collection
    () => (<CSelect model={complexVm.methodWithManyParams} for="modelCollection" modelValue={[testVm]} onUpdate:modelValue={receivesTestModels} />);
    
    //@ts-expect-error Explicit `multiple` for object non-collection is disallowed
    () => (<CSelect model={complexVm.methodWithManyParams} for="model" multiple modelValue={[testVm]} onUpdate:modelValue={receivesTestModels} />);
    //@ts-expect-error Explicit `multiple` for object non-collection is disallowed
    () => (<CSelect model={complexVm.methodWithManyParams} for="model" multiple modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    //@ts-expect-error Arrays passed to non-multiple binding
    () => (<CSelect model={complexVm.methodWithManyParams} for="model" modelValue={[testVm]} onUpdate:modelValue={receivesTestModels} />);
    
    //@ts-expect-error arrays passed to non-multiple
    () => (<CSelect for="Test" modelValue={[testVm]} onUpdate:modelValue={receivesTestModels} />);
    
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple modelValue={testVm}  />);
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple onUpdate:modelValue={receivesTestModel}  />);
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple objectValue={testVm} />);
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple onUpdate:objectValue={receivesTestModel} />);
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple keyValue={2} />);
    //@ts-expect-error arrays required for multiple
    () => (<CSelect for="Test" multiple onUpdate:keyValue={receivesNumber}  />);
    


    // ********
    // Binding with for + v-model
    // ********
    () => (<CSelect for={testVm.$metadata} modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:objectValue={receivesTestModel} />);
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:keyValue={receivesNumber} />);
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:keyValue={(e) => { e?.toFixed() }} />);
    // @ts-expect-error wrong event handler type.
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:keyValue={receivesTestModel} />);
    // @ts-expect-error wrong event handler type.
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:objectValue={(receivesComplexModel)} />);
    // @ts-expect-error wrong event handler type.
    () => (<CSelect for="Test" modelValue={testVm} onUpdate:modelValue={receivesComplexModel} />);

    // @ts-expect-error wrong keyValue type.
    () => (<CSelect for="Test" keyValue={anyString} />);
    // @ts-expect-error wrong modelValue type.
    () => (<CSelect for="Test" modelValue={complexVm} onUpdate:modelValue={receivesTestModel} />);

    () => (<CSelect for="EnumPk" keyValue={EnumPkId.Value10} onUpdate:keyValue={(v: EnumPkId | null) => {}} />);
    // @ts-expect-error keyValue type
    () => (<CSelect for="EnumPk" keyValue={'42'} onUpdate:keyValue={(v: EnumPkId | null) => {}} />);
    // @ts-expect-error onUpdate type
    () => (<CSelect for="EnumPk" keyValue={EnumPkId.Value10} onUpdate:keyValue={receivesComplexModel} />);


    // ********
    // Prop types that are only weakly known, missing ability to resolve types
    // ********
    const weakModelProp: ModelReferenceNavigationProperty = complexVm.$metadata.props.singleTest;
    () => (<CSelect for={weakModelProp} modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    () => (<CSelect for={weakModelProp} keyValue={42} onUpdate:keyValue={receivesNumber} />);

    const weakFkProp: ForeignKeyProperty = complexVm.$metadata.props.singleTestId;
    () => (<CSelect for={weakFkProp} modelValue={42} onUpdate:modelValue={receivesNumber} />);
    () => (<CSelect for={weakFkProp} objectValue={testVm} onUpdate:objectValue={receivesTestModel} />);
    () => (<CSelect for={weakFkProp} keyValue={42} onUpdate:keyValue={receivesNumber} />);
    
    () => (<CSelect model={genericModel} for="singleTest" modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    () => (<CSelect model={genericModel} for="singleTest" objectValue={testVm} onUpdate:objectValue={receivesTestModel} />);
    () => (<CSelect model={genericModel} for="singleTest" keyValue={42} onUpdate:keyValue={receivesNumber} />);


    // ********
    // Passing concrete metadata objects
    // ********
    () => (<CSelect for={complexVm.$metadata.props.singleTest} modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    // @ts-expect-error wrong modelValue and event types
    () => (<CSelect for={complexVm.$metadata.props.singleTest} modelValue={42} onUpdate:modelValue={receivesNumber} />);

    () => (<CSelect for={complexVm.$metadata.props.singleTestId} modelValue={42} onUpdate:modelValue={receivesNumber} />);
    // @ts-expect-error wrong modelValue and event types
    () => (<CSelect for={complexVm.$metadata.props.singleTestId} modelValue={testVm} onUpdate:modelValue={receivesTestModel} />);
    
    // Binding to method parameters
    () => <CSelect model={complexVm.methodWithManyParams} for="model" />;
    () => <CSelect model={complexVm.methodWithManyParams} for="model" onUpdate:modelValue={(x: Test | null) => {console.log(x)}} />;
    // @ts-expect-error wrong onUpdate:modelValue type
    () => <CSelect model={complexVm.methodWithManyParams} for="model" onUpdate:modelValue={receivesComplexModel} />;
    // @ts-expect-error wrong modelValue type
    () => <CSelect model={complexVm.methodWithManyParams} for="model" modelValue={complexVm} />;
    () => (<CSelect model={complexVm.methodWithManyParams} for={complexVm.$metadata.methods.methodWithManyParams.params.model} />);
    () => (<CSelect model={complexVm.methodWithManyParams}
      for={complexVm.$metadata.methods.methodWithManyParams.params.model}
      onUpdate:modelValue={(x: Test | null) => {console.log(x)}}
    />);
    () => (<CSelect model={complexVm.methodWithManyParams}
      for={complexVm.$metadata.methods.methodWithManyParams.params.model}
      // @ts-expect-error wrong onUpdate:modelValue type
      onUpdate:modelValue={receivesComplexModel}
    />);

    //@ts-expect-error non-object method parameter
    () => <CSelect model={complexVm.methodWithManyParams} for="integer" />;

    () => (<CSelect model={complexVm.methodWithManyParams as AnyArgCaller} for="model" />);
    // This has to be valid when we don't have a known type for the caller
    () => (<CSelect model={complexVm.methodWithManyParams as AnyArgCaller} for="specificString" />);


    // ********
    // Rules
    // ********
    () => <CSelect model={complexVm} for="singleTest" rules={[v => v === 7 || 'Must be 7']} />;
    //@ts-expect-error invalid rule func (`v` is number, equality to string is invalid).
    () => <CSelect model={complexVm} for="singleTest" rules={[v => v === "foo" || 'Must be 7']} />;
    
    // ********
    // Vuetify props
    // ********
    () => <CSelect for="Test" variant="outlined" />;
    //@ts-expect-error
    () => <CSelect for="Test" variant="bad-variant" />;

    () => <CSelect for="Test"  />;
  });

  test.each([
    {
      props: { model, for: "singleTest" },
      label: "Single Test",
      hint: "The active Test record for the model.",
    },
    {
      props: { model, for: "singleTestId" },
      label: "Single Test",
      hint: "The active Test record for the model.",
    },
    {
      props: { model, for: model.$metadata.props.singleTest },
      label: "Single Test",
      hint: "The active Test record for the model.",
    },
    {
      props: { model, for: model.$metadata.props.singleTestId },
      label: "Single Test",
      hint: "The active Test record for the model.",
    },
    { props: { for: "Test" }, label: "Test", hint: "" },
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
          for="Test"
          label="custom label"
          hint="custom hint"
          persistent-hint
          clearable
        ></CSelect>
      ));

      expect(wrapper.find("label").text()).toEqual("custom label");
      expect(wrapper.find(".v-field__clearable").exists()).toBeTruthy();
    });
  });

  describe("clearable defaults to required state of FK", () => {
    test("required fk", async () => {
      mockEndpoint("/Test/list", () => ({ wasSuccessful: true, list: [] }));

      const model = new ComplexModelViewModel();
      const wrapper = mountApp(() => (
        <CSelect model={model} for="singleTest"></CSelect>
      ));

      expect(model.$metadata.props.singleTestId.rules.required).toBeTruthy();
      expect(wrapper.find(".v-field__clearable").exists()).toBeFalsy();
    });

    test("optional fk", async () => {
      mockEndpoint("/ComplexModel/list", () => ({
        wasSuccessful: true,
        list: [],
      }));

      const model = new ComplexModelViewModel();
      const wrapper = mountApp(() => (
        <CSelect model={model} for="referenceNavigation"></CSelect>
      ));

      expect(
        //@ts-expect-error We're asserting this prop doesn't have a required rule,
        // but our types are so good that this even gets caught by typescript.
        model.$metadata.props.referenceNavigationId.rules?.required,
      ).toBeFalsy();

      expect(wrapper.find(".v-field__clearable").exists()).toBeTruthy();
    });

    test("optional method param", async () => {
      mockEndpoint("/Test/list", () => ({
        wasSuccessful: true,
        list: [],
      }));

      const model = new ComplexModelViewModel();
      const wrapper = mountApp(() => (
        <CSelect
          model={model.methodWithOptionalParams}
          for="optionalObject"
        ></CSelect>
      ));

      const methodMeta = model.$metadata.methods.methodWithOptionalParams;

      expect(
        // @ts-expect-error We're asserting this prop doesn't have a required rule,
        // but our types are so good that this even gets caught by typescript.
        methodMeta.params.optionalObject.rules?.required,
      ).toBeFalsy();

      expect(wrapper.find(".v-field__clearable").exists()).toBeTruthy();
    });
  });

  test("autofocus applies to correct element", () => {
    const wrapper = mountApp(() => <CSelect for="Test" autofocus></CSelect>);

    const elements = wrapper.findAll("[autofocus]");
    expect(elements).toHaveLength(1);
    expect(elements[0].element).toBeInstanceOf(HTMLInputElement);
  });

  describe("hint", () => {
    test("shows persistent hint", () => {
      const wrapper = mountApp(() => (
        <CSelect for="Test" hint="custom hint" persistent-hint></CSelect>
      ));

      expect(wrapper.find(".v-messages").text()).toEqual("custom hint");
    });

    test("shows non-persistent hint when focused", async () => {
      const wrapper = mountApp(() => (
        <CSelect for="Test" hint="custom hint"></CSelect>
      ));

      expect(wrapper.find(".v-messages").text()).toEqual("");
      wrapper.find("input").element.focus();
      await delay(10);

      expect(wrapper.find(".v-messages").text()).toEqual("custom hint");
    });

    test("allow null hint", async () => {
      const wrapper = mountApp(() => (
        <CSelect for="Test" hint={null}></CSelect>
      ));

      expect(wrapper.find(".v-messages").text()).toEqual("");
      wrapper.find("input").element.focus();
      await delay(10);

      expect(wrapper.find(".v-messages").text()).toEqual("");
    });
  });

  describe("validation", () => {
    async function assertValidation(
      TargetComponent: FunctionalComponent,
      message: string,
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
      { p: "singleTest", d: "navigation" },
      { p: "singleTestId", d: "fk" },
    ] as const)("when bound by $d", ({ p: propName }) => {
      test("pulls validation from fk metadata", async () => {
        await assertValidation(
          () => <CSelect model={model} for={propName}></CSelect>,
          "Single Test is required.",
        );
      });

      test("pulls validation from ViewModel instance", async () => {
        const model = new ComplexModelViewModel();
        model.$addRule(
          "singleTestId",
          "required",
          (v: any) => !!v || "Custom rule from VM.",
        );
        await assertValidation(
          () => <CSelect model={model} for={propName}></CSelect>,
          "Custom rule from VM.",
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
          "Custom rule",
        );
      });
    });
  });

  describe("value binding", () => {
    test("keyValue fetches object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect for="Test" keyValue={303}></CSelect>
      ));
      await flushPromises();

      // Assert - the name of the selected option should be fetched from the server
      // and show as the selected item.
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(listMock).toHaveBeenCalledTimes(1);
      expect(get303Mock).toHaveBeenCalledTimes(1);
    });

    test("model + FK-only fetches object from server", async () => {
      // Arrange/Act
      const model = new ComplexModel({ singleTestId: 303 });
      const wrapper = mountApp(() => (
        <CSelect model={model} for="singleTest"></CSelect>
      ));
      await flushPromises();

      // Assert - the name of the selected option should be fetched from the server
      // and show as the selected item.
      expect(wrapper.text()).toContain("baz 303");
      // Two calls - one list, and one /get for the specific key value.
      expect(listMock).toHaveBeenCalledTimes(1);
      expect(get303Mock).toHaveBeenCalledTimes(1);
    });

    test("objectValue does not fetch object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect
          for="Test"
          objectValue={new Test({ testId: 303, testName: "baz 303" })}
        ></CSelect>
      ));
      await flushPromises();

      // Assert
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(listMock).toHaveBeenCalledTimes(1);
      expect(get303Mock).toHaveBeenCalledTimes(0);
    });

    test("modelValue does not fetch object from server", async () => {
      // Arrange/Act
      const wrapper = mountApp(() => (
        <CSelect
          for="Test"
          modelValue={new Test({ testId: 303, testName: "baz 303" })}
        ></CSelect>
      ));
      await flushPromises();

      // Assert
      expect(wrapper.text()).toContain("baz 303");
      // One call - just /list
      expect(listMock).toHaveBeenCalledTimes(1);
      expect(get303Mock).toHaveBeenCalledTimes(0);
    });

    test("emits updates on selection", async () => {
      // Arrange
      const onUpdateKey = vitest.fn();
      const onUpdateObject = vitest.fn();
      const onUpdateModel = vitest.fn();

      const wrapper = mountApp(() => (
        <CSelect
          for="Test"
          onUpdate:keyValue={onUpdateKey}
          onUpdate:objectValue={onUpdateObject}
          onUpdate:modelValue={onUpdateModel}
        ></CSelect>
      )).findComponent(CSelect<Test>);

      // Act
      await selectFirstResult(wrapper);

      // Assert: Emits events
      expect(onUpdateKey).toHaveBeenCalledWith(101);
      expect(onUpdateObject).toHaveBeenCalledWith(
        new Test({ testId: 101, testName: "foo 101" }),
      );
      expect(onUpdateModel).toHaveBeenCalledWith(
        new Test({ testId: 101, testName: "foo 101" }),
      );

      // Assert: Menu closes after selection
      expect(wrapper.vm.menuOpen).toBe(false);
    });

    test.each([
      new ComplexModel({ singleTestId: 303 }),
      new ComplexModelViewModel({ singleTestId: 303 }),
    ])("mutates model on selection when bound by model", async (model) => {
      // Arrange
      const onUpdateObject = vitest.fn();
      const onUpdateModel = vitest.fn();
      const wrapper = mountApp(() => (
        <CSelect
          model={model}
          for="singleTest"
          onUpdate:modelValue={onUpdateModel}
          onUpdate:objectValue={onUpdateObject}
        ></CSelect>
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert: `model` prop mutated
      expect(model.singleTestId).toBe(101);
      expect(model.singleTest?.testId).toBe(101);

      // Emitted event value should exactly equal by instance the resulting value on `model`.
      expect(onUpdateModel.mock.calls[0][0]).toBe(model.singleTest);
      expect(onUpdateObject.mock.calls[0][0]).toBe(model.singleTest);
    });

    test("mutates model on selection when bound by apicaller arg", async () => {
      const model = new ComplexModelViewModel({});
      const wrapper = mountApp(() => (
        <CSelect model={model.methodWithManyParams} for="model" />
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert: arg prop mutated
      expect(model.methodWithManyParams.args.model?.testId).toBe(101);
    });

    test("mutates v-model on selection when bound by apicaller arg", async () => {
      const model = new ComplexModelViewModel({});
      const onUpdate = vitest.fn<(x: Test | null) => void>();

      const wrapper = mountApp(() => (
        <CSelect
          for={model.$metadata.methods.methodWithManyParams.params.model}
          modelValue={model.methodWithManyParams.args.model}
          onUpdate:modelValue={onUpdate}
        />
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert
      expect(onUpdate).toHaveBeenCalledWith(
        new Test({ testId: 101, testName: "foo 101" }),
      );
    });
  });

  describe("multiple", () => {
    test("emits ViewModel to onSelectionChanged when bound by ViewModelCollection", async () => {
      // Arrange
      const model = new ComplexModelViewModel({ singleTestId: 303 });
      const onUpdateObject = vitest.fn();
      const onSelectionChanged = vitest.fn();
      const wrapper = mountApp(() => (
        <CSelect
          for="Test"
          multiple
          onUpdate:modelValue={(v) => (model.tests = v)}
          modelValue={model.tests}
          onSelectionChanged={onSelectionChanged}
          onUpdate:objectValue={onUpdateObject}
        ></CSelect>
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert
      expect(model.tests).toHaveLength(1);
      const selectedItem = model.tests[0];
      expect(selectedItem.testId).toBe(101);

      // Emitted event value should exactly equal by instance the resulting value on `model`.
      expect(onUpdateObject.mock.calls[0][0][0]).toBe(selectedItem);
      expect(onSelectionChanged.mock.calls[0][0][0]).toBe(selectedItem);
    });

    test.each(["menu", "chip"])(
      "deselect by %s click emits existing ViewModel",
      async (method) => {
        // Arrange
        const model = new ComplexModelViewModel({ singleTestId: 303 });
        const onSelectionChanged = vitest.fn();
        const wrapper = mountApp(() => (
          <CSelect
            for="Test"
            multiple
            onUpdate:modelValue={(v) => (model.tests = v)}
            modelValue={model.tests}
            onSelectionChanged={onSelectionChanged}
          ></CSelect>
        )).findComponent(CSelect);

        // Act
        await selectFirstResult(wrapper);
        const selectedItem = model.tests[0];

        method == "menu"
          ? await deselectMenuResult(wrapper, 0)
          : await deselectChipResult(wrapper, 0);

        // Assert
        // Emitted event value should exactly equal by instance the resulting value on `model`.
        expect(onSelectionChanged.mock.calls[0][0][0]).toBe(selectedItem);
        expect(onSelectionChanged.mock.calls[1][0][0]).toBe(selectedItem);
        expect(selectedItem).toBeInstanceOf(TestViewModel);
      },
    );

    test("collectionNavigation binding sets inverse nav on select", async () => {
      // Arrange
      const model = new ComplexModelViewModel().$loadCleanData({
        complexModelId: 1,
      });

      const wrapper = mountApp(() => (
        <CSelect model={model} for="tests"></CSelect>
      )).findComponent(CSelect);

      // Act
      await selectFirstResult(wrapper);

      // Assert
      expect(model.tests).toHaveLength(1);
      const added = model.tests[0];
      expect(added.complexModel).toBe(model);
      expect(added.complexModelId).toBe(model.complexModelId);

      expect(model.$bulkSavePreview().items).toEqual([
        {
          action: "none",
          data: { complexModelId: 1 },
          refs: { complexModelId: model.$stableId },
          root: true,
          type: "ComplexModel",
        },
        {
          action: "save",
          data: { complexModelId: 1, testId: 101 },
          refs: { testId: added.$stableId },
          type: "Test",
        },
      ]);
    });

    test("collectionNavigation clears inverse nav on deselect", async () => {
      // Arrange
      const model = new ComplexModelViewModel().$loadCleanData({
        complexModelId: 1,
        tests: [{ testId: 101, testName: "foo 101", complexModelId: 2 }],
      });

      const wrapper = mountApp(() => (
        <CSelect model={model} for="tests"></CSelect>
      )).findComponent(CSelect);

      // Act
      await deselectChipResult(wrapper, 0);

      // Assert
      expect(model.tests).toHaveLength(0);
      const removed = model.$removedItems![0] as TestViewModel;
      expect(removed).not.toBeUndefined();
      expect(removed).toBeInstanceOf(TestViewModel);
      expect(removed.complexModel).toBeNull();
      expect(removed.complexModelId).toBeNull();

      expect(model.$bulkSavePreview().items).toEqual([
        {
          action: "none",
          data: { complexModelId: 1 },
          refs: { complexModelId: model.$stableId },
          root: true,
          type: "ComplexModel",
        },
        {
          action: "save",
          data: { complexModelId: null, testId: 101 },
          refs: { testId: removed.$stableId },
          type: "Test",
        },
      ]);
    });
  });

  describe("interaction", () => {
    test("typing while focused opens search", async () => {
      const wrapper = mountApp(() => (
        <CSelect model={model} for="singleTest"></CSelect>
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
        <CSelect model={model} for="singleTest" clearable></CSelect>
      )).findComponent(CSelect<ComplexModel>);

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

      await delay(400);

      // Open it again
      await mainInput.trigger("keydown.space");
      expect(wrapper.vm.menuOpen).toBeTruthy();

      // Navigate to the second item
      await menuInput.trigger("keydown.down");
      // Pick it
      await menuInput.trigger("keydown.enter");

      expect(menuWrapper.find(".pending-selection").text()).toBe("bar 202");
      expect(model.singleTestId).toBe(202); // second result in the list
      expect(wrapper.vm.menuOpen).toBeFalsy();

      // Clear the selection with delete
      await mainInput.trigger("keydown.delete");
      expect(model.singleTestId).toBeNull();
    });

    test.each([
      {
        position: "start",
        navKey: "up",
        description: "at start of list",
      },
      {
        position: "end",
        navKey: "down",
        description: "at end of list",
      },
    ] as const)(
      "create item is keyboard navigable when $description",
      async ({ position, navKey }) => {
        const wrapper = mountApp(() => (
          <CSelect
            model={model}
            for="singleTest"
            create={{
              getLabel(search: string) {
                if (search == "f") return "Create new item";
                else return false;
              },
              async getItem(search: string, label: string) {
                return new Test({ testName: label });
              },
              position,
            }}
          ></CSelect>
        )).findComponent(CSelect<ComplexModel>);

        const mainInput = wrapper.find("input");

        // Open the menu and set search to show create item
        await mainInput.trigger("keydown.enter");
        expect(wrapper.vm.menuOpen).toBeTruthy();

        await flushPromises();
        const menuWrapper = menuContents();
        const menuInput = menuWrapper.find("input");

        await menuInput.setValue("f");
        await flushPromises();

        // Create item should be present
        const createItem = menuWrapper.find(".c-select__create-item");
        expect(createItem.text()).toContain("Create new item");

        // Navigate to select the create item
        await menuInput.trigger(`keydown.${navKey}`);
        // Select it with Enter
        await menuInput.trigger("keydown.enter");

        expect(model.singleTest?.testName).toBe("Create new item");
        expect(wrapper.vm.menuOpen).toBeFalsy();
      },
    );

    describe.each(["disabled", "readonly"])("%s", (prop) => {
      async function assertNonInteractive(
        wrapper: VueWrapper<
          BetterComponentInstance<typeof CSelect<ComplexModel>>
        >,
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
        model.singleTest = new Test({ testId: 101 });
        const wrapper = mountApp(() => (
          <VForm {...{ [prop]: true }}>
            <CSelect model={model} for="singleTest" clearable></CSelect>
          </VForm>
        )).findComponent(CSelect<ComplexModel>);

        assertNonInteractive(wrapper);
      });
      test("noninteractive via direct", () => {
        model.singleTest = new Test({ testId: 101 });
        const wrapper = mountApp(() => (
          <CSelect
            model={model}
            for="singleTest"
            clearable
            {...{ [prop]: true }}
          ></CSelect>
        )).findComponent(CSelect<ComplexModel>);

        assertNonInteractive(wrapper);
      });

      test("chips not removable in multiple select", async () => {
        const model = new ComplexModelViewModel();
        model.tests = [new Test({ testId: 101, testName: "foo 101" })];
        const wrapper = mountApp(() => (
          <CSelect model={model} for="tests" {...{ [prop]: true }}></CSelect>
        )).findComponent(CSelect);

        // Assert chip close buttons are not present or not clickable
        const closeButtons = wrapper.findAll(".v-chip__close");
        expect(closeButtons).toHaveLength(0);

        // Ensure selection remains unchanged
        expect(model.tests).toHaveLength(1);
        expect(model.tests[0].testId).toBe(101);
      });
    });
  });

  test("preselect first", async () => {
    mountApp(() => (
      <CSelect model={model} for="singleTest" preselect-first></CSelect>
    )).findComponent(CSelect);
    await flushPromises();
    expect(model.singleTestId).toBe(101);
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
          for="singleTest"
          preselect-single
          params={{ pageSize: results }}
        ></CSelect>
      )).findComponent(CSelect);
      await flushPromises();
      expect(model.singleTestId).toBe(expected);
    },
  );

  test("create", async () => {
    const wrapper = mountApp(() => (
      <CSelect
        model={model}
        for="singleTest"
        create={{
          getLabel(search: string) {
            if (search == "f") return "new thing";
            else return false;
          },
          async getItem(search: string, label: string) {
            return new Test({ testName: label });
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
    expect(model.singleTest?.testName).toBe("new thing");
  });

  test("create with deferred save, clears required validation message after save", async () => {
    const vm = new ComplexModelViewModel();

    const saveMock = mockEndpoint(
      "/Test/save",
      vitest.fn(() => ({
        wasSuccessful: true,
        object: { testId: 1, testName: "foo" },
      })),
    );

    const wrapper = mountApp(() => (
      <CSelect
        model={vm}
        for="singleTest"
        validateOn="eager"
        create={{
          getLabel(search: string) {
            return search;
          },
          async getItem(search: string, label: string) {
            return new TestViewModel({
              testName: label,
              complexModelId: 42,
            });
          },
        }}
      ></CSelect>
    )).findComponent(CSelect);

    const menuWrapper = await openMenu(wrapper);
    await menuWrapper.find("input").setValue("foo");

    // Select the new item entry.
    const createItem = menuWrapper.find(".c-select__create-item");
    await createItem.trigger("click");

    // Item still unsaved, validation message should be present.
    expect(wrapper.text()).toContain("Single Test is required");

    await delay(10);
    vm.singleTest!.$save();
    await delay(10);

    expect(wrapper.text()).not.toContain("Single Test is required");
  });

  describe("vuetify props passthrough", () => {
    beforeEach(() => {
      mockEndpoint("/Person/list", () => ({
        wasSuccessful: true,
        list: [],
      }));
    });
    test("singleLine", async () => {
      const model = new CaseViewModel();
      const wrapper = mountApp(() => (
        <CSelect model={model} for="assignedTo" singleLine></CSelect>
      ));

      // Assert
      expect(wrapper.findAll(".v-field--single-line")).toHaveLength(1);
    });

    test("single-line", async () => {
      const model = new CaseViewModel();
      const wrapper = mountApp(() => (
        <CSelect model={model} for="assignedTo" single-line></CSelect>
      ));

      // Assert
      expect(wrapper.findAll(".v-field--single-line")).toHaveLength(1);
    });
  });

  // test("BOILERPLATE", () => {
  //   const appWrapper = mountApp(CSelect, {});
  //   const wrapper = appWrapper.findComponent(CSelect);
  // });
});

const menuContents = () => getWrapper(".v-overlay__content");

async function selectFirstResult(wrapper: VueWrapper) {
  const overlay = await openMenu(wrapper);
  await overlay.find(".v-list-item").trigger("click");
}
async function deselectChipResult(wrapper: VueWrapper, idx: number) {
  await wrapper.findAll(".v-chip__close")[idx].trigger("click");
}
async function deselectMenuResult(wrapper: VueWrapper, idx: number) {
  const overlay = await openMenu(wrapper);
  await overlay.findAll(".v-list-item--active")[idx].trigger("click");
}
