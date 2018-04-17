/// <reference path="../coalesce.dependencies.d.ts" />

// Extend this class to have the right types.
interface KnockoutBindingHandlers {
    select2Ajax: KnockoutBindingHandler;
    select2AjaxMultiple: KnockoutBindingHandler;
    select2AjaxText: KnockoutBindingHandler;
    datePicker: KnockoutBindingHandler;
    select2: KnockoutBindingHandler;
    delaySave: KnockoutBindingHandler;
    tooltip: KnockoutBindingHandler;
    fadeVisible: KnockoutBindingHandler;
    slideVisible: KnockoutBindingHandler;
    moment: KnockoutBindingHandler & { defaults: any };
    momentFromNow: KnockoutBindingHandler & { defaults: any };
    booleanValue: KnockoutBindingHandler;
    formatNumberText: KnockoutBindingHandler;
}


// Select2 binding for an object that uses an AJAX call for valid values. 
ko.bindingHandlers.select2Ajax = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        const url = allBindings.get('url');
        const textField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('textField'));
        const idField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('idField'));

        const selectionFormat = allBindings.has("selectionFormat") ? allBindings.get("selectionFormat") : '{0}';
        const format = allBindings.has("format") ? allBindings.get("format") : '{0}';
        const setObject = allBindings.has("setObject") ? allBindings.get("setObject") : false;
        const itemViewModel: new (newItem: object) => Coalesce.BaseViewModel = allBindings.get('itemViewModel');
        const object: KnockoutObservable<Coalesce.BaseViewModel> = allBindings.has('object') ? allBindings.get('object') : null;
        const selectOnClose = allBindings.has("selectOnClose") ? allBindings.get("selectOnClose") : false;
        const openOnFocus = allBindings.has("openOnFocus") ? allBindings.get("openOnFocus") : false; // This doesn't work in IE (GE: 2016-09-27)
        const allowClear = allBindings.has("allowClear") ? allBindings.get("allowClear") : true;
        const placeholder = $(element).attr('placeholder') || "select";
        const pageSize = allBindings.get('pageSize') || 25;

        if (!url) throw "select2Ajax requires additional binding 'url'";
        if (!textField) throw "select2Ajax requires additional binding 'textField'";
        if (!idField) throw "select2Ajax requires additional binding 'idField'";
        if (setObject && !itemViewModel)
            throw "select2Ajax with 'setObject' requires additional binding 'itemViewModel'."
            + " This should be a reference to the class of object being selected - e.g.ViewModels.Person.";

        interface ResultItem {
            id: any,
            text: string,
            _apiObject: object
        }

        // Create the Select2
        $(element)
            .select2(<Select2Options>{
                theme: Coalesce.GlobalConfiguration.app.select2Theme() || undefined,
                ajax: {
                    url: url,
                    dataType: 'json',
                    delay: 250,
                    data: function (params: any) {
                        var data: any = {
                            search: params.term,
                            page: params.page,
                            pageSize: pageSize,
                        };

                        if (!setObject) {
                            // If we're NOT going to populate the object observable with the results from our API call,
                            // then its safe to only get the two fields that we need to populate the dropdown.
                            // If we ARE going to set the object (setObject == true), then we should be requesting the entire object from the server.

                            // There may be scenarios where you may want to set the object but still only want the two fields.
                            // In this case, just specify ".../list?fields=field1,field2" as part of the url in the binding.
                            data.fields = idField + "," + textField;
                        };

                        if (!allBindings.has('cache') || allBindings.get('cache'))
                            data["_"] = new Date().getTime();
                        return data;
                    },
                    processResults: function (data: any, params: any) {

                        // Transform our objects into what select2 wants (it needs objects with a key "id").
                        // We throw the raw object from the API on there as well so we can use it when handling a selection.
                        var results = (data.list as any[]).map(item => {
                            return {
                                id: item[idField],
                                text: item[textField] && item[textField].toString(),
                                _apiObject: item,
                            } as ResultItem;
                        });


                        // This doesn't seem to have ever worked. Params doesn't have a property named 'page'.
                        // Leaving it commented out to maintain that there was some intention to do this, 
                        // but we seem to have gotten by just fine with out it up until now.

                        //if (allowClear && params.page == 1) {
                        //    // Add a blank item
                        //    var blank = {
                        //        id: 0, // This should probably not be 0, since 0 is a totally valid primary key.
                        //        text: 'No Selection',
                        //    };
                        //    results.unshift(blank);
                        //}

                        return {
                            results: results,
                            pagination: {
                                more: data.page < data.pageCount
                            }
                        };
                    },
                    cache: (allBindings.has('cache') ? allBindings.get('cache') : false).toString(),
                },
                placeholder: placeholder,
                allowClear: allowClear,
                selectOnClose: selectOnClose,
                templateResult: function (item: any) {
                    return format.replace('{0}',
                        typeof item.text == "string" ? item.text :
                            typeof item == "string" ? item : ""
                    );
                },
                templateSelection: function (item: any) {
                    return format.replace('{0}',
                        typeof item.text == "string" ? item.text :
                            typeof item == "string" ? item : ""
                    );
                },
            })
            .on("change", function (e) {
                // Code to update knockout
                var value: string = $(element).val() as string;

                // Loose equality is intentional - select2 always provides a string value when asked, 
                // but our observable is probably an integer number.
                if (valueAccessor()() != value && (valueAccessor()() || value)) {

                    // Just like we do in the .loadFromDto method in the generated ViewModels,
                    // we load the object property first BEFORE we load the primary key.

                    if (object) {
                        // Set the object if such functionality is enabled
                        if (setObject) {
                            var selectedData = $(element).select2("data");
                            if (selectedData && selectedData.length > 0) {
                                var result = (selectedData[0] as ResultItem)._apiObject;

                                var oldObject = object();
                                if (oldObject instanceof itemViewModel) {
                                    oldObject.loadFromDto(result);
                                    if (object.valueHasMutated) object.valueHasMutated();
                                } else {
                                    object(new itemViewModel(result));
                                }
                            } else {
                                object(null);
                            }
                        }
                        else {
                            // Clear the object because we don't know anything about it.
                            // It might be reloaded when the parent object is saved.
                            object(null);
                        }
                    }

                    // Set the ID.
                    if (value) {
                        valueAccessor()(value);
                    } else {
                        valueAccessor()(null);
                    }
                }
            });

        if (openOnFocus) {
            $.data(element).select2.on("focus", function () {
                $(element).select2("open");
            });
        }

        // Add the validation message
        const validationCore = ko.bindingHandlers['validationCore'];
        if (!validationCore.init) throw "Fatal: validationCore.init missing"
        validationCore.init(element, valueAccessor, allBindings, viewModel, bindingContext)

        // The validation message needs to go after the new select2 dropdown, not before it.
        $(element).next(".validationMessage").insertAfter($(element).nextAll(".select2").first());
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // See if the value exists. If not, we haven't loaded it from the server yet.
        var clearOnNull = allBindings.get("clearOnNull") || false;
        var value = valueAccessor()();
        var select2Value = $(element).val();
        var setObject = allBindings.has("setObject") ? allBindings.get("setObject") : false;
        var textField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('textField'));
        var idField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('idField'));
        var object: KnockoutObservable<any> = allBindings.get('object')

        // See if something has changed
        var option: HTMLOptionElement | undefined;
        var triggerSelect2Change = false;

        if (value) {
            let options = $(element).find('option[value="' + value.toString().replace(/"/g, '\\"') + '"]');
            if (options.length > 0) option = options[0] as HTMLOptionElement;
        } else {
            let options = $(element).find('option[value="' + value + '"]');
            if (options.length > 0) option = options[0] as HTMLOptionElement;
        }

        if (!option) {
            option = new Option();
            option.value = value;
            option.selected = true;
            option.defaultSelected = true;

            $(element).append(option);
            triggerSelect2Change = true;
        }


        if (value === null && clearOnNull) {
            $(element).val("").trigger("change");
        } else {
            // Add it based on the object.
            var objectUnwrapped = ko.unwrap(object);
            if (value && objectUnwrapped) {
                // Get the raw key value from the object that we think represents the selected key.
                var id = ko.unwrap(objectUnwrapped[idField]);
                var text = ko.unwrap(objectUnwrapped[textField]);

                // Check id == value here to make sure we're creating an option for the correct object.
                // If the observable holding the ID changes but the object doesn't, then these won't match,
                // and we would risk displaying the wrong data if we didn't check for this.
                if (text && id && id == value) {
                    option.text = text;
                    triggerSelect2Change = true;
                }
            }
        }

        // Set the element based on the value in the model.
        if (triggerSelect2Change) {

            // When a change is triggered in select2, it doesn't update its internal option object
            // to reflect the text of the HTMLOptionElement. So, we have to manually update it,
            // taking care to only change it if the IDs match.
            var select2Option = $(element).select2("data")[0];
            if (select2Option && select2Option.id == option.value) {
                select2Option.text = option.text;
            }

            $(element).val(value).trigger('change');
        }
    }
};

