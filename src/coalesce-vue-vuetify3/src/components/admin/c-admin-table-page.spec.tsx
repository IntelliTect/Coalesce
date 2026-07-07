import { CAdminTablePage } from "..";
import { PersonListViewModel } from "@test-targets/viewmodels.g";
import $metadata from "@test-targets/metadata.g";
import { defineComponent, h } from "vue";
import { mockEndpoint, mountWithCoalesceOptions } from "@test/util";

describe("CAdminTablePage", () => {
  beforeEach(() => {
    mockEndpoint("/Person/list", () => ({
      wasSuccessful: true,
      list: [],
      page: 1,
      pageCount: 0,
      pageSize: 10,
      totalCount: 0,
    }));
  });

  describe("adminExtensions - tablePageHeader", () => {
    test("renders tablePageHeader extension", async () => {
      const HeaderExtension = defineComponent({
        name: "TablePageHeader",
        props: { list: { type: Object, required: true } },
        setup() {
          return () =>
            h("div", { class: "table-page-header-ext" }, "page header");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTablePage list={list} />,
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { tablePageHeader: HeaderExtension }],
          ],
        },
      );

      expect(wrapper.find(".table-page-header-ext").exists()).toBeTruthy();
      expect(wrapper.find(".table-page-header-ext").text()).toBe("page header");
    });

    test("tablePageHeader extension receives list prop", async () => {
      let receivedList: any = null;

      const HeaderExtension = defineComponent({
        name: "HeaderPropsInspector",
        props: { list: { type: Object, required: true } },
        setup(props) {
          receivedList = props.list;
          return () => h("div", { class: "header-props-ext" });
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      mountWithCoalesceOptions(
        () => <CAdminTablePage list={list} />,
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { tablePageHeader: HeaderExtension }],
          ],
        },
      );

      expect(receivedList).toBe(list);
    });

    test("global '*' tablePageHeader renders when no type-specific override", async () => {
      const GlobalHeaderExtension = defineComponent({
        name: "GlobalTablePageHeader",
        props: { list: { type: Object, required: true } },
        setup() {
          return () => h("div", { class: "global-table-header-ext" }, "global");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTablePage list={list} />,
        undefined,
        {
          adminExtensions: [["*", { tablePageHeader: GlobalHeaderExtension }]],
        },
      );

      expect(wrapper.find(".global-table-header-ext").exists()).toBeTruthy();
    });

    test("page-header slot replaces extension component", async () => {
      const ExtensionComponent = defineComponent({
        name: "ShouldNotRender",
        props: { list: { type: Object, required: true } },
        setup() {
          return () => h("div", { class: "extension-component" });
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => (
          <CAdminTablePage list={list}>
            {{
              "page-header": () =>
                h("div", { class: "slot-content" }, "Custom Header"),
            }}
          </CAdminTablePage>
        ),
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { tablePageHeader: ExtensionComponent }],
          ],
        },
      );

      expect(wrapper.find(".slot-content").exists()).toBeTruthy();
      expect(wrapper.find(".extension-component").exists()).toBeFalsy();
    });
  });
});
