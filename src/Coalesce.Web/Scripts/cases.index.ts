/// <reference path="viewmodels.generated.d.ts" />

module Cases {
    var viewModel: CaseDetailModel;
    var status: string = Coalesce.Utilities.GetUrlParameter("status");

    $(function () {
        viewModel = new CaseDetailModel();
        ko.applyBindings(viewModel);

        viewModel.load();
    });

    class CaseDetailModel {
        cases = new ListViewModels.CaseList();

        load() {
            if (status != null && status != '') {
                if (status == 'allOpen') {
                    this.cases.dataSource = ListViewModels.CaseDataSources.AllOpenCases;
                }
                else {
                    let retrievedStatus = new ViewModels.Case().statusValues.filter(function (obj) {
                        return obj.value.toLowerCase() == status.toLowerCase();
                    });
                    if (retrievedStatus.length > 0) {
                        this.cases.query = { status: retrievedStatus[0].id };
                    }
                }
            }

            this.cases.load();
        }
    }
}