// Multi-select Select2 binding that uses an AJAX call for the list of valid values.
ko.bindingHandlers.select2AjaxMultiple = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        const url = allBindings.get('url');
        const itemViewModel: new (newItem: object) => Coalesce.BaseViewModel = allBindings.get('itemViewModel');

        // 'idFieldName' was the old name, kept for backwards compat. 'idField' is the new name.
        const idField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('idFieldName') || allBindings.get('idField'));
        const textField = Coalesce.Utilities.lowerFirstLetter(allBindings.get('textFieldName') || allBindings.get('textField'));

        if (!url) throw "select2AjaxMultiple requires additional binding 'url'";
        if (!itemViewModel) throw "select2AjaxMultiple requires additional binding 'itemViewModel'."
            + " This should be the class of the foreign end of the relationship - e.g.ViewModels.Case.";
        if (!idField) throw "select2AjaxMultiple requires additional binding 'idField'";
        if (!textField) throw "select2AjaxMultiple requires additional binding 'textField'";

        const selectionFormat = allBindings.has("selectionFormat") ? allBindings.get("selectionFormat") : '{0}';
        const format = allBindings.has("format") ? allBindings.get("format") : '{0}';
        const selectOnClose = allBindings.has("selectOnClose") ? allBindings.get("selectOnClose") : false;
        const openOnFocus = allBindings.has("openOnFocus") ? allBindings.get("openOnFocus") : false;
        const allowClear = allBindings.has("allowClear") ? allBindings.get("allowClear") : true;
        const placeholder = $(element).attr('placeholder') || "select";
        const pageSize = allBindings.get('pageSize') || 25;

        interface ResultItem {
            id: any,
            text: string,
            _apiObject?: object
        }

        // Create the Select2
        $(element)
            .select2(<Select2Options>{
                theme: Coalesce.GlobalConfiguration.app.select2Theme() || undefined,
                ajax: {
                    url: url,
                    dataType: 'json',
                    delay: 250,
                    data: function (params: any) {
                        var data: any = {
                            search: params.term,
                            page: params.page
                        };
                        if (!allBindings.has('cache') || allBindings.get('cache'))
                            data["_"] = new Date().getTime();
                        return data;
                    },
                    processResults: function (data: any, params: any) {
                        // Transform our objects into what select2 wants (it needs objects with a key "id").
                        // We throw the raw object from the API on there as well so we can use it when handling a selection.
                        var results = (data.list as any[]).map(item => {
                            return {
                                id: item[idField],
                                text: item[textField] && item[textField].toString(),
                                _apiObject: item,
                            } as ResultItem;
                        });

                        return {
                            results: results,
                            pagination: {
                                more: data.page < data.pageCount
                            }
                        };
                    },
                    cache: (allBindings.has('cache') ? allBindings.get('cache') : false).toString(),
                },
                //escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
                //minimumInputLength: 1,
                placeholder: placeholder,
                selectOnClose: selectOnClose,
                allowClear: allowClear,
                templateResult: function (item: any) {
                    if (item.Classes) {
                        // This has a class use the formatting
                        var optionElement = $('<span class="' + item.Classes + '">' +
                            format.replace('{0}', (typeof item.text == "string" ? item.text : item))
                            + '</span>');
                        return optionElement;
                    }
                    return format.replace('{0}', (typeof item.text == "string" ? item.text : item));
                },
                templateSelection: function (item: any) {
                    //if (e.Classes) {
                    //    // This has a class use the formatting
                    //    var optionElement = $('<span class="' + e.Classes + '">' +
                    //        format.replace('{0}', (e[textFieldName] || e.text || e))
                    //        + '</span>');
                    //    return optionElement;
                    //}
                    return selectionFormat.replace('{0}', (item.text || item));
                },
            })
            .on("change", function (e) {
                if ($(element).data("select2-ajax-updating")) return;
                $(element).data("select2-ajax-updating", true);

                // Code to update knockout
                var selectedItems = $(element).select2("data");
                var values: KnockoutObservableArray<Coalesce.BaseViewModel> = valueAccessor();
                if (values() && selectedItems && values().length != selectedItems.length) {
                    // Add the items to the observable array.
                    // Warning: this code is O(n^2), but n should always be very reasonably small (if its > 50, you shouldn't be using a control like this.)
                    if (selectedItems.length > values().length) {
                        // Item was added.
                        for (var i = 0; i < selectedItems.length; i++) {
                            var selectedItem = selectedItems[i];
                            var found = false;
                            for (var j = 0; j < values().length; j++) {
                                var value = values()[j];
                                found = found || (value.myId == selectedItem.id);
                            }
                            if (!found) {
                                // This is the missing one.
                                values.push(new itemViewModel(selectedItem._apiObject));
                            }
                        }
                    } else if (selectedItems.length < values().length) {
                        // Item was removed
                        for (var i = 0; i < values().length; i++) {
                            var value = values()[i];
                            var found = false;
                            for (var j = 0; j < selectedItems.length; j++) {
                                var selectedItem = selectedItems[j];
                                found = found || (value.myId == selectedItem.id);
                            }
                            if (!found) {
                                // This is the missing one. Remove it.
                                values.splice(i, 1);

                                // Also remove the corresponding option element, if there is one.
                                // See the wall of text in the update handler of this binding below for an explanation why.
                                $(element).find("option").filter((_, e) => e.getAttribute("value") == value.myId).remove();
                            }
                        }

                    } else {
                        // Nothing changed.
                    }
                }

                $(element).data("select2-ajax-updating", false);
            });
        if (openOnFocus) {
            $.data(element).select2.on("focus", function () {
                $(element).select2("open");
            });
        }
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // Make sure that we read the valueAccessor BEFORE we return out of this function.
        // Otherwise, the dependency detection will not be set up correctly.
        // See https://stackoverflow.com/a/23880007/2465631;
        var value: Array<any> = valueAccessor()();

        if ($(element).data("select2-ajax-updating")) return;
        $(element).data("select2-ajax-updating", true);

        var idFieldName = Coalesce.Utilities.lowerFirstLetter(allBindings.get('idFieldName') || allBindings.get('idField'));
        var textFieldName = Coalesce.Utilities.lowerFirstLetter(allBindings.get('textFieldName') || allBindings.get('textField'));

        // See if the value exists. If not, we haven't loaded it from the server yet.
        var select2Value = $(element).val();
        if (!select2Value) select2Value = [];
        var selectedIds = [];
        // Convert the field names to js variables.
        textFieldName = textFieldName.charAt(0).toLowerCase() + textFieldName.slice(1);
        idFieldName = idFieldName.charAt(0).toLowerCase() + idFieldName.slice(1);

        // Remove all of the temporary option elements that we had to create.
        // These option elements are created so that select2 can display items that are
        // part of the collection of selected objects, but were not added to the list via select2 -
        // they came either with the initial data load, or were manually added to the undelying collection programatically.
        // We have to remove these options because if we don't,
        // and the user decides to remove an option from the selection box and then re-select that same item,
        // select2 won't use the "options" object from its ajax call (which contains our _apiObject property) -
        // it will use a fake object that it made that contains only and 'id' and 'text' property.
        $(element).find("option").remove();

        for (var i in value) {
            var item = value[i];
            var text = item[textFieldName]();
            var id = item[idFieldName]();
            var option = new Option(text, id, true, true);
            $(element).append(option);
            selectedIds.push(id);
        }

        $(element).val(selectedIds).trigger('change');
        $(element).data("select2-ajax-updating", false);
    }
};


