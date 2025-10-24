import { Property, ViewModel, BehaviorFlags } from "coalesce-vue";

type ValidationResult = string | boolean;
export type TypedValidationRule<T> =
  | ValidationResult
  | PromiseLike<ValidationResult>
  | ((value: T | null | undefined) => ValidationResult)
  | ((value: T | null | undefined) => PromiseLike<ValidationResult>);

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

  const mutableValue = p.role == "referenceNavigation" ? p.foreignKey : p;

  if (mutableValue.role == "primaryKey" && !mutableValue.createOnly) {
    // Primary keys without createOnly are server-generated.
    // They're never client-mutable in any circumstance.
    return true;
  }

  if (mutableValue.createOnly || mutableValue.role == "primaryKey") {
    // Editable create-only fields should only be editable if there either is no value for the PK,
    // or if the PK is dirty. If there's a clean value for the PK, it means it has been saved.

    // If the model is saving, or has ever saved, disable editing the create-only field.
    // Do the same if the model is loading or has ever loaded,
    // since this would also mean that some PK value is known for the instance.
    // This is checked first so editing gets disabled while a save is in progress.
    if (
      model.$save.isLoading ||
      model.$save.wasSuccessful ||
      model.$load.isLoading ||
      model.$load.wasSuccessful
    ) {
      return true;
    }

    // If there's no PK, one has yet to be entered.
    // If there is a PK but it is dirty, it has yet to be saved.
    if (!model.$primaryKey || model.$getPropDirty(mutableValue.name)) {
      return false;
    }

    return true;
  }

  if (p.role == "collectionNavigation") {
    return false;
  }

  return mutableValue.dontSerialize;
}

export type ComponentSlots<T> = T extends new () => { $slots: infer S }
  ? NonNullable<S>
  : T extends (
        props: any,
        ctx: { slots: infer S; attrs: any; emit: any },
        ...args: any
      ) => any
    ? NonNullable<S>
    : object;

export function detectFileFormat(base64: string): string | null {
  // Decode Base64 string
  const byteCharacters = atob(base64);
  const byteArray = new Uint8Array(byteCharacters.length);

  for (let i = 0; i < byteCharacters.length; i++) {
    byteArray[i] = byteCharacters.charCodeAt(i);
  }

  // Convert first few bytes to hex string
  const hex = byteArray
    .slice(0, 16)
    .reduce((acc, byte) => acc + byte.toString(16).padStart(2, "0"), "");

  // cSpell:disable
  const magicNumbers: { [key: string]: string[] } = {
    jpg: ["ffd8ff"],
    png: ["89504e47"],
    gif: ["47494638"],
    pdf: ["25504446"],
    tiff: ["49492a00", "4d4d002a"],
    mp3: ["494433"],
    wav: ["52494646........57415645"], // RIFF + (skip 4 bytes) + WAVE
    webp: ["52494646........57454250"], // RIFF + (skip 4 bytes) + WEBP
    html: [
      "3c21444f43545950452048544d4c", // <!DOCTYPE HTML
      "3c21444f43545950452068746d6c", // <!DOCTYPE html
      "3c21646f63747970652068746d6c", // <!doctype html
      "3c68746d6c", // <html
    ],
    mp4: [
      "........667479706d703432", // ftypmp42 (MP4 v2)
      "........6674797069736f6d", // ftypisom (ISO Base Media)
      "........667479704d534e56", // ftypMSNV
      "........667479704d344120", // ftypM4A (M4A Audio)
      "........667479704d344220", // ftypM4B (M4B Audiobook)
      "........667479704d344350", // ftypM4P (Protected MP4)
    ],
    webm: ["1a45dfa3"], // EBML header (used in WebM and MKV)
  };
  // cSpell:enable

  // Check against known file signatures
  for (const [format, signatures] of Object.entries(magicNumbers)) {
    if (signatures.some((sig) => hex.match(new RegExp("^" + sig)))) {
      return format;
    }
  }

  return null; // Unknown format
}
