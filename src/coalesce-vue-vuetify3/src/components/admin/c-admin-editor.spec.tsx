import { Person } from "@test-targets/models.g";
import { CAdminEditor } from "..";
import {
  PersonViewModel,
  PersonListViewModel,
  OneToOneParentViewModel,
  OneToOneSharedKeyChild1ViewModel,
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
});
