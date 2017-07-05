/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="coalesce.utilities.ts" />
/// <reference path="coalesce.ko.base.ts" />

module Coalesce {
    export module KnockoutUtilities {

        // Function to marge two arrays based on data from the server
        export function RebuildArray<T>(observableArray: KnockoutObservableArray<T>, incomingArray: T[], idField: string, viewModelClass, parent, allowCollectionDeletes: boolean = true) {

            var originalContent: Array<T> = observableArray();
            // We're going to build a new array from scratch.
            // If we spliced and pushed the existing one row at a time as needed,
            // it performs much more slowly, and also rebuilds the DOM in realtime as that happens.
            // Knockout is smart enough when we update the value of observableArray with newArray
            // to figure out exactly what changed, and will only rebuild the DOM as needed,
            // instead of rebuilding the entire thing: http://stackoverflow.com/a/18050443
            var newArray: Array<T> = [];

            // Can't do for (var i in array) because IE sees new methods added on to the prototype as keys
            for (var i = 0; i < incomingArray.length; i++) {
                var newItem;
                var inItem = incomingArray[i];
                var key = inItem[idField] || inItem["id"];
                var matchingItems = idField ? originalContent.filter(
                    function (value) {
                        return value[Coalesce.Utilities.lowerFirstLetter(idField)]() == key;
                    }
                ) : [originalContent[i] ];
                if (matchingItems.length == 0 || typeof (matchingItems[0]) === 'undefined') {
                    // This is a brand new item that we don't already have an object for.
                    // We need to construct a new object and stick it in our newArray.
                    newItem = new viewModelClass(inItem)
                    newItem.parent = parent;
                    newItem.parentCollection = observableArray;
                    newArray.push(newItem);
                } else if (matchingItems.length == 1) {
                    // We already have an object for this item.
                    // Stick the existing object into our new array, and then reload it from the DTO.
                    newArray.push(matchingItems[0]);

                    // Only reload the item if it is not dirty. If it is dirty, there are user-made changes
                    // that aren't yet saved that we shouldn't be overwriting.
                    if (typeof((<any>matchingItems[0]).isDirty) === 'undefined' || !(<any>matchingItems[0]).isDirty()) {
                        (<any>matchingItems[0]).loadFromDto(inItem);
                    }

                    if (!allowCollectionDeletes) {
                        // This item is already in the collection, and we're not allowing not-found items to be deleted from the collection.
                        // We're going to do a pass of everything that was in the original collection at this end of this method,
                        // where we'll add everything from the original collection to the new collection.
                        // We need to remove the current item from the original collection so it doesn't get added again when we do that.
                        originalContent.splice(originalContent.indexOf(matchingItems[0]), 1);
                    }
                } else {
                    // We have a problem because keys are duplicated.
                }
            }

            // If we are not allowing deletes.
            if (!allowCollectionDeletes) {
                // If we aren't allowing deletes, we need to add all items from the original collection
                // to the new collection that we haven't already added. At this point, originalContent contains that set.

                // Note that this used to only re-insert items that are dirty,
                // but that didn't make any sense, and there was no comment that said why it was done that way.
                // So, we're just going to add in everything from originalContent.
                newArray.unshift(...originalContent);
            }

            observableArray(newArray);
        }


        export function RebuildArrayInPlace<T>(existingArray: KnockoutObservableArray<T>, incomingArray: T[] | KnockoutObservableArray<T>, idField: string) {
            var incomingArrayUnwrapped = ko.unwrap(incomingArray);
            var existingArrayCopy = existingArray().slice();

            for (var i in incomingArrayUnwrapped) {
                var newItem;
                var inItem = incomingArrayUnwrapped[i];
                var key = inItem[idField] || inItem["id"];
                var matchingItems = idField ? existingArrayCopy.filter(
                    function (value) {
                        return value[Coalesce.Utilities.lowerFirstLetter(idField)]() == ko.utils.unwrapObservable(key);
                    }
                ) : [existingArrayCopy[i]];

                if (matchingItems.length == 0 || typeof (matchingItems[0]) === 'undefined') {
                    // Add this to the observable collection
                    existingArray.push(inItem);
                } else if (matchingItems.length == 1) {
                    // Remove this one from the copy so we don't remove it later.
                    existingArrayCopy.splice(existingArrayCopy.indexOf(matchingItems[0]), 1);
                } else {
                    // We have a problem because keys are duplicated.
                }
            }

            // Remove any items that we didn't find in the incoming array.
            for (var i in existingArrayCopy) {
                existingArray.splice(existingArray.indexOf(existingArrayCopy[i]), 1);
            }
        }
    }
}