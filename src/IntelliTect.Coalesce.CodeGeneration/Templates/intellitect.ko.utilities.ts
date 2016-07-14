// Function to marge two arrays based on data from the server
function RebuildArray(observableArray, incomingArray, idField, viewModelClass, parent) {
    // Move all the items to a new array so we can populate the original one.
    var obsArrayContent = observableArray().splice(0, observableArray().length);
    for (var i in incomingArray) {
        var newItem;
        var inItem = incomingArray[i];
        var key = inItem[idField] || inItem.id;
        var matchingItems = obsArrayContent.filter(
            function (value) {
                return value[intellitect.utilities.lowerFirstLetter(idField)]() == key;
            }
        );
        if (matchingItems.length == 0) {
            // Add this to the observable collection
            newItem = new viewModelClass(inItem)
            newItem.parent = parent;
            newItem.parentCollection = observableArray;
            observableArray.push(newItem);
        } else if (matchingItems.length == 1) {
            // See if the item is dirty. If so, leave it so we don't overwrite changes.
            if (!matchingItems[0].isDirty()) {
                matchingItems[0].loadFromDto(inItem);
            }
            // This is an update. Update it and add it to the new collection.
            observableArray.push(matchingItems[0]);
            // Remove this on from the collection so we don't add it later.
            obsArrayContent.splice(obsArrayContent.indexOf(matchingItems[0]), 1);
        } else {
            // We have a problem because keys are duplicated.
        }
    }
    // Add any items that are already there but are still dirty.
    for (var i in obsArrayContent) {
        var existingItem = obsArrayContent[i];
        if (existingItem.isDirty()) {
            observableArray.push(existingItem);
        }
    }
}

// Used by tooltip from Knockstrap
function unwrapProperties (wrappedProperies) {

    if (wrappedProperies === null || typeof wrappedProperies !== 'object') {
        return wrappedProperies;
    }

    var options = {};

    ko.utils.objectForEach(wrappedProperies, function (propertyName, propertyValue) {
        options[propertyName] = ko.unwrap(propertyValue);
    });

    return options;
};

