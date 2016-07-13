/// <reference path="../typings/tsd.d.ts" />
/// <reference path="intellitect.references.d.ts" />

module Demo {
    $(function () {
        var allOpenCases = new ListViewModels.CaseList();

        allOpenCases.pageSize(20);
        allOpenCases.listDataSource = ListViewModels.CaseDataSources.GetAllOpenCases;
        allOpenCases.load();

        ko.applyBindings(allOpenCases);
    });
}