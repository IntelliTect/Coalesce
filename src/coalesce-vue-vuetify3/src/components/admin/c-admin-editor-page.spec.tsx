import { CAdminEditorPage } from "..";
import "@test-targets/viewmodels.g"; // registers ViewModel.typeLookup
import $metadata from "@test-targets/metadata.g";
import { defineComponent, h } from "vue";
import { mockEndpoint, mountWithCoalesceOptions } from "@test/util";

describe("CAdminEditorPage", () => {
  beforeEach(() => {
    mockEndpoint("/Person/get/1", () => ({
      wasSuccessful: true,
      object: { personId: 1, firstName: "Alice", lastName: "A" },
    }));
  });

  describe("adminExtensions - editorPageHeader", () => {
    test("renders editorPageHeader extension", () => {
      const HeaderExtension = defineComponent({
        name: "EditorPageHeader",
        props: { model: { type: Object, required: true } },
        setup() {
          return () =>
            h("div", { class: "editor-page-header-ext" }, "editor header");
        },
      });

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminEditorPage type="Person" id={1} />,
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { editorPageHeader: HeaderExtension }],
          ],
        },
      );

      expect(wrapper.find(".editor-page-header-ext").exists()).toBeTruthy();
      expect(wrapper.find(".editor-page-header-ext").text()).toBe(
        "editor header",
      );
    });

    test("editorPageHeader extension receives model prop", () => {
      let receivedModel: any = null;

      const HeaderExtension = defineComponent({
        name: "HeaderPropsInspector",
        props: { model: { type: Object, required: true } },
        setup(props) {
          receivedModel = props.model;
          return () => h("div", { class: "header-props-ext" });
        },
      });

      mountWithCoalesceOptions(
        () => <CAdminEditorPage type="Person" id={1} />,
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { editorPageHeader: HeaderExtension }],
          ],
        },
      );

      expect(receivedModel).not.toBeNull();
      expect(receivedModel.$metadata.name).toBe("Person");
    });

    test("global '*' editorPageHeader renders when no type-specific override", () => {
      const GlobalHeaderExtension = defineComponent({
        name: "GlobalEditorPageHeader",
        props: { model: { type: Object, required: true } },
        setup() {
          return () =>
            h("div", { class: "global-editor-header-ext" }, "global");
        },
      });

      const wrapper = mountWithCoalesceOptions(
        () => <CAdminEditorPage type="Person" id={1} />,
        undefined,
        {
          adminExtensions: [["*", { editorPageHeader: GlobalHeaderExtension }]],
        },
      );

      expect(wrapper.find(".global-editor-header-ext").exists()).toBeTruthy();
    });

    test("page-header slot replaces extension component", () => {
      const ExtensionComponent = defineComponent({
        name: "ShouldNotRender",
        props: { model: { type: Object, required: true } },
        setup() {
          return () => h("div", { class: "extension-component" });
        },
      });

      const wrapper = mountWithCoalesceOptions(
        () => (
          <CAdminEditorPage type="Person" id={1}>
            {{
              "page-header": () =>
                h("div", { class: "slot-content" }, "Custom Header"),
            }}
          </CAdminEditorPage>
        ),
        undefined,
        {
          adminExtensions: [
            [$metadata.types.Person, { editorPageHeader: ExtensionComponent }],
          ],
        },
      );

      expect(wrapper.find(".slot-content").exists()).toBeTruthy();
      expect(wrapper.find(".extension-component").exists()).toBeFalsy();
    });
  });
});
