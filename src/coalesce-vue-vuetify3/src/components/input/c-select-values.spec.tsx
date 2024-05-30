import { ref } from "vue";
import { CSelectValues } from "..";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";

describe("CSelectValues", () => {
  test("types", () => {
    const vm = new ComplexModelViewModel();

    () => <CSelectValues model={vm} for="mutablePrimitiveCollection" />;

    // Against models that might be null
    () => (
      <CSelectValues
        model={vm.referenceNavigation}
        for="mutablePrimitiveCollection"
      />
    );

    () => (
      <CSelectValues
        model={vm}
        for={vm.$metadata.props.mutablePrimitiveCollection}
      />
    );
    () => <CSelectValues model={vm.methodWithManyParams} for="stringsParam" />;
    () => (
      <CSelectValues
        model={vm.methodWithManyParams}
        for={vm.$metadata.methods.methodWithManyParams.params.stringsParam}
      />
    );

    const selectedStrings: string[] = [];
    () => (
      <CSelectValues
        for={vm.$metadata.props.mutablePrimitiveCollection}
        modelValue={selectedStrings}
      />
    );

    //@ts-expect-error wrong type
    () => <CSelectValues model={vm} for="name" />;
    () => (
      //@ts-expect-error wrong type
      <CSelectValues model={vm} for={vm.$metadata.props.name} />
    );

    //@ts-expect-error missing `for`
    () => <CSelectValues model={vm} />;
  });
});