// Select2 binding for a string value to show a list of other values
ko.bindingHandlers.select2AjaxText = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        const url = allBindings.get('url');
        const selectOnClose = allBindings.has("selectOnClose") ? allBindings.get("selectOnClose") : false;
        const openOnFocus = allBindings.has("openOnFocus") ? allBindings.get("openOnFocus") : false; // This doesn't work in IE (GE: 2016-09-27)
        const allowClear = allBindings.has("allowClear") ? allBindings.get("allowClear") : true;
        const placeholder = $(element).attr('placeholder') || "select";
        const resultField = allBindings.has("resultField") ? allBindings.get("resultField") : null;

        var myParams: any;

        // Create the Select2
        $(element)
            .select2(<Select2Options>{
                theme: Coalesce.GlobalConfiguration.app.select2Theme() || undefined,
                ajax: {
                    url: url,
                    dataType: 'json',
                    delay: 250,
                    data: function (params: any) {
                        myParams = params
                        var data: any = {
                            search: params.term,
                            page: params.page
                        };
                        if (!allBindings.has('cache') || allBindings.get('cache'))
                            data["_"] = new Date().getTime();
                        return data;
                    },
                    processResults: function (data: any, page: any) {
                        var result = [];
                        if (allowClear && !myParams.term) {
                            // Add a blank item
                            var blank = {
                                id: 0,
                                text: 'No Selection'
                            };
                            result.unshift(blank);
                        }
                        var perfectMatch = false;

                        var items: any[];
                        if (Array.isArray(data)) {
                            // Raw endpoint that serves an array response.
                            items = data;
                        }
                        else if (typeof data !== "object") {
                            throw "Couldn't figure out how to access the text results - response wasn't an object for call to " + url;
                        }
                        else if (Array.isArray(data.object)) {
                            // Endpoint that serves an ItemResult<IEnumerable<string>>
                            // For example, a custom model method or static method.
                            items = data.object;
                        }
                        else if (Array.isArray(data.list)) {
                            // Endpoint that serves a ListResult<string>
                            items = data.list;
                        }
                        else {
                            throw "Couldn't figure out how to access the text results for call to " + url;
                        }

                        for (let item of items) {
                            if (typeof item === "object" && resultField) {
                                item = item[resultField];
                            }
                            
                            if (item === undefined || item === null) {
                                continue;
                            }

                            if (item == myParams.term) {
                                perfectMatch = true;
                                result.push({ id: item, text: item, selected: true });
                            } else {
                                result.push({ id: item, text: item });
                            }
                        }
                        if (!perfectMatch && myParams.term) {
                            result.push({ id: myParams.term, text: myParams.term, selected: true });
                        }
                        return { results: result };
                    },
                    cache: (allBindings.has('cache') ? allBindings.get('cache') : false).toString(),
                },
                //escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
                //minimumInputLength: 1,
                placeholder: placeholder,
                allowClear: allowClear,
                selectOnClose: selectOnClose
            })
            .on("change", function (e) {
                // Code to update knockout
                var value = $(element).val();
                if (valueAccessor()() !== value) {
                    if (value) {
                        valueAccessor()(value);
                    } else {
                        valueAccessor()(null);
                    }
                }
            });
        if (openOnFocus) {
            $.data(element).select2.on("focus", function () {
                $(element).select2("open");
            });
        }

    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // See if the value exists. If not, we haven't loaded it from the server yet.
        var value = valueAccessor()();
        var options;
        if (value) {
          options = $(element).find('option[value="' + value.toString().replace(/"/g, '\\"') + '"]');
        } else {
          options = $(element).find('option[value="' + value + '"]');
        }


        // The option doesn't exist.
        if (options.length == 0) {
            // Add it based on the object.
            var option = new Option(value, value, true, true);
            $(element).append(option);
        }

        // Set the element based on the value in the model.
        $(element).val(valueAccessor()()).trigger('change');
    }
};


