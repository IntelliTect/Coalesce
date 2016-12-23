// Function to marge two arrays based on data from the server
function RebuildArray(observableArray, incomingArray, idField, viewModelClass, parent, allowCollectionDeletes: boolean = true) {

    var obsArrayContent;
    if (allowCollectionDeletes) {
        // Move all the items to a new array so we can populate the original one.
        obsArrayContent = observableArray.splice(0, observableArray().length);
    } else {
        // Use the original array because we aren't removing anything.
        obsArrayContent = observableArray();
    }
    // Can't do for (var i in array) because IE sees new methods added on to the prototype as keys
    for (var i = 0; i < incomingArray.length; i++) {
        var newItem;
        var inItem = incomingArray[i];
        var key = inItem[idField] || inItem.id;
        var matchingItems = idField ? obsArrayContent.filter(
            function (value) {
                return value[intellitect.utilities.lowerFirstLetter(idField)]() == key;
            }
        ) : [ obsArrayContent[i] ];
        if (matchingItems.length == 0 || typeof (matchingItems[0]) === 'undefined') {
            // Add this to the observable collection
            newItem = new viewModelClass(inItem)
            newItem.parent = parent;
            newItem.parentCollection = observableArray;
            observableArray.push(newItem);
        } else if (matchingItems.length == 1) {
            // See if the item is dirty. If so, leave it so we don't overwrite changes.
            if (typeof(matchingItems[0].isDirty) === 'undefined' || !matchingItems[0].isDirty()) {
                matchingItems[0].loadFromDto(inItem);
            }
            // use the intermediary collection if we are not preserving, allowing deletes.
            if (allowCollectionDeletes) {
                // This is an update. Update it and add it to the new collection.
                observableArray.push(matchingItems[0]);
                // Remove this on from the collection so we don't add it later.
                obsArrayContent.splice(obsArrayContent.indexOf(matchingItems[0]), 1);
            }
        } else {
            // We have a problem because keys are duplicated.
        }
    }

    // If we are not allowing deletes.
    if (allowCollectionDeletes) {
        // Add any items that are already there but are still dirty.
        for (var i = 0; i < obsArrayContent.length; i++) {
        //for (var i in obsArrayContent) {
            var existingItem = obsArrayContent[i];
            if (typeof(existingItem.isDirty) !== 'undefined' && existingItem.isDirty()) {
                observableArray.push(existingItem);
            }
        }
    }
}


function RebuildArrayInPlace(existingArray, incomingArray, idField) {
    var incomingArrayUnwrapped = ko.unwrap(incomingArray);
    var existingArrayCopy = existingArray().slice();

    for (var i in incomingArrayUnwrapped) {
        var newItem;
        var inItem = incomingArrayUnwrapped[i];
        var key = inItem[idField] || inItem.id;
        var matchingItems = idField ? existingArrayCopy.filter(
            function (value) {
                return value[intellitect.utilities.lowerFirstLetter(idField)]() == ko.utils.unwrapObservable(key);
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

// Used by tooltip from Knockstrap
function unwrapProperties(wrappedProperies) {

    if (wrappedProperies === null || typeof wrappedProperies !== 'object') {
        return wrappedProperies;
    }

    var options = {};

    ko.utils.objectForEach(wrappedProperies, function (propertyName, propertyValue) {
        options[propertyName] = ko.unwrap(propertyValue);
    });

    return options;
};

