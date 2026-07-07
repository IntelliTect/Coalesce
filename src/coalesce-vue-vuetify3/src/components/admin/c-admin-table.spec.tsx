import { CAdminTable } from "..";
import { PersonListViewModel } from "@test-targets/viewmodels.g";
import $metadata from "@test-targets/metadata.g";
import { defineComponent, h } from "vue";
import { mockEndpoint, mountWithCoalesceOptions } from "@test/util";

describe("CAdminTable", () => {
  beforeEach(() => {
    mockEndpoint("/Person/list", () => ({
      wasSuccessful: true,
      list: [
        { personId: 1, firstName: "Alice", lastName: "A" },
        { personId: 2, firstName: "Bob", lastName: "B" },
      ],
      page: 1,
      pageCount: 1,
      pageSize: 10,
      totalCount: 2,
    }));
  });

  describe("adminExtensions - tableRowActions", () => {
    test("renders tableRowActions extension in each row", async () => {
      const RowExtension = defineComponent({
        name: "RowExtension",
        props: {
          model: { type: Object, required: true },
          list: { type: Object, required: true },
          editable: { type: Boolean, required: true },
        },
        setup(props) {
          return () =>
            h("div", { class: "row-ext" }, `row-${props.model.$primaryKey}`);
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTable list={list} />,
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { tableRowActions: RowExtension }],
          ],
        },
      );

      const rowExts = wrapper.findAll(".row-ext");
      expect(rowExts.length).toBe(2);
      expect(rowExts[0].text()).toBe("row-1");
      expect(rowExts[1].text()).toBe("row-2");
    });

    test("tableRowActions extension receives model, list, and editable props", async () => {
      let receivedModel: any = null;
      let receivedList: any = null;
      let receivedEditable: any = undefined;

      const RowExtension = defineComponent({
        name: "RowPropsInspector",
        props: {
          model: { type: Object, required: true },
          list: { type: Object, required: true },
          editable: { type: Boolean, required: true },
        },
        setup(props) {
          receivedModel = props.model;
          receivedList = props.list;
          receivedEditable = props.editable;
          return () => h("div", { class: "row-props-ext" });
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      mountWithCoalesceOptions(() => <CAdminTable list={list} />, undefined, {
        adminExtensions: [
          [$metadata.types.Person, { tableRowActions: RowExtension }],
        ],
      });

      expect(receivedModel).not.toBeNull();
      expect(receivedList).toBe(list);
      expect(typeof receivedEditable).toBe("boolean");
    });

    test("global '*' tableRowActions renders when no type-specific override", async () => {
      const GlobalRowExtension = defineComponent({
        name: "GlobalRowExtension",
        props: {
          model: { type: Object, required: true },
          list: { type: Object, required: true },
          editable: { type: Boolean, required: true },
        },
        setup() {
          return () => h("div", { class: "global-row-ext" }, "global");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTable list={list} />,
        undefined,
        {
          adminExtensions: [["*", { tableRowActions: GlobalRowExtension }]],
        },
      );

      expect(wrapper.find(".global-row-ext").exists()).toBeTruthy();
    });
  });
});