// Simple Select2 binding used with an options list that is in HTML 
ko.bindingHandlers.select2 = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        const selectOnClose = allBindings.has("selectOnClose") ? allBindings.get("selectOnClose") : false;
        const openOnFocus = allBindings.has("openOnFocus") ? allBindings.get("openOnFocus") : false; // This doesn't work in IE (GE: 2016-09-27)
        const allowClear = allBindings.has("allowClear") ? allBindings.get("allowClear") : true;
        const placeholder = $(element).attr('placeholder') || "select";

        // Create the Select2
        $(element)
            .select2(<Select2Options>{
                theme: Coalesce.GlobalConfiguration.app.select2Theme() || undefined,
                placeholder: placeholder,
                allowClear: allowClear,
                selectOnClose: selectOnClose,
            })
            .on("change", function (e) {
                // Code to update knockout
                var value = $(element).val();
                if (valueAccessor()() != value) {
                    if (value) {
                        valueAccessor()(value);
                    } else {
                        valueAccessor()(null);
                    }
                }
            });
        if (openOnFocus) {
            $.data(element).select2.on("focus", function () {
                $(element).select2("open");
            });
        }


        // Add the validation message
        const validationCore = ko.bindingHandlers['validationCore'];
        if (!validationCore.init) throw "Fatal: validationCore.init missing"
        validationCore.init(element, valueAccessor, allBindings, viewModel, bindingContext);
        
        // The validation message needs to go after the new select2 dropdown, not before it.
        $(element).next(".validationMessage").insertAfter($(element).nextAll(".select2").first());
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // Set the element based on the value in the model.
        $(element).val(valueAccessor()()).trigger('change');
    }
};


