/// <reference path="../coalesce.dependencies.d.ts" />

module Coalesce {
    export module KnockoutUtilities {

        function BuildLookup<T>(array: T[], idField: string) {
            const lookup: { [k: string]: T; } = {};
            for (let i = 0; i < array.length; i++) {
                const item = array[i];
                const key = ko.unwrap((item as any)[idField]);

                // If an item is missing a value for a key, we can't look it up.
                // This is OK, because keyless items will never match an incoming item anyway.
                if (key != null) lookup[key.toString()] = item;
            }
            return lookup;
        }

        function GetMatchingItem<U, T>(
            originalContent: U[],
            incomingItem: T,
            incomingItemIndex: number,
            originalLookup: { [k: string]: U; } | null = null,
            idField: string | null = null,
            equalityComparer: ((existingItem: U, incomingKey: any) => boolean) | null = null
        ) {
            var matchingItem: U | null;
            if (idField) {
                const key = ko.unwrap((incomingItem as any)[idField]);
                if (originalLookup) {
                    matchingItem = originalLookup[key.toString()];
                } else {
                    if (!equalityComparer) throw "Equality comparer is required if no originalLookup is provided with an idField."
                    const matchingItems = originalContent.filter(item => equalityComparer(item, key));

                    if (matchingItems.length > 1) {
                        // We have a problem because keys are duplicated.
                        throw `Found duplicate items by key (name:${idField}) when rebuilding array.`
                    } else {
                        matchingItem = matchingItems.length > 0 ? matchingItems[0] : null;
                    }
                }
            } else {
                matchingItem = originalContent[incomingItemIndex];
            }

            return matchingItem;
        }

        // Function to marge two arrays based on data from the server
        export function RebuildArray<U extends LoadableViewModel, T extends object>(
            existingArray: KnockoutObservableArray<U>,
            incomingArray: T[],
            idField: string,
            viewModelClass: new () => U,
            parent: any,
            allowCollectionDeletes: boolean = true,
            equalityComparer: ((existingItem: U, incomingKey: any) => boolean) | null = null
        ) {

            var originalContent = existingArray() || [];

            // We're going to build a new array from scratch.
            // If we spliced and pushed the existing array one row at a time as needed,
            // it performs much more slowly, and also rebuilds the DOM in realtime as that happens.
            // This is because each push/splice triggers all subscribers to update.
            // If there are expensive subscriptions (not just the DOM - custom application code as well),
            // then performance drops off the edge of a cliff into a firey abyss.

            // Knockout is smart enough when we update the value of existingArray with newArray
            // to figure out exactly what changed, and will only rebuild the DOM as needed,
            // instead of rebuilding the entire thing: http://stackoverflow.com/a/18050443.
            // However, there will ALWAYS be one single notification to subscribers, even if we didn't actually change the array.
            // If arrays are being rebuilt frequently, this "false" subscriber notification could be detrimental to performance.
            // To prevent this from happening, at the bottom of this function we perform an array comparison before updating the final observable.
            var newContent: Array<U> = [];

            // If no specific equality comparison has been requested,
            // use a hash table for O(1) lookups on a single key to prevent O(n^2) from nested for-loops.
            let originalLookup: { [k: string]: U; } | null = null;
            if (equalityComparer == null && idField) {
                originalLookup = BuildLookup(originalContent, idField);
            }

            // Can't do for (var i in array) because IE sees new methods added on to the prototype as keys
            for (let i = 0; i < incomingArray.length; i++) {
                var inItem = incomingArray[i];
                var matchingItem = GetMatchingItem(originalContent, inItem, i, originalLookup, idField, equalityComparer);

                if (matchingItem == null) {
                    // This is a brand new item that we don't already have an object for.
                    // We need to construct a new object and stick it in our newArray.
                    var newItem = new viewModelClass();
                    newItem.loadFromDto(inItem);
                    newItem.parent = parent;
                    newItem.parentCollection = existingArray;
                    newContent.push(newItem);
                } else {
                    // We already have an object for this item.
                    // Stick the existing object into our new array, and then reload it from the DTO.
                    newContent.push(matchingItem);

                    // Only reload the item if it is not dirty. If it is dirty, there are user-made changes
                    // that aren't yet saved that we shouldn't be overwriting.

                    if (!(matchingItem instanceof BaseViewModel) || !matchingItem.isDirty()) {
                        matchingItem.loadFromDto(inItem);
                    }

                    if (!allowCollectionDeletes) {
                        // This item is already in the collection, and we're not allowing not-found items to be deleted from the collection.
                        // We're going to do a pass of everything that was in the original collection at this end of this method,
                        // where we'll add everything from the original collection to the new collection.
                        // We need to remove the current item from the original collection so it doesn't get added again when we do that.
                        originalContent.splice(originalContent.indexOf(matchingItem), 1);
                    }
                }
            }

            // If we are not allowing deletes.
            if (!allowCollectionDeletes) {
                // If we aren't allowing deletes, we need to add all items from the original collection
                // to the new collection that we haven't already added. At this point, originalContent contains that set.

                // Note that this used to only re-insert items that are dirty,
                // but that didn't make any sense, and there was no comment that said why it was done that way.
                // So, we're just going to add in everything from originalContent.
                newContent.unshift(...originalContent);
            }

            if (newContent.length == originalContent.length &&
                ko.utils.compareArrays(newContent, originalContent).every(c => c.status == "retained")) {
                // Everything is the same (by doing a shallow equality check of the array - objects are checked by reference).
                // Shallow equality check by reference is perfectly in line with the spec for ObservableArray.

                // Do nothing.
            } else {
                // Something is different. Update the observable.
                // See the comments at the top of this method for why we do this conditionally.
                // Basically, its because this call ALWAYS notifies subscribers, but we can be more intelligent about it.
                existingArray(newContent);
            }
        }


        export function RebuildArrayInPlace<T>(
            existingArray: KnockoutObservableArray<T>,
            incomingArray: T[] | KnockoutObservableArray<T>,
            idField?: string,
            equalityComparer: ((existingItem: T, incomingKey: any) => boolean) | null = null
        ) {
            var incomingArrayUnwrapped = ko.unwrap(incomingArray);
            var originalContent = existingArray().slice();

            // If no specific equality comparison has been requested,
            // use a hash table for O(1) lookups on a single key to prevent O(n^2) from nested for-loops.
            let originalLookup: { [k: string]: T; } | null = null;
            if (equalityComparer == null && idField) {
                originalLookup = BuildLookup(originalContent, idField);
            }

            for (let i = 0; i < incomingArrayUnwrapped.length; i++) {
                var inItem = incomingArrayUnwrapped[i];
                var matchingItem = GetMatchingItem(originalContent, inItem, i, originalLookup, idField, equalityComparer);

                if (matchingItem == null) {
                    // Add this to the observable collection
                    existingArray.push(inItem);
                } else {
                    // Remove this one from the copy so we don't remove it later.
                    originalContent.splice(originalContent.indexOf(matchingItem), 1);
                }
            }

            // Remove any items that we didn't find in the incoming array.
            for (var i in originalContent) {
                existingArray.splice(existingArray.indexOf(originalContent[i]), 1);
            }
        }
    }
}