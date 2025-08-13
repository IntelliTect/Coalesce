import { CAdminTableColumnSelector } from "..";
import { PersonViewModel } from "@test-targets/viewmodels.g";

describe("CAdminTableColumnSelector", () => {
  test("types", () => {
    const vm = new PersonViewModel();
    const availableProps = Object.values(vm.$metadata.props);
    const selectedColumns = ["name", "email"];
    const defaultColumns = ["name", "email", "phone"];

    // Test basic component usage
    () => (
      <CAdminTableColumnSelector
        availableProps={availableProps}
        selectedColumns={selectedColumns}
        defaultColumns={defaultColumns}
        onUpdate:selectedColumns={(columns: string[]) => {}}
      />
    );

    // Test with empty arrays
    () => (
      <CAdminTableColumnSelector
        availableProps={[]}
        selectedColumns={[]}
        defaultColumns={[]}
        onUpdate:selectedColumns={(columns: string[]) => {}}
      />
    );
  });
});