import { delay, mount, mountApp, openMenu } from "@test/util";
import { CDatetimePicker } from "..";
import { Case, ComplexModel } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { AnyArgCaller } from "coalesce-vue";
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
    expect(overlay.text()).contains("August 1970");
    expect(overlay.text()).contains("Sun, Aug 2");
    expect(overlay.find(".c-time-picker-header").text()).equals("1:51 PM PDT");
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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );
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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );
      const minuteColumn = overlay.find(
        '.c-time-picker__column-minute',
      );
      const meridiemColumn = overlay.find(
        '.c-time-picker__column-meridiem',
      );

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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );
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
      const meridiemColumn = overlay.find(
        '.c-time-picker__column-meridiem',
      );

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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );
      (hourColumn.element as HTMLElement).focus();

      await hourColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(13); // 12 PM -> 1 PM

      await hourColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getHours()).toBe(12); // back to 12 PM

      // Test minute column navigation
      const minuteColumn = overlay.find(
        '.c-time-picker__column-minute',
      );
      (minuteColumn.element as HTMLElement).focus();

      await minuteColumn.trigger("keydown", { key: "ArrowDown" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(31);

      await minuteColumn.trigger("keydown", { key: "ArrowUp" });
      await delay(1);
      expect(model.dateTime?.getMinutes()).toBe(30);

      // Test meridiem column navigation
      const meridiemColumn = overlay.find(
        '.c-time-picker__column-meridiem',
      );
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
      const meridiemColumn = overlay.find(
        '.c-time-picker__column-meridiem',
      );

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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );
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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );

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
      const hourColumn = overlay.find(
        '.c-time-picker__column-hour',
      );

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
      const minuteColumn = overlay.find(
        '.c-time-picker__column-minute',
      );

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
});

