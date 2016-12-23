/// <reference path="../typings/tsd.d.ts" />
/// <reference path="./IntelliTect.References.d.ts" />

var model = new ViewModels.Person();
model.load(1);
model.isSavingAutomatically = false;
ko.applyBindings(model);