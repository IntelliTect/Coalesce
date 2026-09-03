import {
  delay,
  mount,
  mountApp,
  openMenu,
  getWrapper,
  flushPromises,
} from "@test/util";
import { CDatetimePicker } from "..";
import { Case, ComplexModel } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { AnyArgCaller } from "coalesce-vue";
import { ref } from "vue";
import { VForm } from "vuetify/components";

describe("CDatetimePicker", () => {
  let model: ComplexModelViewModel;
  beforeEach(() => {
    model = new ComplexModelViewModel({
      name: "bob",
    });
  });

  test("types", () => {
    const model = new ComplexModel();
    const vm = new ComplexModelViewModel();
    const ds = new Case.DataSources.AllOpenCases();

    const selectedDate = new Date();

    () => <CDatetimePicker model={vm} for="systemDateOnly" />;
    () => <CDatetimePicker model={model} for="systemDateOnly" />;
    () => (
      <CDatetimePicker
        model={model}
        for={vm.$metadata.props.dateTimeNullable}
      />
    );
    () => <CDatetimePicker model={model as any} for="systemDateOnly" />;

    //@ts-expect-error non-existent prop
    () => <CDatetimePicker model={vm} for="asdf" />;
    //@ts-expect-error non-date prop
    () => <CDatetimePicker model={vm} for="long" />;

    // Against models that might be null
    () => (
      <CDatetimePicker model={vm.referenceNavigation} for="systemDateOnly" />
    );

    () => (
      <CDatetimePicker for="ComplexModel.dateTime" modelValue={selectedDate} />
    );
    () => (
      <CDatetimePicker
        for={vm.$metadata.props.dateTimeNullable}
        modelValue={selectedDate}
      />
    );

    () => <CDatetimePicker modelValue={selectedDate} />;
    //@ts-expect-error wrong value type
    () => <CDatetimePicker modelValue={selectedDate as string} />;

    () => <CDatetimePicker modelValue={selectedDate} openOn="picker-only" />;
    //@ts-expect-error not an activation mode
    () => <CDatetimePicker modelValue={selectedDate} openOn="whenever" />;
    () => (
      <CDatetimePicker
        modelValue={selectedDate}
        menu={true}
        onUpdate:menu={(v: boolean) => {}}
      />
    );

    // *****
    // API caller args
    // *****
    const caller = vm.methodWithManyParams;
    () => <CDatetimePicker model={caller} for="dateTime" />;
    () => <CDatetimePicker model={caller as AnyArgCaller} for="dateTime" />;
    () => <CDatetimePicker model={caller as AnyArgCaller} for="anyString" />;
    //@ts-expect-error non-existent param
    () => <CDatetimePicker model={caller} for="asdf" />;
    //@ts-expect-error non-date param
    () => <CDatetimePicker model={caller} for="integer" />;

    // *****
    // Data source parameters
    // *****
    () => <CDatetimePicker model={ds} for="minDate" />;
    //@ts-expect-error invalid param
    () => <CDatetimePicker model={ds} for="asdf" />;

    // *****
    // Vuetify props
    // *****
    () => <CDatetimePicker model={ds} for="minDate" variant="filled" />;
    //@ts-expect-error variant doesn't exist
    () => <CDatetimePicker model={ds} for="minDate" variant="bad-variant" />;

    // *****
    // datePickerProps
    // *****
    () => (
      <CDatetimePicker
        model={ds}
        for="minDate"
        datePickerProps={{
          weeksInMonth: "dynamic",
        }}
      />
    );
    //@ts-expect-error invalid datePickerProps
    () => <CDatetimePicker datePickerProps={{ weeksInMonth: "invalid" }} />;

    // *****
    // Rules
    // *****
    // Good rules
    const dateRule = (date: Date | null | undefined) => !!date || "test";
    () => <CDatetimePicker model={ds} for="minDate" rules={[dateRule]} />;

    // Bad rules
    const badDateRule = (date: string | null | undefined) => !!date || "test";
    //@ts-expect-error rules must be array
    () => <CDatetimePicker model={ds} for="minDate" rules={dateRule} />;
    //@ts-expect-error invalid rules
    () => <CDatetimePicker model={ds} for="minDate" rules={[badDateRule]} />;
  });

  test("disabled inherits from form", async () => {
    const wrapper = mount(() => (
      <VForm disabled>
        <CDatetimePicker />
      </VForm>
    ));

    expect(wrapper.find("input").element.disabled).toBeTruthy();
  });

  test("readonly inherits from form", async () => {
    const wrapper = mount(() => (
      <VForm readonly>
        <CDatetimePicker />
      </VForm>
    ));

    expect(wrapper.find("input").element.readOnly).toBeTruthy();
  });

  test("opens picker menu", async () => {
    const date = new Date(18478289085);
    const wrapper = mountApp(() => (
      <CDatetimePicker modelValue={date} timeZone="America/Los_Angeles" />
    )).findComponent(CDatetimePicker);

    const overlay = await openMenu(wrapper);

    expect(overlay.findAll(".c-time-picker__item-active")).toHaveLength(3);
    expect(overlay.text()).contains("Aug");
    expect(overlay.text()).contains("1970");
    expect(overlay.text()).contains("Sun, Aug 2");
    expect(overlay.find(".c-time-picker-header").text()).equals("1:51 PM PDT");
  });

  test("clicking hint text does not open picker menu", async () => {
    const date = new Date(18478289085);
    const wrapper = mountApp(() => (
      <CDatetimePicker
        modelValue={date}
        timeZone="America/Los_Angeles"
        hint="Some hint text"
        persistent-hint
      />
    )).findComponent(CDatetimePicker);

    await flushPromises();
    await wrapper.find(".v-input__details").trigger("click");
    await flushPromises();

    expect(document.querySelector(".v-overlay__content")).toBeNull();
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CDatetimePicker model={model.methodWithManyParams} for="dateTime" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Date Time");

    // Set a value, and look for the value
    model.methodWithManyParams.args.dateTime = new Date("2023-08-16T01:02:03Z");
    await delay(1);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(model.methodWithManyParams.args.dateTime.getFullYear()).toBe(2017);
  });

  test("validation rules are passed date, not string", async () => {
    const rule = vitest.fn(
      (v) => !v || v.getFullYear() > 2017 || "Year must be > 2017",
    );

    const wrapper = mount(() => (
      <CDatetimePicker model={model} for="systemDateOnly" rules={[rule]} />
    ));

    // Perform an input on the component, and then look at the args that were passed to the rule function:
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(wrapper.text()).toContain("Year must be > 2017");
    expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    expect(rule).toHaveBeenLastCalledWith(model.systemDateOnly);

    // Do it again, but with a valid input this time. The error should be gone.
    await wrapper.find("input").setValue("1/3/2018");
    await delay(1);
    expect(wrapper.text()).not.toContain("Year must be > 2017");
    expect(model.systemDateOnly?.getFullYear()).toBe(2018);
    expect(rule).toHaveBeenLastCalledWith(model.systemDateOnly);
  });

  test("pressing tab closes menu and updates bound value", async () => {
    const wrapper = mountApp(() => (
      <CDatetimePicker model={model} for="systemDateOnly" />
    )).findComponent(CDatetimePicker);

    // Open the menu by clicking on the input field
    const overlay = await openMenu(wrapper);
    expect(overlay.exists()).toBeTruthy();

    // Set a value in the text input
    const input = wrapper.find("input");
    await input.setValue("1/15/2024");
    await delay(1);

    // Press tab to close the menu
    await input.trigger("keydown.tab");
    await delay(1);

    // Menu should be closed - check that overlay content is no longer visible
    const menuContent = document.querySelector(
      ".v-overlay__content",
    ) as HTMLElement;
    expect(menuContent?.style.display).toBe("none");

    // Value should be updated
    expect(model.systemDateOnly?.getFullYear()).toBe(2024);
    expect(model.systemDateOnly?.getMonth()).toBe(0); // January is 0
    expect(model.systemDateOnly?.getDate()).toBe(15);
  });

  describe("date picker keyboard navigation", () => {
    test("arrow keys navigate dates in date picker", async () => {
      model.dateTime = new Date("2024-01-15T12:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const datePicker = overlay.find(".v-date-picker");

      // Arrow Right - next day
      await datePicker.trigger("keydown", { key: "ArrowRight" });
      await delay(1);
      expect(model.dateTime?.getDate()).toBe(16);

      // Arrow Left - previous day
      await datePicker.trigger("keydown", { key: "ArrowLeft" });
      await delay(1);
      expect(model.dateTime?.getDate()).toBe(15);

      // Arrow Down - next week
      await datePicker.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getDate()).toBe(22);

      // Arrow Up - previous week
      await datePicker.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getDate()).toBe(15);
    });

    test("tab from date picker focuses time picker hour column", async () => {
      model.dateTime = new Date("2024-01-15T12:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const datePicker = overlay.find(".v-date-picker");

      // Tab from date picker
      await datePicker.trigger("keydown", { key: "Tab" });
      await delay(1);

      // Check that the hour column is focused
      const hourColumn = overlay.find(".c-time-picker__column-hour");
      expect(document.activeElement).toBe(hourColumn.element);
    });

    test("shift+tab from date picker closes menu", async () => {
      model.dateTime = new Date("2024-01-15T12:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const datePicker = overlay.find(".v-date-picker");

      // Shift+Tab from date picker
      await datePicker.trigger("keydown", { key: "Tab", shiftKey: true });
      await delay(1);

      // Menu should be closed
      const menuContent = document.querySelector(
        ".v-overlay__content",
      ) as HTMLElement;
      expect(menuContent?.style.display).toBe("none");
    });

    test("tab from date-only picker closes menu", async () => {
      model.systemDateOnly = new Date("2024-01-15T00:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const datePicker = overlay.find(".v-date-picker");

      // Tab from date picker when there's no time picker
      await datePicker.trigger("keydown", { key: "Tab" });
      await delay(1);

      // Menu should be closed
      const menuContent = document.querySelector(
        ".v-overlay__content",
      ) as HTMLElement;
      expect(menuContent?.style.display).toBe("none");
    });
  });

  describe("time picker keyboard navigation", () => {
    test("arrow left/right navigates between time picker columns", async () => {
      model.dateTime = new Date("2024-01-15T12:30:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const hourColumn = overlay.find(".c-time-picker__column-hour");
      const minuteColumn = overlay.find(".c-time-picker__column-minute");
      const meridiemColumn = overlay.find(".c-time-picker__column-meridiem");

      // Start at hour column
      (hourColumn.element as HTMLElement).focus();
      expect(document.activeElement).toBe(hourColumn.element);

      // Arrow Right - to minute column
      await hourColumn.trigger("keydown", { key: "ArrowRight" });
      await delay(1);
      expect(document.activeElement).toBe(minuteColumn.element);

      // Arrow Right - to meridiem column
      await minuteColumn.trigger("keydown", { key: "ArrowRight" });
      await delay(1);
      expect(document.activeElement).toBe(meridiemColumn.element);

      // Arrow Left - back to minute column
      await meridiemColumn.trigger("keydown", { key: "ArrowLeft" });
      await delay(1);
      expect(document.activeElement).toBe(minuteColumn.element);

      // Arrow Left - back to hour column
      await minuteColumn.trigger("keydown", { key: "ArrowLeft" });
      await delay(1);
      expect(document.activeElement).toBe(hourColumn.element);
    });

    test("arrow left from hour column navigates to date picker", async () => {
      model.dateTime = new Date("2024-01-15T12:30:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const hourColumn = overlay.find(".c-time-picker__column-hour");
      const datePicker = overlay.find(".v-date-picker");

      (hourColumn.element as HTMLElement).focus();

      // Arrow Left from hour column
      await hourColumn.trigger("keydown", { key: "ArrowLeft" });
      await delay(1);

      // Should focus the date picker
      expect(document.activeElement).toBe(datePicker.element);
    });

    test("arrow right from meridiem column closes menu", async () => {
      model.dateTime = new Date("2024-01-15T12:30:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const meridiemColumn = overlay.find(".c-time-picker__column-meridiem");

      (meridiemColumn.element as HTMLElement).focus();

      // Arrow Right from meridiem column
      await meridiemColumn.trigger("keydown", { key: "ArrowRight" });
      await delay(1);

      // Menu should be closed
      const menuContent = document.querySelector(
        ".v-overlay__content",
      ) as HTMLElement;
      expect(menuContent?.style.display).toBe("none");
    });

    test("arrow up/down navigates items within time picker columns", async () => {
      model.dateTime = new Date("2024-01-15 12:30:00"); // Using local time
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);

      // Test hour column navigation
      const hourColumn = overlay.find(".c-time-picker__column-hour");
      (hourColumn.element as HTMLElement).focus();

      await hourColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(13); // 12 PM -> 1 PM

      await hourColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(12); // back to 12 PM

      // Test minute column navigation
      const minuteColumn = overlay.find(".c-time-picker__column-minute");
      (minuteColumn.element as HTMLElement).focus();

      await minuteColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(31);

      await minuteColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(30);

      // Test meridiem column navigation
      const meridiemColumn = overlay.find(".c-time-picker__column-meridiem");
      (meridiemColumn.element as HTMLElement).focus();

      await meridiemColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(0); // 12 PM -> 12 AM

      await meridiemColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(12); // back to 12 PM
    });

    test("tab from meridiem column closes menu", async () => {
      model.dateTime = new Date("2024-01-15T12:30:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const meridiemColumn = overlay.find(".c-time-picker__column-meridiem");

      (meridiemColumn.element as HTMLElement).focus();

      // Tab from meridiem column
      await meridiemColumn.trigger("keydown", { key: "Tab" });
      await delay(1);

      // Menu should be closed
      const menuContent = document.querySelector(
        ".v-overlay__content",
      ) as HTMLElement;
      expect(menuContent?.style.display).toBe("none");
    });

    test("shift+tab from hour column navigates to date picker", async () => {
      model.dateTime = new Date("2024-01-15T12:30:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const hourColumn = overlay.find(".c-time-picker__column-hour");
      const datePicker = overlay.find(".v-date-picker");

      (hourColumn.element as HTMLElement).focus();

      // Shift+Tab from hour column
      await hourColumn.trigger("keydown", { key: "Tab", shiftKey: true });
      await delay(1);

      // Should focus the date picker
      expect(document.activeElement).toBe(datePicker.element);
    });

    test("shift+tab from hour column in time-only picker closes menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" dateKind="time" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const hourColumn = overlay.find(".c-time-picker__column-hour");

      (hourColumn.element as HTMLElement).focus();

      // Shift+Tab from hour column when there's no date picker
      await hourColumn.trigger("keydown", { key: "Tab", shiftKey: true });
      await delay(1);

      // Menu should be closed
      const menuContent = document.querySelector(
        ".v-overlay__content",
      ) as HTMLElement;
      expect(menuContent?.style.display).toBe("none");
    });

    test("arrow navigation wraps around at boundaries", async () => {
      model.dateTime = new Date("2024-01-15 01:00:00"); // 1 AM, using local time
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const hourColumn = overlay.find(".c-time-picker__column-hour");

      (hourColumn.element as HTMLElement).focus();

      // Arrow Up from 1 should wrap to 12
      await hourColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(0); // 12 AM

      // Arrow Down from 12 should wrap to 1
      await hourColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(1); // 1 AM
    });

    test("arrow navigation in minute column with step prop", async () => {
      model.dateTime = new Date("2024-01-15T12:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" step={15} />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      const minuteColumn = overlay.find(".c-time-picker__column-minute");

      (minuteColumn.element as HTMLElement).focus();

      // Arrow Down should jump by the first available minute (0 -> 15)
      await minuteColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(15);

      // Arrow Down again (15 -> 30)
      await minuteColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(30);

      // Arrow Up (30 -> 15)
      await minuteColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(15);
    });
  });

  describe("lazy", () => {
    test("publishes each parseable keystroke when not lazy", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" />
      ));

      // A partially typed year still parses, so it reaches the bound value.
      await wrapper.find("input").setValue("6/6/202");
      await delay(1);
      expect(model.systemDateOnly).toBeTruthy();
      expect(model.systemDateOnly!.getFullYear()).not.toBe(2026);
    });

    test("defers text input until blur", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy />
      ));

      const input = wrapper.find("input");
      await input.trigger("focus");

      await input.setValue("6/6/202");
      await delay(1);
      expect(model.systemDateOnly).toBeFalsy();

      await input.setValue("6/6/2026");
      await delay(1);
      expect(model.systemDateOnly).toBeFalsy();
      // The text field keeps showing exactly what was typed.
      expect(input.element.value).toBe("6/6/2026");

      await input.trigger("blur");
      await delay(1);
      expect(model.systemDateOnly?.getFullYear()).toBe(2026);
      expect(model.systemDateOnly?.getMonth()).toBe(5);
      expect(model.systemDateOnly?.getDate()).toBe(6);
    });

    test("commits on enter", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy />
      ));

      const input = wrapper.find("input");
      await input.setValue("1/3/2017");
      await delay(1);
      expect(model.systemDateOnly).toBeFalsy();

      await input.trigger("keydown.enter");
      await delay(1);
      expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    });

    test("commits on tab", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy />
      ));

      const input = wrapper.find("input");
      await input.setValue("1/3/2017");
      await delay(1);
      expect(model.systemDateOnly).toBeFalsy();

      await input.trigger("keydown.tab");
      await delay(1);
      expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    });

    test("defers clearing the text until blur", async () => {
      model.systemDateOnly = new Date("2024-01-15T00:00:00");
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy />
      ));

      const input = wrapper.find("input");
      await input.trigger("focus");
      await input.setValue("");
      await delay(1);
      expect(model.systemDateOnly).toBeTruthy();

      await input.trigger("blur");
      await delay(1);
      expect(model.systemDateOnly).toBeNull();
    });

    test("commits immediately when the clear icon is clicked", async () => {
      model.systemDateOnly = new Date("2024-01-15T00:00:00");
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy clearable />
      ));

      await wrapper.find(".v-field__clearable .v-icon").trigger("click");
      await delay(1);
      expect(model.systemDateOnly).toBeNull();
    });

    test("publishes date picker selections immediately", async () => {
      model.dateTime = new Date("2024-01-15T12:00:00Z");
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="dateTime" lazy />
      )).findComponent(CDatetimePicker);

      const overlay = await openMenu(wrapper);
      await overlay.find(".v-date-picker").trigger("keydown", {
        key: "ArrowRight",
      });
      await delay(1);
      expect(model.dateTime?.getDate()).toBe(16);
    });

    test("does not publish unparseable text on blur", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy />
      ));

      const input = wrapper.find("input");
      await input.trigger("focus");
      await input.setValue("not a date");
      await input.trigger("blur");
      await delay(1);

      expect(model.systemDateOnly).toBeFalsy();
      expect(wrapper.text()).toContain("Invalid value");
      // The invalid text is retained so it can be corrected.
      expect(input.element.value).toBe("not a date");
    });

    test("honors the lazy modifier on v-model", async () => {
      const value = ref<Date | null>(null);
      const wrapper = mount(() => (
        <CDatetimePicker
          modelValue={value.value}
          onUpdate:modelValue={(v: Date | null | undefined) =>
            (value.value = v ?? null)
          }
          {...({ modelModifiers: { lazy: true } } as any)}
        />
      ));

      const input = wrapper.find("input");
      await input.trigger("focus");
      await input.setValue("1/3/2017 5:00 PM");
      await delay(1);
      expect(value.value).toBeNull();

      await input.trigger("blur");
      await delay(1);
      expect(value.value?.getFullYear()).toBe(2017);
    });

    test("native inputs are unaffected", async () => {
      const wrapper = mount(() => (
        <CDatetimePicker model={model} for="systemDateOnly" lazy native />
      ));

      const input = wrapper.find("input");
      input.element.value = "2017-01-03";
      await input.trigger("change");
      await delay(1);

      expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    });
  });

  describe("openOn", () => {
    /** The overlay element persists after closing, so its visibility is what tells them apart. */
    function menuState() {
      const el = document.querySelector(".v-overlay__content") as HTMLElement;
      if (!el) return "absent";
      return el.style.display == "none" ? "closed" : "open";
    }

    test("field: clicking the field opens the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="field" />
      )).findComponent(CDatetimePicker);

      await openMenu(wrapper);
      expect(menuState()).toBe("open");
    });

    test("field: the icon is not a tab stop", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      expect(
        wrapper.find(".v-field__append-inner .v-icon").attributes("tabindex"),
      ).toBeUndefined();
    });

    test("icon: clicking the field does not open the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="icon" />
      )).findComponent(CDatetimePicker);

      await openMenu(wrapper);
      expect(menuState()).toBe("absent");
      expect(wrapper.find("input").element.readOnly).toBe(false);
    });

    test("icon: clicking the icon toggles the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="icon" />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      const icon = wrapper.find(".v-field__append-inner .v-icon");
      // The click handler also makes the icon reachable by keyboard.
      expect(icon.attributes("tabindex")).toBe("0");

      await icon.trigger("click");
      await flushPromises();
      expect(menuState()).toBe("open");

      await icon.trigger("click");
      await flushPromises();
      expect(menuState()).toBe("closed");
    });

    test("focus: focusing the field opens the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="focus" />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      await wrapper.find("input").trigger("focus");
      await flushPromises();
      expect(menuState()).toBe("open");
    });

    test("focus: closing the menu does not reopen it", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="focus" />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      const input = wrapper.find("input");
      await input.trigger("focus");
      await flushPromises();

      // Simulate the menu having taken focus, as it does when opened.
      await input.trigger("blur");
      await flushPromises();

      // Closing returns focus to the field, which must not open it again.
      await getWrapper(".c-datetime-picker__close-btn").trigger("click");
      await flushPromises();
      expect(menuState()).toBe("closed");
    });

    test("picker-only: the field cannot be typed into", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker
          model={model}
          for="systemDateOnly"
          openOn="picker-only"
        />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      expect(wrapper.find("input").element.readOnly).toBe(true);
    });

    test("picker-only: clicking the field toggles the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker
          model={model}
          for="systemDateOnly"
          openOn="picker-only"
        />
      )).findComponent(CDatetimePicker);

      await openMenu(wrapper);
      expect(menuState()).toBe("open");

      await wrapper.find(".v-field").trigger("click");
      await flushPromises();
      expect(menuState()).toBe("closed");
    });

    test("none: nothing in the component opens the menu", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" openOn="none" />
      )).findComponent(CDatetimePicker);

      await openMenu(wrapper);
      expect(menuState()).toBe("absent");

      const input = wrapper.find("input");
      await input.trigger("keydown.down");
      await flushPromises();
      expect(menuState()).toBe("absent");

      // Nothing the user can expand, so the combobox attributes are dropped.
      expect(input.attributes("role")).toBeUndefined();
      expect(input.attributes("aria-expanded")).toBeUndefined();
      expect(input.attributes("aria-controls")).toBeUndefined();

      // Text entry still works.
      await input.setValue("1/3/2017");
      await delay(1);
      expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    });

    test("none: v-model:menu still opens the menu", async () => {
      const menu = ref(false);
      mountApp(() => (
        <CDatetimePicker
          model={model}
          for="systemDateOnly"
          openOn="none"
          menu={menu.value}
          onUpdate:menu={(v: boolean) => (menu.value = v)}
        />
      ));
      await flushPromises();

      menu.value = true;
      await flushPromises();
      expect(menuState()).toBe("open");
    });

    test("readonly fields have no menu to advertise", async () => {
      const wrapper = mountApp(() => (
        <CDatetimePicker model={model} for="systemDateOnly" readonly />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      const input = wrapper.find("input");
      expect(input.attributes("role")).toBeUndefined();
      expect(input.attributes("aria-expanded")).toBeUndefined();
      expect(input.attributes("aria-controls")).toBeUndefined();
    });

    describe.each(["field", "icon", "focus", "picker-only"] as const)(
      "%s",
      (openOn) => {
        test("arrow down opens the menu, escape closes it", async () => {
          const wrapper = mountApp(() => (
            <CDatetimePicker
              model={model}
              for="systemDateOnly"
              openOn={openOn}
            />
          )).findComponent(CDatetimePicker);
          await flushPromises();

          const input = wrapper.find("input");
          expect(input.attributes("aria-expanded")).toBe("false");

          await input.trigger("keydown.down");
          await flushPromises();
          expect(menuState()).toBe("open");
          expect(input.attributes("aria-expanded")).toBe("true");

          await input.trigger("keydown", { key: "Escape" });
          await flushPromises();
          expect(menuState()).toBe("closed");
        });

        test("arrow down hands the popup focus", async () => {
          const wrapper = mountApp(() => (
            <CDatetimePicker
              model={model}
              for="systemDateOnly"
              openOn={openOn}
            />
          )).findComponent(CDatetimePicker);
          await flushPromises();

          await wrapper.find("input").trigger("keydown.down");
          await delay(100);

          expect(
            document.activeElement?.closest(".v-date-picker"),
          ).toBeTruthy();

          // And back out again, with focus returned to the field.
          await getWrapper(".v-overlay__content").trigger("keydown", {
            key: "Escape",
          });
          await flushPromises();
          expect(menuState()).toBe("closed");
          expect(document.activeElement).toBe(wrapper.find("input").element);
        });

        test("arrow down hands a time-only popup focus", async () => {
          const wrapper = mountApp(() => (
            <CDatetimePicker
              model={model}
              for="systemDateOnly"
              dateKind="time"
              openOn={openOn}
            />
          )).findComponent(CDatetimePicker);
          await flushPromises();

          await wrapper.find("input").trigger("keydown.down");
          await delay(100);

          expect(document.activeElement?.className).toContain(
            "c-time-picker__column-hour",
          );
        });

        test("arrow up opens the menu", async () => {
          const wrapper = mountApp(() => (
            <CDatetimePicker
              model={model}
              for="systemDateOnly"
              openOn={openOn}
            />
          )).findComponent(CDatetimePicker);
          await flushPromises();

          await wrapper.find("input").trigger("keydown.up");
          await flushPromises();
          expect(menuState()).toBe("open");
        });
      },
    );

    test("v-model:menu reflects and controls the menu", async () => {
      const menu = ref(false);
      const wrapper = mountApp(() => (
        <CDatetimePicker
          model={model}
          for="systemDateOnly"
          menu={menu.value}
          onUpdate:menu={(v: boolean) => (menu.value = v)}
        />
      )).findComponent(CDatetimePicker);
      await flushPromises();

      // Opened by the consumer
      menu.value = true;
      await flushPromises();
      expect(menuState()).toBe("open");

      // Closed by the component
      await getWrapper(".c-datetime-picker__close-btn").trigger("click");
      await flushPromises();
      expect(menu.value).toBe(false);
      expect(menuState()).toBe("closed");

      // Opened by the component
      await wrapper.find(".v-field").trigger("click");
      await flushPromises();
      expect(menu.value).toBe(true);
    });
  });
});
