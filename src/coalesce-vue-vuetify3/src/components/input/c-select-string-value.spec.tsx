import { nextTick, ref } from "vue";
import { CSelectStringValue } from "..";
import { PersonViewModel } from "@test-targets/viewmodels.g";
import { mockEndpoint } from "coalesce-vue/test-utils";
import { delay, getWrapper, mountApp } from "@test/util";
import { Mock } from "vitest";
import { VueWrapper, flushPromises } from "@vue/test-utils";

describe("CSelectStringValue", () => {
  test("types", () => {
    const vm = new PersonViewModel();
    const method = vm.$metadata.methods.namesStartingWith;
    const badMethod = vm.$metadata.methods.rename;
    const selectedString = ref<string | null>("foo");

    () => <CSelectStringValue model={vm} for="name" method={method} />;
    () => <CSelectStringValue model={vm as any} for="name" method={method} />;
    () => (
      <CSelectStringValue model={vm} for="name" method="namesStartingWith" />
    );
    () => (
      <CSelectStringValue
        model={vm as any}
        for="name"
        method="namesStartingWith"
      />
    );
    () => (
      <CSelectStringValue
        model={vm}
        for={vm.$metadata.props.name}
        method={method}
      />
    );

    
    () => <CSelectStringValue 
      for="Person" 
      method="namesStartingWith" 
      modelValue={selectedString.value} 
      onUpdate:modelValue={v => selectedString.value = v} 
    />;
    

    //@ts-expect-error wrong type for method
    () => <CSelectStringValue model={vm} for="name" method={badMethod} />;
    //@ts-expect-error wrong type for method
    () => <CSelectStringValue model={vm} for="name" method="rename" />;
    //@ts-expect-error nonexistent method
    () => <CSelectStringValue model={vm} for="name" method="doesntExist" />;

    //@ts-expect-error wrong `for` type
    () => <CSelectStringValue model={vm} for="birthDate" method={method} />;

    //@ts-expect-error missing `for`
    () => <CSelectStringValue model={vm} method={method} />;
    //@ts-expect-error missing `method`
    () => <CSelectStringValue model={vm} for="name" />;
  });

  describe("prop variants", () => {
    let mock: Mock;
    beforeEach(() => {
      mock = mockEndpoint(new PersonViewModel().$metadata.methods.namesStartingWith, vitest.fn(req => ({
        wasSuccessful: true,
        object: ["strFromServer"]
      })));
    })

    test("model={vm} for=propName method=methodName", async () =>{
      // Arrange/Act
      const vm = new PersonViewModel();
      const wrapper = mountApp(() => <CSelectStringValue 
        model={vm}
        for="firstName" 
        method="namesStartingWith"
        listWhenEmpty
      />);

      await delay(10);
      expect(mock).toBeCalledTimes(1)
      await selectFirstResult(wrapper);

      expect(mock).toBeCalledTimes(2) // Another call happens after selection to search for the new value
      expect(vm.firstName).toBe("strFromServer")
    })

    test("for=TypeName method=methodName v-model=selectedValue", async () =>{
      // Arrange/Act
      const selectedString = ref<string | null>("foo");
      const wrapper = mountApp(() => <CSelectStringValue 
        for="Person" 
        method="namesStartingWith" 
        listWhenEmpty
        modelValue={selectedString.value} 
        onUpdate:modelValue={v => selectedString.value = v} 
      />);

      await delay(10);
      expect(mock).toBeCalledTimes(1)
      await selectFirstResult(wrapper);

      expect(mock).toBeCalledTimes(2) // Another call happens after selection to search for the new value
      expect(selectedString.value).toBe("strFromServer")
    })
  })

  test("search", async () =>{
    const vm = new PersonViewModel();
    const mock = mockEndpoint(vm.$metadata.methods.namesStartingWith, (req => ({
      wasSuccessful: true,
      object: ["strFromServer", "otherStrFromServer"].filter(item => item.includes(req.params.search || ""))
    })));
    
    const wrapper = mountApp(() => <CSelectStringValue 
      model={vm}
      for="firstName" 
      method="namesStartingWith"
      listWhenEmpty
    />);

    await nextTick();
    wrapper.find("input").setValue("oth")
    await nextTick();
    await selectFirstResult(wrapper);

    expect(vm.firstName).toBe("otherStrFromServer")
  })
});

const selectFirstResult = async (wrapper: VueWrapper) => {
  await flushPromises();
  await wrapper.find(".v-icon--clickable").trigger("mousedown");
  await flushPromises();
  const overlay = getWrapper(".v-overlay__content");

  const listItem = overlay.find(".v-list-item");
  await listItem.trigger("click");
};
