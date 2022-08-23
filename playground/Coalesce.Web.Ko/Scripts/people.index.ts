/// <reference path="viewmodels.generated.d.ts" />

module People {
    var viewModel: PeopleModel;
    var id: string | null = Coalesce.Utilities.GetUrlParameter("id");

    $(function () {
        viewModel = new PeopleModel();
        ko.applyBindings(viewModel, document.body);

        viewModel.load();
    });

    class PeopleModel {
        //people = new ListViewModels.PersonList();
        person = new ViewModels.Person();

        load() {
            if (id != null && id != '') {
                this.person.load(id);
            }
        }
    }
}