ko.bindingHandlers.datePicker = {
    init: function (element, valueAccessor: () => KnockoutObservable<moment.Moment>, allBindings, viewModel, bindingContext) {
        // See if we should use the parent element 
        var theElement = $(element).parent(".input-group.date") || $(element);

        var getValue = (): moment.Moment | null => {
            var newValue = theElement.data("DateTimePicker").date();

            if (!newValue) {
                return null;
            }

            var preserveDate = allBindings.get('preserveDate') || false;
            var preserveTime = allBindings.get('preserveTime') || false;
            var unwrappedValue = valueAccessor()();

            if (!preserveDate && !preserveTime) {
                newValue = newValue;
            } else if (preserveTime) {
                // This is a date entry, keep the time. 
                var unwrappedTime = moment.duration(unwrappedValue.format('HH:mm:ss'));
                newValue = moment(newValue.format("YYYY/MM/DD"), "YYYY/MM/DD").add(unwrappedTime);
            } else if (preserveDate) {
                // This is a time entry, keep the date.
                var newTime = moment.duration(newValue.format('HH:mm:ss'));
                newValue = moment(unwrappedValue.format('YYYY/MM/DD'), "YYYY/MM/DD").add(newTime);
            }

            return newValue;
        }

        var updateValue = () => {
            var newValue = getValue();

            // Set the value if it has changed.
            var currentObservable = valueAccessor()();
            if (!currentObservable || !newValue || !newValue.isSame(currentObservable)) {
                valueAccessor()(newValue);
            }
        }



        theElement.datetimepicker({
            format: allBindings.get('format') || "M/D/YY h:mm a",
            stepping: allBindings.get('stepping') || 1,
            sideBySide: allBindings.get('sideBySide') || false,
            timeZone: allBindings.get('timeZone') || "",
            keyBinds: allBindings.get('keyBinds') || { left: null, right: null, delete: null },
        })
            .on("dp.change", function (e) {
                if (allBindings.get('updateImmediate')) {
                    updateValue();
                }
            })
            .on("dp.hide", updateValue)
            .on('click', e => e.stopPropagation())
            .on('dblclick', e => e.stopPropagation());

        $(element)
            .on("blur", updateValue);

        // Add the validation message
        const validationCore = ko.bindingHandlers['validationCore'];
        if (!validationCore.init) throw "Fatal: validationCore.init missing"
        validationCore.init(element, valueAccessor, allBindings, viewModel, bindingContext);
        
        // The validation message needs to go after the input group with the button.
        $(element).next(".validationMessage").insertAfter($(theElement));
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // Set the element based on the value in the model.
        var theElement = $(element).parent(".input-group.date") || $(element);
        if (valueAccessor()()) theElement.data("DateTimePicker").date(valueAccessor()());
        else theElement.data("DateTimePicker").date(null);

    }
};


