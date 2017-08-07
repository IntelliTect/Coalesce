## Breaking Changes in Coalesce 2.0:

* NPM Packages: Upgraded to `gulp-typescript` 3.0 and `typescript` 2.3. Breaking changes may be found at http://dev.ivogabe.com/gulp-typescript-3/.
  * You will need to add/update references to `"gulp-typescript": "^3.1.6"` and `"typescript": "~2.3.2"` in your package.json.

* TypeScript Global variable `baseUrl` is gone. Replaced by `Coalesce.GlobalConfiguration.baseApiUrl` and `baseViewUrl`.
* TypeScript gloabl variable `saveTimeoutInMs` is gone. Replaced by `Coalesce.GlobalConfiguration.saveTimeoutMs`.
* Various knockout bindings that took a `saveImmediately` supplimentary binding no longer do so. 
  * The `saveImmediately` standalone binding still exists, but will only function if the currently scoped binding object is a Coalesce `BaseViewModel<T>`. This is beacuse the binding must access the model's `coalesceConfig` to set the save timeout, since `saveTimeoutInMs` is no longer a global.
* Knockout binding `select2AjaxText`: no longer takes `object` as a complimentary binding in order to compute the url - now takes `url` instead to be consistent with other select2 bindings.
* (Non-breaking, but very signigicant): References to Scripts/Coalesce/coalesce.*.ts files are now located in Scripts/coalesce.depdenencies.d.ts. This file is not regenerated and may be modified at will, but must remain in its location relative to Scripts/Generated.
* Scripts/intellitect.references.d.ts is now named viewmodels.generated.d.ts. This is the file that should be referenced in your custom typescript files in order to consume Coalesce generated typescript files.
* Scripts/Coalesce/intellitect.* files are now named Scripts/Coalesce/coalesce.*
* Namespace intellitect.utilities is now Coalesce.Utilities
* Namespace intellitect.webApi is now Coalesce.ModalHelpers
* Generated API Controllers no longer have "Api" in the file name in order to better match the name of the class within.
* <ModelName>ListUrl properties on generated ViewModels is now correctly camelCased.
* BaseViewModel<T>:
	* deleteCallbacks have been removed.
	* customField1, 2, & 3 have been removed.
	* isDataFromSaveLoaded has been changed to BaseViewModel<T>.coalesceConfig.loadResponseFromSaves
	* afterLoadFromDto has been changed to BaseViewModel<T>.coalesceConfig.onLoadFromDto
	* loadValidValues is gone, as are the other model-specific methods and collections of a similar nature.
	* isSavingWithChildren is now named isThisOrChildSaving.
	* reload has been removed - it was redundant with .load().
	* changeIsExpanded is now named toggleIsExpanded, and no longer takes a parameter.
	* changeIsEditing is now named toggleIsEditing, and no longer takes a parameter.
	* isSelectedToggle is now named toggleIsSelected
* Generated ViewModels:
	* public originalData: KnockoutObservable<any> has been removed.
    * public init() has been removed.

## Deprications in Coalesce 2..0"

* `BaseViewModel<T>.autoSaveEnabled`: Use `BaseViewModel<T>.coalesceConfig.autoSaveEnabled` observable instead.
* `BaseViewModel<T>.showBusyWhenSaving`: Use `BaseViewModel<T>.coalesceConfig.showBusyWhenSaving` observable instead.
* `BaseViewModel<T>.showFailureAlerts`: Use `BaseViewModel<T>.coalesceConfig.showFailureAlerts` observable instead.

## New Features in Coalesce 2.0:

* Hierarchical configuration system:
  * Many points of configuration for ViewModels and ListViewModels are done via a hierarchical configuration system.
  * All configuration properties are observables. Getters will set the property at that level, causing it cascade down until it reaches an instance, or until it is overridden at a lower level.
  * Setting a configuration property to null will reset it back to inheriting from an ancestor. Setting the global defaults to null will probably cause errors.
  * Configuration defaults can be found around line 68 of coalesce.ko.base.ts.
  * A global object, Coalesce.GlobalConfiguration, has most of the default configuration.
  * Two more global configuration objects exist as properties on Coalesce.GlobalConfiguration. These are `.viewModel` and `.listViewModel`, and they control the defaults for ViewModels and ListViewModels, respectively. There are some properties on these that are specific to one type or the other that don't appear on Coalesce.GlobalConfiguration itself.
  * On each ViewModel and ListViewModel class, there is a class-level configuration object named `coalesceConfig`. For example, `ViewModels.User.coalesceConfig`. These objects control settings for all instances of their type.
  * On each ViewModel and ListViewModel instance, there is an instance-level configuration object named `coalesceConfig`. This object controls settings for each specific instance of a model.

* New attribute: [Controller]. Place this attribute on your C# model classes to customize the generation of the API controllers.


