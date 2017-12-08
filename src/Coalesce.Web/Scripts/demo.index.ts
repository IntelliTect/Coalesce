/// <reference path="viewmodels.generated.d.ts" />

module Demo {
    $(function () {
        var allOpenCases = new ListViewModels.CaseList();

        allOpenCases.pageSize(100);
        allOpenCases.dataSource = new allOpenCases.dataSources.AllOpenCases();
        allOpenCases.load();

        ko.applyBindings(allOpenCases);
    });
}