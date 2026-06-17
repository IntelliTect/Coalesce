import { CAdminTableToolbar } from "..";
import { PersonListViewModel } from "@test-targets/viewmodels.g";
import $metadata from "@test-targets/metadata.g";
import { defineComponent, h } from "vue";
import { mockEndpoint, mount, mountWithCoalesceOptions } from "@test/util";

describe("CAdminTableToolbar", () => {
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

  describe("adminExtensions - tableToolbarActions", () => {
    test("renders type-specific extension component", async () => {
      const ExtensionComponent = defineComponent({
        name: "PersonToolbarActions",
        props: { list: { type: Object, required: true } },
        setup(props) {
          return () =>
            h("div", { class: "person-toolbar-extension" }, "Person Actions");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTableToolbar list={list} />,
        undefined,
        {
          adminExtensions: [
            [
              $metadata.types.Person,
              { tableToolbarActions: ExtensionComponent },
            ],
          ],
        },
      );

      expect(wrapper.find(".person-toolbar-extension").exists()).toBeTruthy();
      expect(wrapper.find(".person-toolbar-extension").text()).toBe(
        "Person Actions",
      );
    });

    test("renders global '*' extension when no type-specific override exists", async () => {
      const GlobalExtension = defineComponent({
        name: "GlobalToolbarActions",
        props: { list: { type: Object, required: true } },
        setup() {
          return () =>
            h("div", { class: "global-toolbar-extension" }, "Global Actions");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTableToolbar list={list} />,
        undefined,
        {
          adminExtensions: [["*", { tableToolbarActions: GlobalExtension }]],
        },
      );

      expect(wrapper.find(".global-toolbar-extension").exists()).toBeTruthy();
    });

    test("type-specific override takes precedence over global '*'", async () => {
      const GlobalExtension = defineComponent({
        name: "GlobalToolbarActions",
        props: { list: { type: Object, required: true } },
        setup() {
          return () => h("div", { class: "global-extension" }, "Global");
        },
      });

      const TypeExtension = defineComponent({
        name: "PersonToolbarActions",
        props: { list: { type: Object, required: true } },
        setup() {
          return () => h("div", { class: "type-extension" }, "Type-specific");
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminTableToolbar list={list} />,
        undefined,
        {
          adminExtensions: [
            ["*", { tableToolbarActions: GlobalExtension }],
            [$metadata.types.Person, { tableToolbarActions: TypeExtension }],
          ],
        },
      );

      expect(wrapper.find(".type-extension").exists()).toBeTruthy();
      expect(wrapper.find(".global-extension").exists()).toBeFalsy();
    });

    test("does not render anything when no extension is configured", async () => {
      const list = new PersonListViewModel();
      await list.$load();

      const wrapper = mount(() => <CAdminTableToolbar list={list} />);

      // Toolbar should still render normally without extensions
      expect(wrapper.find(".c-admin-table-toolbar").exists()).toBeTruthy();
    });

    test("extension component receives list prop", async () => {
      let receivedList: any = null;

      const ExtensionComponent = defineComponent({
        name: "ListInspector",
        props: { list: { type: Object, required: true } },
        setup(props) {
          receivedList = props.list;
          return () => h("div", { class: "list-inspector" });
        },
      });

      const list = new PersonListViewModel();
      await list.$load();

      mountWithCoalesceOptions(
        () => <CAdminTableToolbar list={list} />,
        undefined,
        {
          adminExtensions: [
            [
              $metadata.types.Person,
              { tableToolbarActions: ExtensionComponent },
            ],
          ],
        },
      );

      expect(receivedList).toBe(list);
    });
  });

  describe("toolbar-actions slot", () => {
    test("slot content replaces extension component", async () => {
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
          <CAdminTableToolbar list={list}>
            {{
              "toolbar-actions": () =>
                h("div", { class: "slot-content" }, "Custom Slot"),
            }}
          </CAdminTableToolbar>
        ),
        undefined,
        {
          adminExtensions: [
            [
              $metadata.types.Person,
              { tableToolbarActions: ExtensionComponent },
            ],
          ],
        },
      );

      expect(wrapper.find(".slot-content").exists()).toBeTruthy();
      expect(wrapper.find(".extension-component").exists()).toBeFalsy();
    });

    test("slot receives list as scoped slot prop", async () => {
      let slotList: any = null;

      const list = new PersonListViewModel();
      await list.$load();

      mount(() => (
        <CAdminTableToolbar list={list}>
          {{
            "toolbar-actions": (props: any) => {
              slotList = props.list;
              return h("div", { class: "slot-content" });
            },
          }}
        </CAdminTableToolbar>
      ));

      expect(slotList).toBe(list);
    });
  });
});
