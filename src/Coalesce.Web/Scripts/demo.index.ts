/// <reference path="../typings/tsd.d.ts" />
/// <reference path="intellitect.references.d.ts" />

module Demo {
    $(function () {
        var viewModel = new DemoModel();
        ko.applyBindings(viewModel);

        viewModel.load();
    });

    class DemoModel {
        allOpenCases = new ListViewModels.CaseList();

        load() {
            this.allOpenCases.pageSize(20);
            this.allOpenCases.listDataSource = ListViewModels.CaseDataSources.GetAllOpenCases;
            this.allOpenCases.load();
        }
    }
}