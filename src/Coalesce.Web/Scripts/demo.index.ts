/// <reference path="viewmodels.generated.d.ts" />

module Demo {
    $(function () {
        var allOpenCases = new ListViewModels.CaseList();

        allOpenCases.pageSize(20);
        allOpenCases.listDataSource = ListViewModels.CaseDataSources.GetAllOpenCases;
        allOpenCases.load();

        ko.applyBindings(allOpenCases);
    });
}