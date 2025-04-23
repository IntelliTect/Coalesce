import { CSelectManyToMany } from "..";
import { CaseViewModel } from "@test-targets/viewmodels.g";

describe("CSelectManyToMany", () => {
  // prettier-ignore
  test("types", () => {
    const caseVm = new CaseViewModel();

    () => <CSelectManyToMany model={caseVm} for="caseProducts" />;
    () => (
      <CSelectManyToMany
        model={caseVm}
        for={caseVm.$metadata.props.caseProducts}
      />
    );
    () => <CSelectManyToMany model={caseVm} for="caseProducts"
      itemTitle={(model) => model.caseProductId!.toFixed()}
    />;

    //@ts-expect-error wrong type
    () => <CSelectManyToMany model={caseVm} for="long" />;
    () => (
      //@ts-expect-error wrong type
      <CSelectManyToMany model={caseVm} for={caseVm.$metadata.props.caseKey} />
    );

    //@ts-expect-error wrong parameter type on itemTitle
    () => <CSelectManyToMany model={caseVm} for="caseProducts" itemTitle={(x: CaseViewModel) => x.toString()} />;

    //@ts-expect-error missing `model`
    () => <CSelectManyToMany for={caseVm.$metadata.props.caseProducts} />;
    //@ts-expect-error missing `for`
    () => <CSelectManyToMany model={caseVm} />;
  });
});
