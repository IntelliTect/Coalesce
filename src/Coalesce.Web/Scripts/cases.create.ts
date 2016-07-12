/// <reference path="../typings/tsd.d.ts" />
/// <reference path="intellitect.references.d.ts" />

module Cases {
    var viewModel: NewCaseModel;

    $(function () {
        viewModel = new NewCaseModel();
        viewModel.newCase.isSavingAutomatically = false;
        ko.applyBindings(viewModel);
    });

    class NewCaseModel {
        newCase = new ViewModels.Case;

        save() {
            this.newCase.save(function() {
                window.location.href = "/cases";
            });
        }
    }
}