ko.bindingHandlers.saveImmediately = {
    init: function (element, valueAccessor, allBindings, viewModel: Coalesce.BaseViewModel, bindingContext) {
        if (!viewModel.coalesceConfig || !viewModel.coalesceConfig.autoSaveEnabled) {
            console.error("saveImmediately binding was used in a context where $data is not a Coalesce.BaseViewModel");
            return;
        }

        // Set up to save immediately when the cursor enters and return to a regular state when it leaves.
        var oldTimeoutValue: number;
        $(element).on("focus", function () {
            oldTimeoutValue = viewModel.coalesceConfig.saveTimeoutMs.raw();
            viewModel.coalesceConfig.saveTimeoutMs(0);
        });
        $(element).on("blur", function () {
            viewModel.coalesceConfig.saveTimeoutMs(oldTimeoutValue);
        });
    }
};


// Delays the save until the cursor leaves the field even if there is a value change.
ko.bindingHandlers.delaySave = {
    init: function (element, valueAccessor, allBindings, viewModel: Coalesce.BaseViewModel, bindingContext) {
        if (!viewModel.coalesceConfig || !viewModel.coalesceConfig.autoSaveEnabled) {
            console.error("delaySave binding was used in a context where $data is not a Coalesce.BaseViewModel");
            return;
        }
        var existingAutoSaveValueRaw: boolean;

        // Set up to not save immediately when the cursor enters and return to a regular state when it leaves.
        $(element).on("focus", function () {
            existingAutoSaveValueRaw = viewModel.coalesceConfig.autoSaveEnabled.raw();
            viewModel.coalesceConfig.autoSaveEnabled(false);
        });
        // Turn it back to previous state when the cursor leaves.
        $(element).on("blur", function () {
            viewModel.coalesceConfig.autoSaveEnabled(existingAutoSaveValueRaw);
            // If there were changes, perform an autosave now (autoSave() won't save if autosave is disabled.)
            if (viewModel.isDirty()) {
                viewModel.autoSave();
            }

        });
    }
};


