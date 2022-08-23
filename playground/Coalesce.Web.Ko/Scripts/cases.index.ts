/// <reference path="viewmodels.generated.d.ts" />

module Cases {
    var viewModel: CaseDetailModel;
    var status: string | null = Coalesce.Utilities.GetUrlParameter("status");

    $(function () {
        viewModel = new CaseDetailModel();
        ko.applyBindings(viewModel, document.body);

        viewModel.load();
    });

    class CaseDetailModel {
        cases = new ListViewModels.CaseList();

        load() {
            if (status != null && status != '') {
                if (status == 'allOpen') {
                    this.cases.dataSource = new this.cases.dataSources.AllOpenCases();
                }
                else {
                    let retrievedStatus = new ViewModels.Case().statusValues.filter(function (obj) {
                        return obj.value.toLowerCase() == (status || "").toLowerCase();
                    });
                    if (retrievedStatus.length > 0) {
                        this.cases.filter = { status: retrievedStatus[0].id.toString() };
                    }
                }
            }

            this.cases.load();
        }
    }
}