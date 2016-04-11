/// <reference path="../../../typings/tsd.d.ts" />
/// <reference path="../../../Scripts/intellitect.references.d.ts" />

module CRM.Demo {
    var viewModel: DemoModel;

    $(function () {
        viewModel = new DemoModel();
        ko.applyBindings(viewModel);

        viewModel.load();
    });

    class DemoModel {
        totalCases = new ListViewModels.CaseList();
        openStatusCases = new ListViewModels.CaseList();
        allOpenCases = new ListViewModels.CaseList();

        load() {
            this.totalCases.getCount();

            this.openStatusCases.query = { status: new ViewModels.Case().statusValues[0].id };
            this.openStatusCases.getCount();

            this.allOpenCases.listDataSource = ListViewModels.CaseDataSources.GetAllOpenCases;
            this.allOpenCases.getCount();
        }
    }
}