// Binding for Bootstrap ToolTips
// Format: tooltip: {title:note}  (where note is the observable with the value you want)
// Format: tooltip: {title:note, placement: 'bottom', animation: false}  (where note is the observable with the value you want)
// Format: tooltip: note          (where note is the observable with the value you want)
ko.bindingHandlers.tooltip = {
    init: function (element) {
        var $element = $(element);

        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            if ($element.data('bs.tooltip')) {
                $element.tooltip('destroy');
            }
        });
    },

    update: function (element, valueAccessor) {
        var $element = $(element);
        var value = ko.unwrap(valueAccessor());
        var options: any = {};

        if (value === null || typeof value !== 'object') {
            options = value;
        } else {
            ko.utils.objectForEach(value, function (propertyName, propertyValue) {
                options[propertyName] = ko.unwrap(propertyValue);
            });
        }

        if (typeof options !== 'object') {
            options = { title: value }
        }

        var tooltipData = $element.data('bs.tooltip');

        if (!tooltipData) {
            $element.tooltip(options);
        } else {
            ko.utils.extend(tooltipData.options, options);
        }
    }
};


ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).fadeIn() : $(element).fadeOut();
    }
};

ko.bindingHandlers.slideVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).slideDown() : $(element).slideUp();
    }
};

ko.bindingHandlers.booleanValue = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var observable = valueAccessor(),
            interceptor = ko.computed({
                read: function () {
                    return observable().toString();
                },
                write: function (newValue) {
                    observable(newValue === "true");
                }
            });

        ko.applyBindingsToNode(element, { value: interceptor });
    }
};

