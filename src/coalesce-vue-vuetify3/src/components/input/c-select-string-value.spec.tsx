import { CSelectStringValue } from "..";
import { PersonViewModel } from "@test-targets/viewmodels.g";

describe("CSelectStringValue", () => {
  test("types", () => {
    const vm = new PersonViewModel();
    const method = vm.$metadata.methods.namesStartingWith;
    const badMethod = vm.$metadata.methods.rename;

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

    //@ts-expect-error wrong type for method
    () => <CSelectStringValue model={vm} for="name" method={badMethod} />;
    //@ts-expect-error wrong type for method
    () => <CSelectStringValue model={vm} for="name" method="rename" />;

    //@ts-expect-error wrong `for` type
    () => <CSelectStringValue model={vm} for="birthDate" method={method} />;

    //@ts-expect-error missing `for`
    () => <CSelectStringValue model={vm} method={method} />;
    //@ts-expect-error missing `model`
    () => <CSelectStringValue for="name" method={method} />;
    //@ts-expect-error missing `method`
    () => <CSelectStringValue model={vm} for="name" />;
  });
});
