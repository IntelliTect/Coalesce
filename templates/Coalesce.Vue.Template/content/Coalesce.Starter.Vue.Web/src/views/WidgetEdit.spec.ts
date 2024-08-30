import { mount, flushPromises, mockEndpoint } from "@/test-utils";

import { VTextField } from "vuetify/components";
import WidgetEdit from "./WidgetEdit.vue";
import { Widget, WidgetCategory } from "@/models.g";

describe("WidgetEdit.vue", () => {
  it("loads user id 1", async () => {
    // Arrange
    mockEndpoint("/Widget/get", () => ({
      wasSuccessful: true,
      object: {
        widgetId: 1,
        name: "Whirlygig",
        category: WidgetCategory.Sprecklesprockets,
      } as Widget,
    }));

    // Act
    const wrapper = mount(WidgetEdit, { props: { id: 1 } });
    await flushPromises();

    // Assert
    expect(wrapper.text()).toMatch("Editing Widget: 1");
    expect(document.title).toMatch("Whirlygig");
    expect(wrapper.findComponent(VTextField).vm.modelValue).toBe("Whirlygig");
  });
});