ko.bindingHandlers.formatNumberText = {
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var phone = ko.utils.unwrapObservable(valueAccessor());
        var formatPhone = function () {
            return phone.replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
        }

        // Satisfy typescript type guards.
        if (!ko.bindingHandlers.text.update) throw "Fatal: text binding missing";

        ko.bindingHandlers.text.update(element, formatPhone, allBindings, viewModel, bindingContext);
    }
};

// http://xion.io/post/code/knockout-let-binding.html
ko.bindingHandlers['let'] = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var innerContext = bindingContext.extend(valueAccessor);
        ko.applyBindingsToDescendants(innerContext, element);
        return { controlsDescendantBindings: true };
    }
};
ko.virtualElements.allowedBindings['let'] = true;


// Used from grahampcharles 
// https://github.com/grahampcharles/moment-knockout/


(function () {

    ko.bindingHandlers.moment = {

        defaults: {
            invalid: '',
            format: 'MM/DD/YYYY'
        },

        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var allBindings = allBindingsAccessor();

            // register change event
            ko.utils.registerEventHandler(element, 'change', function () {

                var observable = valueAccessor();
                var val = moment(($(element).val() || "").toString());

                observable(val);

            });
        },

        update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var value = valueAccessor(),
                allBindings = allBindingsAccessor(),
                valueUnwrapped = ko.utils.unwrapObservable(value);

            // Date formats: http://momentjs.com/docs/#/displaying/format/
            var pattern = allBindings.format || ko.bindingHandlers.moment.defaults.format;
            var invalidString = allBindings.invalid == undefined ? ko.bindingHandlers.moment.defaults.invalid : allBindings.invalid;

            var dateMoment = moment(valueUnwrapped);

            // format string for input box
            var output = dateMoment.isValid() ?
                dateMoment.format(pattern) :
                invalidString;

            if ($(element).is("input") === true) {
                $(element).val(output);
            } else {
                $(element).text(output);
            }

        }
    };







    ko.bindingHandlers.momentFromNow = {

        defaults: {
            invalid: '',
        },

        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var allBindings = allBindingsAccessor();

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                clearInterval(element.koFromNowTimer);
            });
        },

        update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var value = valueAccessor(),
                allBindings = allBindingsAccessor(),
                valueUnwrapped = ko.utils.unwrapObservable(value);

            // Date formats: http://momentjs.com/docs/#/displaying/format/
            var invalidString = allBindings.invalid == undefined ? ko.bindingHandlers.moment.defaults.invalid : allBindings.invalid;
            var shorten = allBindings.shorten == undefined ? false : allBindings.shorten;

            var dateMoment = moment(valueUnwrapped);

            var fmt = () => {
                var output = dateMoment.isValid() ?
                    dateMoment.fromNow() :
                    invalidString;

                if (shorten) {
                    output = output.replace("minutes", "mins");
                    output = output.replace("a few seconds", "a moment");
                }
                return output;
            }

            $(element).text(fmt());

            clearInterval(element.koFromNowTimer);
            element.koFromNowTimer = setInterval(function () {

                $(element).text(fmt());
            }, 1000);

        }
    };
} ());