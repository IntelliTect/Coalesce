import { Person } from "@test-targets/models.g";
import { CAdminEditor } from "..";
import {
  PersonViewModel,
  PersonListViewModel,
  OneToOneParentViewModel,
  OneToOneSharedKeyChild1ViewModel,
  ZipCodeViewModel,
} from "@test-targets/viewmodels.g";
import { mockEndpoint, mount } from "@test/util";

describe("CAdminEditor", () => {
  test("types", () => {
    const model = new Person();
    const vm = new PersonViewModel();
    const list = new PersonListViewModel();

    () => <CAdminEditor model={vm} />;
    //@ts-expect-error plain model not allowed
    () => <CAdminEditor model={model} />;
    //@ts-expect-error list not allowed
    () => <CAdminEditor model={list} />;
  });

  describe("shared-key one-to-one parent", () => {
    mockEndpoint(
      "/OneToOneSharedKeyChild1/get/42",
      vitest.fn(() => ({ wasSuccessful: true })),
    );
    mockEndpoint(
      "/OneToOneSharedKeyChild1/list",
      vitest.fn(() => ({ wasSuccessful: true, list: [] })),
    );

    test("without value renders readonly c-select and link to create", () => {
      const vm = new OneToOneParentViewModel();
      vm.$loadCleanData({ id: 42 });
      vm.$load.wasSuccessful = true;
      const wrapper = mount(() => (
        <CAdminEditor model={vm} props={["sharedKeyChild1"]} />
      ));

      // Find the row for SharedKeyChild1
      const row = wrapper.find(".prop-sharedKeyChild1");
      expect(row.exists()).toBeTruthy();

      // Should contain a readonly c-select
      const select = row.find(".c-select");
      expect(select.exists()).toBeTruthy();
      expect(select.classes()).toContain("v-input--readonly");

      // Should contain a link to the child item with filter for parent's id
      // to allow for creation of the child item
      const link = row.find(".c-admin-editor--ref-nav-link");
      expect(link.exists()).toBeTruthy();
      expect(link.attributes("href")).toBe(
        "/admin/OneToOneSharedKeyChild1/item?filter.parentId=42",
      );
    });

    test("with value renders readonly c-select and link to edit", () => {
      const vm = new OneToOneParentViewModel();
      vm.$loadCleanData({ id: 42, sharedKeyChild1: { parentId: 42 } });
      vm.$load.wasSuccessful = true;
      const wrapper = mount(() => (
        <CAdminEditor model={vm} props={["sharedKeyChild1"]} />
      ));

      // Find the row for SharedKeyChild1
      const row = wrapper.find(".prop-sharedKeyChild1");
      expect(row.exists()).toBeTruthy();

      // Should contain a readonly c-select
      const select = row.find(".c-select");
      expect(select.exists()).toBeTruthy();
      expect(select.classes()).toContain("v-input--readonly");

      // Should contain a link to the child item
      const link = row.find(".c-admin-editor--ref-nav-link");
      expect(link.exists()).toBeTruthy();
      expect(link.attributes("href")).toBe(
        "/admin/OneToOneSharedKeyChild1/item/42",
      );
    });
  });

  describe("shared-key one-to-one child", () => {
    mockEndpoint(
      "/OneToOneParent/get/42",
      vitest.fn(() => ({ wasSuccessful: true, object: { id: 42 } })),
    );
    mockEndpoint(
      "/OneToOneParent/list",
      vitest.fn(() => ({ wasSuccessful: true, list: [] })),
    );

    test.each([
      // Simulate being rendered by c-admin-editor-page with `filter.parentId=42`
      { parentId: 42 },
      // Simulate direct navigation to create page
      {},
    ])("unsaved item renders selectable PK", (initialData) => {
      const vm = new OneToOneSharedKeyChild1ViewModel();
      vm.$loadDirtyData(initialData);

      const wrapper = mount(() => <CAdminEditor model={vm} />);

      // Find the row for Parent
      const row = wrapper.find(".prop-parent");
      expect(row.exists()).toBeTruthy();

      // Should contain an editable c-select
      const select = row.find(".c-select");
      expect(select.exists()).toBeTruthy();
      expect(select.classes()).not.toContain("v-input--readonly");
    });

    test("saved item renders readonly c-select and link to edit", () => {
      const vm = new OneToOneSharedKeyChild1ViewModel();
      vm.$loadCleanData({ parentId: 42, parent: { id: 42 } });

      const wrapper = mount(() => <CAdminEditor model={vm} />);

      // Find the row for Parent
      const row = wrapper.find(".prop-parent");
      expect(row.exists()).toBeTruthy();

      // Should contain a readonly c-select
      const select = row.find(".c-select");
      expect(select.exists()).toBeTruthy();
      expect(select.classes()).toContain("v-input--readonly");

      // Should contain a link to the parent item
      const link = row.find(".c-admin-editor--ref-nav-link");
      expect(link.exists()).toBeTruthy();
      expect(link.attributes("href")).toBe("/admin/OneToOneParent/item/42");
    });
  });

  describe("user-provided PK", () => {
    const saveMock = mockEndpoint("/ZipCode/save", vitest.fn());

    test("create with user-provided PK", async () => {
      const vm = new ZipCodeViewModel();

      const wrapper = mount(() => <CAdminEditor model={vm} />);

      // Find the row for the PK field (zip)
      const zipRow = wrapper.find(".prop-zip");
      expect(zipRow.exists()).toBeTruthy();

      // PK field should be editable for a new item
      const zipInput = zipRow.find("input");
      expect(zipInput.exists()).toBeTruthy();
      expect(zipInput.element.readOnly).toBe(false);

      // Set PK and other field values
      await zipInput.setValue("98052");
      vm.state = "WA";

      // Save the item
      saveMock.mockResolvedValue({
        wasSuccessful: true,
        object: { zip: "98052", state: "WA" },
      });
      await vm.$save();

      expect(JSON.parse(saveMock.mock.calls[0][0].data)).toMatchObject({
        zip: "98052",
        state: "WA",
      });
      expect(vm.$primaryKey).toBe("98052");
      expect(vm.$isDirty).toBeFalsy();
      expect(zipInput.element.readOnly).toBe(true);
    });

    test("update with user-provided PK", async () => {
      const vm = new ZipCodeViewModel();
      vm.$loadCleanData({ zip: "98052", state: "WA" });

      const wrapper = mount(() => <CAdminEditor model={vm} />);

      // Find the row for the PK field (zip)
      const zipRow = wrapper.find(".prop-zip");
      expect(zipRow.exists()).toBeTruthy();

      // PK field should be readonly for an existing item
      const zipInput = zipRow.find("input");
      expect(zipInput.exists()).toBeTruthy();
      expect(zipInput.element.readOnly).toBe(true);

      // Update a non-PK field
      const stateRow = wrapper.find(".prop-state");
      const stateInput = stateRow.find("input");
      await stateInput.setValue("Washington");

      // Save the item
      saveMock.mockResolvedValue({
        wasSuccessful: true,
        object: { zip: "98052", state: "Washington" },
      });
      await vm.$save();

      expect(JSON.parse(saveMock.mock.calls[1][0].data)).toMatchObject({
        zip: "98052",
        state: "Washington",
      });
      expect(vm.$primaryKey).toBe("98052");
      expect(vm.$isDirty).toBeFalsy();
    });
  });
});
