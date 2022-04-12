import { Property, ViewModel, BehaviorFlags } from "coalesce-vue";

export function isPropReadOnly(p: Property, model: ViewModel) {
  const metadata = model.$metadata;
  if (!metadata) return false;

  if (
    (metadata.behaviorFlags &
      (model.$primaryKey != null
        ? BehaviorFlags.Edit
        : BehaviorFlags.Create)) ===
    0
  ) {
    return true;
  }

  if (p.role == "primaryKey") {
    // Editable PKs should only be editable if there either is no value for the PK,
    // or if the PK is dirty. If there's a clean value for the PK, it means it has been saved.

    // If the model is saving, or has ever saved, disable editing the PK.
    // Do the same if the model is loading or has ever loaded,
    // since this would also mean that some PK value is known for the instance.
    // This is checked first so editing gets disabled while a save is in progress.
    if (
      model.$save.isLoading ||
      model.$save.wasSuccessful !== null ||
      model.$load.isLoading ||
      model.$load.wasSuccessful !== null
    ) {
      return true;
    }

    // If there's no PK, one has yet to be entered.
    // If there is a PK but it is dirty, it has yet to be saved.
    if (!model.$primaryKey || model.$getPropDirty(p.name)) {
      return false;
    }

    return true;
  }

  return (
    p.dontSerialize &&
    p.role !== "referenceNavigation" &&
    p.role !== "collectionNavigation"
  );
}
