// This will tree shake correctly as of v2.0.0-alpha.21
import { formatDistanceToNow, lightFormat } from "date-fns";

// These weird imports from date-fns-tz are needed because date-fns-tz
// doesn't define its esm exports from its root correctly.
// https://github.com/marnusw/date-fns-tz/blob/0577249fb6c47ad7b6a84826e90d976dac9ab52e/README.md#esm-and-commonjs
import format from "date-fns-tz/esm/format";
import utcToZonedTime from "date-fns-tz/esm/utcToZonedTime";

import type {
  ClassType,
  Property,
  PropNames,
  Value,
  EnumValue,
  PrimitiveValue,
  DateValue,
  CollectionValue,
  DataSourceType,
  ModelValue,
  ObjectValue,
  FileValue,
  ModelType,
  BinaryValue,
  UnknownValue,
} from "./metadata.js";
import { resolvePropMeta } from "./metadata.js";
import {
  type Indexable,
  isNullOrWhitespace,
  type VueInstance,
  parseJSONDate,
} from "./util.js";

/**
 * Represents a model with metadata information.
 */
export interface Model<TMeta extends ClassType = ClassType> {
  readonly $metadata: TMeta;
}

/**
 * Represents a data source with metadata information and parameter values.
 */
export interface DataSource<TMeta extends DataSourceType> {
  readonly $metadata: TMeta;
}

abstract class Visitor<TValue = any, TArray = any[], TObject = any> {
  public visitValue(value: any, meta: Value): TValue | TArray | TObject {
    switch (meta.type) {
      case undefined:
        throw "Missing type on value metadata";

      case "model":
      case "object":
        return this.visitObject(value, meta.typeDef);

      case "collection":
        return this.visitCollection(value, meta);

      case "enum":
        return this.visitEnumValue(value, meta);

      case "date":
        return this.visitDateValue(value, meta);

      case "file":
        return this.visitFileValue(value, meta);

      case "binary":
        return this.visitBinaryValue(value, meta);

      case "unknown":
        return this.visitUnknownValue(value, meta);

      default:
        return this.visitPrimitiveValue(value, meta);
    }
  }

  public abstract visitObject(value: any, meta: ClassType): TObject;

  protected abstract visitCollection(
    value: any[],
    meta: CollectionValue
  ): TArray;

  protected abstract visitPrimitiveValue(
    value: any,
    meta: PrimitiveValue
  ): TValue;

  protected abstract visitDateValue(value: any, meta: DateValue): TValue;

  protected abstract visitFileValue(value: any, meta: FileValue): TValue;

  protected abstract visitBinaryValue(value: any, meta: BinaryValue): TValue;

  protected abstract visitEnumValue(value: any, meta: EnumValue): TValue;

  protected abstract visitUnknownValue(value: any, meta: UnknownValue): TValue;
}

function parseError(
  value: any,
  meta: { type: string; name: string },
  details?: string
) {
  return new Error(
    `Encountered unparsable ${typeof value} \`${value}\` for ${meta.type} '${
      meta.name
    }'. ${details || ""}`
  );
}

/**
 * Attempts to change the input value into a correctly-typed
 * result given some metadata describing the desired result.
 * Values that cannot be converted will throw an error.
 */
export function parseValue(value: null | undefined, meta: Value): null;
export function parseValue(
  value: any,
  meta: PrimitiveValue
): null | string | number | boolean;
export function parseValue(
  value: any,
  meta: PrimitiveValue & { type: "string" }
): null | string;
export function parseValue(
  value: any,
  meta: PrimitiveValue & { type: "number" }
): null | number;
export function parseValue(
  value: any,
  meta: PrimitiveValue & { type: "boolean" }
): null | boolean;
export function parseValue(value: any, meta: EnumValue): null | number;
export function parseValue(value: any, meta: DateValue): null | Date;
export function parseValue(value: any, meta: FileValue): null | Blob | File;
export function parseValue(
  value: any,
  meta: BinaryValue
): null | Uint8Array | string;
export function parseValue(value: any, meta: ModelValue): null | object;
export function parseValue(value: any, meta: ObjectValue): null | object;
export function parseValue(value: any, meta: UnknownValue): null | unknown;
export function parseValue(value: any[], meta: CollectionValue): Array<any>;
export function parseValue(
  value: any,
  meta: Value
): null | string | number | boolean | object | Date | Array<any> | unknown {
  if (value == null) {
    return null;
  }

  const type = typeof value;

  switch (meta.type) {
    case "number":
    case "enum":
      if (type === "number") return value;

      if (type !== "string") {
        // We don't want to parse things like booleans into numbers.
        // Strings are all we should be handling.
        throw parseError(value, meta);
      }

      // Parse empty/blank as null, not zero.
      // If we parsed it as zero, clearing out an number input field
      // would default numbers to zero, even if that number is nullable.
      if (isNullOrWhitespace(value)) {
        return null;
      }

      const parsed = Number(value);
      if (isNaN(parsed)) {
        throw parseError(value, meta);
      }
      return parsed;

    case "string":
      if (type === "string") return value;
      if (type === "object") throw parseError(value, meta);
      return String(value);

    case "boolean":
      if (type === "boolean") return value;
      if (value === "true") return true;
      if (value === "false") return false;
      throw parseError(value, meta);

    case "collection":
      if (type !== "object" || !Array.isArray(value))
        throw parseError(value, meta);

      return value;

    case "model":
    case "object":
      if (type !== "object" || Array.isArray(value))
        throw parseError(value, meta);

      return value;

    case "date":
      let date: Date | undefined = undefined;
      if (value instanceof Date) {
        date = value;
      } else if (type === "string") {
        date = parseJSONDate(value);
      }

      // isNaN is what date-fn's `isValid` calls internally,
      // but we can perform better if we just do it directly.
      if (date === undefined || isNaN(date as any)) {
        throw parseError(value, meta);
      }

      return date;

    case "file":
      if (value instanceof Blob) return value;
      throw parseError(value, meta);

    case "binary":
      if (typeof value == "string" || value instanceof Uint8Array) return value;
      if (meta.base64 && typeof value !== "string") {
        throw parseError(
          value,
          meta,
          "Value only allows base64 string representations."
        );
      }
      if (value instanceof ArrayBuffer) return new Uint8Array(value);
      throw parseError(value, meta);
  }
}

class ModelConversionVisitor extends Visitor<any, any[] | null, any | null> {
  private objects = new Map<object, object>();

  public override visitValue(value: any, meta: Value): any {
    // Models shouldn't contain undefined - only nulls where a value isn't present.
    if (value === undefined) return null;

    return super.visitValue(value, meta);
  }

  public visitObject<TMeta extends ClassType>(
    value: object,
    meta: TMeta
  ): Model<TMeta>;
  public visitObject<TMeta extends ClassType>(value: null, meta: TMeta): null;
  public visitObject<TMeta extends ClassType>(
    value: any,
    meta: TMeta
  ): null | Model<TMeta> {
    if (value == null) return null;

    if (typeof value !== "object" || Array.isArray(value))
      throw parseError(value, meta);

    // Prevent infinite recursion on circular object graphs.
    if (this.objects.has(value))
      return this.objects.get(value)! as Model<TMeta>;

    const props = meta.props;

    let target: Indexable<Model<TMeta>>;
    if (this.mode == "convert") {
      this.objects.set(value, value);

      // If there already is metadata but it doesn't match,
      // this is bad - someone passed mismatched parameters.

      if ("$metadata" in value) {
        if (value.$metadata !== meta) {
          throw Error(
            `While trying to convert object, found metadata for ${value.$metadata.name} where metadata for ${meta.name} was expected.`
          );
        } else {
          // Performance optimization: If the object already looks like a model,
          // and we're converting, don't descend into its child props,
          // because they should already be correct.
          // If they *aren't* correct, something is messed up somewhere else
          // and is assigining non-models to props that should only be accepting models.

          // If we don't do this optimization, we can end up with horrible call stacks
          // when creating a new model with initial data that includes references
          // to existing models that have an extensive amount of interconnected data.
          return value;
        }
      } else {
        value.$metadata = meta;
        target = value;
      }
    } else if (this.mode == "map") {
      target = { $metadata: meta };
      this.objects.set(value, target);
    } else {
      throw `Unhandled mode ${this.mode}`;
    }

    this.objects.set(value, target);

    for (const propName in props) {
      const propVal = value[propName];
      if (!(propName in value) || propVal === undefined) {
        // All properties that are not defined need to be declared
        // so that Vue's reactivity system can discover them.
        // Null is a valid type for all model properties (or at least generated models). Undefined is not.
        target[propName] = null;
      } else {
        target[propName] = this.visitValue(propVal, props[propName]);
      }
    }

    return target;
  }

  protected visitCollection(value: any[] | null, meta: CollectionValue) {
    if (value == null) return null;
    const parsed = parseValue(value, meta);

    const ret = this.mode == "map" ? [] : parsed;

    let j = 0;
    for (let i = 0; i < parsed.length; i++, j++) {
      const value = parsed[i];
      if (
        value === null &&
        (meta.itemType.type === "object" || meta.itemType.type === "model")
      ) {
        // Skip nulls in collections, a workaround for
        // https://github.com/dotnet/runtime/issues/66187
        // https://github.com/dotnet/runtime/issues/76211
        j--;
        continue;
      }
      ret[j] = this.visitValue(value, meta.itemType);
    }

    if (ret.length != j) {
      // If we skipped one or more values above, shorten the array
      // to the correct length in case we were in "convert" mode.
      for (let i = j; i < ret.length; i++) {
        delete ret[i];
      }
      ret.length = j;
    }

    return ret;
  }

  protected visitDateValue(value: any, meta: DateValue) {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    if (this.mode == "convert") {
      // Preserve object ref when converting.
      return parsed;
    } else if (this.mode == "map") {
      // If the parsed result is the same ref as the incoming value when mapping,
      // get a new object ref. Otherwise, return `parsed` since it must be a new Date
      // that was constructed by parseValue.
      return value === parsed ? new Date(parsed) : parsed;
    }
  }

  protected visitEnumValue(value: any, meta: EnumValue) {
    return parseValue(value, meta);
  }

  protected visitPrimitiveValue(value: any, meta: PrimitiveValue) {
    return parseValue(value, meta);
  }

  protected visitFileValue(value: any, meta: FileValue) {
    return parseValue(value, meta);
  }

  protected visitBinaryValue(value: any, meta: BinaryValue) {
    return parseValue(value, meta);
  }

  protected visitUnknownValue(value: any, meta: UnknownValue) {
    return value;
  }

  constructor(private mode: "map" | "convert") {
    super();
  }
}

/**
 * Transforms a given object with data properties into a valid implementation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param value The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function convertToModel<
  TModel extends Model<TMeta>,
  TMeta extends ClassType = TModel["$metadata"]
>(object: { [k: string]: any }, metadata: TMeta): TModel;
/**
 * Transforms a raw value into a valid implementation of a model value.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export function convertToModel(value: any, metadata: Value): any;
export function convertToModel(value: any, metadata: Value | ClassType): any {
  if ("props" in metadata) {
    return new ModelConversionVisitor("convert").visitObject(value, metadata);
  } else {
    return new ModelConversionVisitor("convert").visitValue(value, metadata);
  }
}
export { convertToModel as convertValueToModel };

/**
 * Maps the given object with data properties into a valid implementation of TModel.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param object The object with data properties that should be mapped to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function mapToModel<
  TModel extends Model<TMeta>,
  TMeta extends ClassType = TModel["$metadata"]
>(object: { [k: string]: any }, metadata: TMeta): TModel;
/**
 * Maps a raw value into a valid implementation of a model value.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export function mapToModel(value: any, metadata: Value): any;
export function mapToModel(value: any, metadata: Value | ClassType): any {
  if ("props" in metadata) {
    return new ModelConversionVisitor("map").visitObject(value, metadata);
  } else {
    return new ModelConversionVisitor("map").visitValue(value, metadata);
  }
}
export { mapToModel as mapValueToModel };

/** Visitor that maps a model to a DTO.
 * A DTO in this case is a POJO that is suitable for JSON stringification
 * for purposes of transport via an HTTP API.
 */
class MapToDtoVisitor extends Visitor<
  null | undefined | string | number | boolean,
  null | undefined | string[] | number[] | boolean[] | object[],
  null | undefined | object
> {
  private depth: number = 0;

  public visitObject(value: any, meta: ClassType): {} | null | undefined {
    // If we've exceded max depth, return undefined to prevent the
    // creation of an entry in the parent object for this object.
    if (this.depth >= this.maxObjectDepth) return undefined;
    this.depth++;

    if (value == null) return null;
    const props = meta.props;
    const output: any = {};
    for (const propName in props) {
      const propMeta = props[propName];

      if (propMeta.dontSerialize) continue;

      if (propName in value) {
        const newValue = this.visitValue(value[propName], propMeta);
        if (newValue !== undefined) {
          // Only store values that aren't undefined.
          // We don't support any properties with undefined as their value - we shouldn't define these in the first place.
          output[propName] = newValue;
        }
      }

      // This prop is a foreign key, and it has no value.
      // Lets check and see if the corresponding referenceNavigation prop has an object in it.
      // If it does, try and use that object's primary key as the value of our FK.
      if (
        output[propName] == null &&
        propMeta.role == "foreignKey" &&
        propMeta.navigationProp
      ) {
        const objectValue = value[propMeta.navigationProp.name];
        if (objectValue) {
          const objectValuePkValue =
            objectValue[propMeta.navigationProp.typeDef.keyProp.name];
          if (objectValuePkValue != null) {
            output[propName] = objectValuePkValue;
          }
        }
        propMeta.principalType.keyProp.name;
      }
    }

    this.depth--;

    return output;
  }

  protected visitCollection(value: any[] | null, meta: CollectionValue) {
    if (this.depth >= this.maxObjectDepth) {
      // If we've exceded max depth, return undefined to prevent the
      // creation of an entry in the parent object for this collection.
      return undefined;
    }

    if (value == null) return null;
    const parsed = parseValue(value, meta);

    const ret = [];
    for (let i = 0; i < parsed.length; i++) {
      ret[i] = this.visitValue(parsed[i], meta.itemType);
    }
    return ret as any[];
  }

  protected visitDateValue(value: any, meta: DateValue) {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    // We must exclude timezone (XXX) for DateTime, but keep it for DateTimeOffset.
    // This is needed to keep a timezone-agnostic DateTime from being translated
    // into a different timezone when we serialize it.
    if (meta.noOffset) {
      return format(parsed, "yyyy-MM-dd'T'HH:mm:ss.SSS");
    } else {
      return format(parsed, "yyyy-MM-dd'T'HH:mm:ss.SSSXXX");
    }
  }

  protected visitFileValue(value: any, meta: FileValue): undefined {
    throw new Error("Files cannot be serialized to JSON.");
  }

  protected visitBinaryValue(value: any, meta: BinaryValue) {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    if (typeof parsed == "string") {
      // Assume strings in the position of a binary are base64.
      return parsed;
    }

    throw new Error("Unexpected raw binary value in JSON context");
  }

  protected visitPrimitiveValue(value: any, meta: PrimitiveValue) {
    return parseValue(value, meta);
  }

  protected visitEnumValue(value: any, meta: EnumValue) {
    return parseValue(value, meta);
  }

  protected visitUnknownValue(value: any, meta: UnknownValue) {
    return value;
  }

  /**
   * Create a new DTO mapper, allowing recursive mapping up to the specified depth.
   * @param maxObjectDepth The maximum depth to serialize objects and collections at.
   * Any property with `propMeta.dontSerialize == true` will always be ignored, regardless of depth.
   */
  constructor(private maxObjectDepth: number = 3) {
    // Depth of 3 here is a 'sensible default' for cases where object/collection properties
    // have been marked as serializable. Most of the time, no depth will be reached because
    // the default in the generated metadata is that no objects/collections are serializable.
    super();
  }
}

/**
 * Maps the given object to a POJO suitable for JSON serialization.
 * Will not serialize child objects or collections whose metadata includes `dontSerialize`.
 * @param object The object to map.
 */
export function mapToDto<T extends Model<ClassType>>(
  object: T | null | undefined
): null | undefined | ReturnType<MapToDtoVisitor["visitObject"]>;
/**
 * Maps the given value to a representation suitable for JSON serialization.
 * Will not serialize child objects or collections whose metadata includes `dontSerialize`.
 * @param value The value to map.
 * @param metadata The metadata that describes the value.
 */
export function mapToDto(
  value: any,
  metadata: Value
): null | ReturnType<MapToDtoVisitor["visitValue"]>;
export function mapToDto(value: any, metadata?: Value | ClassType): any {
  if (value == null) {
    // Values (non-objects) (which require metadata) should return the original value - either null or undefined.
    // I'm not sure how strictly necessary this is, but I didn't want to break the tests with this refactor.
    if (metadata) return value;
    // Objects always return null.
    return null;
  }

  if (metadata === undefined) {
    if ("$metadata" in value) {
      metadata = value.$metadata;
    }
  }

  if (!metadata) {
    throw "mapToDto requires metadata.";
  }

  if ("props" in metadata) {
    return new MapToDtoVisitor().visitObject(value, metadata);
  } else {
    return new MapToDtoVisitor().visitValue(value, metadata);
  }
}

export { mapToDto as mapValueToDto };

/**
 * Maps the given object to a POJO suitable for JSON serialization.
 * Will not serialize child objects or collections whose metadata includes `dontSerialize`.
 * @param object The object to map.
 * @param props A whitelisted set of props to include from the mapped object.
 * Unspecified props will be ignored. If null or undefined, all props will be included.
 */
export function mapToDtoFiltered<T extends Model<ClassType>>(
  object: T | null | undefined,
  props?: PropNames<T["$metadata"]>[] | null
): {} | null {
  var dto = mapToDto(object) as any;

  if (props && dto) {
    const filteredDto: any = {};
    for (const field of props) {
      if (field in dto) {
        filteredDto[field] = dto[field];
      }
    }
    return filteredDto;
  }

  return dto;
}

export interface DisplayOptions {
  /** Date format options. One of:
   * - A UTS#35 date format string (https://date-fns.org/docs/format)
   * - An object with options for https://date-fns.org/docs/format or https://github.com/marnusw/date-fns-tz#format, including a string `format` for the format itself. If a `timeZone` option is provided per https://github.com/marnusw/date-fns-tz#format, the date being formatted will be converted to that timezone.
   * - An object with options for https://date-fns.org/docs/formatDistance */
  format?:
    | string
    | ({
        /** A UTS#35 date format string (https://date-fns.org/docs/format) */
        format: string;
      } & Parameters<typeof format>[2])
    | {
        /** Format date with https://date-fns.org/docs/formatDistanceToNow */
        distance: true;
        /** Append/prepend `'in'` or `'ago'` if date is after/before now. Default `true`. */
        addSuffix?: boolean;
        /** Include detail smaller than one minute. Default `false`. */
        includeSeconds?: boolean;
      };

  collection?: {
    /** The maximum number of items to display individually.
     * When there are more than this number of items, the count of items will be displayed instead.
     * Default `5`.
     * */
    enumeratedItemsMax?: number;

    /** The separator to place between enumerated items. Default `', '` */
    enumeratedItemsSeparator?: string;
  };
}

let defaultTimeZone: string | null = null;
/** Set the timezone that will be used to display dates
 * via the `modelDisplay`/`propDisplay`/`valueDisplay` functions
 * if no other timezone is provided as a parameter,
 * and for c-datetime-picker in coalesce-vue-vuetify.
 * @param tzName An IANA tzdb timezone name, or null to clear this setting.
 */
export function setDefaultTimeZone(tzName: string | null) {
  defaultTimeZone = tzName;
}
/** Returns the current default timezone as provided to `setDefaultTimeZone`. */
export function getDefaultTimeZone() {
  return defaultTimeZone;
}

/** Visitor that maps its input to a string representation of its value, suitable for display. */
class DisplayVisitor extends Visitor<
  string | null,
  string | null,
  string | null
> {
  constructor(private options?: DisplayOptions) {
    super();
  }

  public visitObject(value: any, meta: ClassType): string | null {
    if (value == null) return value;

    if ("displayProp" in meta && meta.displayProp) {
      return this.visitValue(value[meta.displayProp.name], meta.displayProp);
    } else {
      // https://stackoverflow.com/a/46908358 - stringify only first-level properties.
      // With a tweak to also not serialize the $metadata prop.
      try {
        return JSON.stringify(value, (k, v) =>
          k === "$metadata" ? undefined : k ? "" + v : v
        );
      } catch {
        return value.toLocaleString();
      }
    }
  }

  protected visitCollection(
    value: any[] | null,
    meta: CollectionValue
  ): string | null {
    if (value == null) return null;
    value = parseValue(value, meta);

    // Is this what we want? I think so - its the cleanest option.
    // Perhaps an prop that controls this would be best.
    if (value.length == 0) return "";

    const { enumeratedItemsMax = 5, enumeratedItemsSeparator = ", " } =
      this.options?.collection ?? {};

    if (value.length <= enumeratedItemsMax) {
      return value
        .map<string>(
          (childItem) => this.visitValue(childItem, meta.itemType) || "???" // TODO: what should this be for un-displayable members of a collection?
        )
        .join(enumeratedItemsSeparator);
    }

    return (
      value.length.toLocaleString() + " item" + (value.length == 1 ? "" : "s")
    ); // TODO: i18n
  }

  protected visitEnumValue(value: any, meta: EnumValue): string | null {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    const enumData = meta.typeDef.valueLookup[value];

    // If we can't find the enum value exactly,
    // just show the numeric value.
    // TODO: support flags enums (see metadata.ts@EnumType)
    if (!enumData) return parsed.toLocaleString();

    return enumData.displayName;
  }

  protected visitDateValue(value: any, meta: DateValue): string | null {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    let formatString: string | undefined;
    let formatOptions: Parameters<typeof format>[2] | undefined;
    let isLightFormatString = false;

    if (this.options) {
      let { format: formatOptionInput } = this.options;
      if (formatOptionInput) {
        if (typeof formatOptionInput == "string") {
          formatString = formatOptionInput;
        } else if ("distance" in formatOptionInput) {
          const {
            addSuffix = true, // Default addSuffix to true - most likely, it is desired.
            includeSeconds = false,
          } = formatOptionInput;
          return formatDistanceToNow(parsed, { addSuffix, includeSeconds });
        } else {
          formatString = formatOptionInput.format;
          formatOptions = formatOptionInput;
        }
      }
    }
    if (!formatString) {
      isLightFormatString = true;
      switch (meta.dateKind) {
        case "date":
          formatString = "M/d/yyyy";
          break;
        default:
          formatString = "M/d/yyyy h:mm:ss aa";
          break;
      }
    }

    if (defaultTimeZone && !formatOptions?.timeZone && !meta.noOffset) {
      formatOptions = { ...formatOptions, timeZone: defaultTimeZone };
    }

    if (isLightFormatString && !formatOptions) {
      return lightFormat(parsed, formatString);
    }

    if (formatOptions?.timeZone) {
      // This is honestly so stupid that you have to manually convert the input
      // instead of the format function converting it for you based on the timeZone option
      // that is being passed to it...
      // From the docs:
      //    "To clarify, the format function will never change the underlying date, it must be changed to a zoned time before passing it to format."
      return format(
        utcToZonedTime(parsed, formatOptions.timeZone),
        formatString,
        formatOptions
      );
    }

    return format(parsed, formatString, formatOptions);
  }

  protected visitFileValue(value: any, meta: FileValue): string | null {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    if (parsed instanceof File) {
      return parsed.name;
    }

    if (parsed instanceof Blob) {
      return `${parsed.type}, ${parsed.size} bytes`;
    }

    return null;
  }

  protected visitBinaryValue(value: any, meta: BinaryValue): string | null {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;

    if (typeof parsed == "string") {
      let padding = 0;
      while (parsed[parsed.length - 1 - padding] == "=") padding++;
      const bytes = 3 * (parsed.length / 4) - padding;
      return `${bytes.toLocaleString()} bytes`;
    }

    return `${parsed.byteLength.toLocaleString()} bytes`;
  }

  protected visitPrimitiveValue(
    value: any,
    meta: PrimitiveValue
  ): string | null {
    const parsed = parseValue(value, meta);
    if (parsed == null) return null;
    if (typeof parsed == "number") {
      // Don't locale-string numbers - it puts in thousands separators.
      // This may seem like a neat idea, until you start displaying things
      // like PKs, FKs, or numbers like order numbers, invoice numbers.
      // That is to say, numbers without any useful meaning to their magnitude.
      return parsed.toString();
    }
    return parsed.toLocaleString();
  }

  protected visitUnknownValue(value: unknown, meta: UnknownValue) {
    if (typeof value == "string") return value;
    if (typeof value == "number") return value.toString();
    if (typeof value == "object" && value) return value.toString();
    return null;
  }
}

/** Singleton instance of `GetDisplayVisitor` to be used when no options are provided. */
const displayVisitor = new DisplayVisitor();

/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
export function modelDisplay<T extends Model<TMeta>, TMeta extends ClassType>(
  item: T,
  options?: DisplayOptions
): string | null {
  const modelMeta = item.$metadata;
  if (!modelMeta) {
    throw `Object has no $metadata property`;
  }

  return (options ? new DisplayVisitor(options) : displayVisitor).visitObject(
    item,
    modelMeta
  );
}

/**
 * Given a model instance and a descriptor of a property on the instance,
 * return a string representation of the property suitable for display.
 * @param item An instance of the model that holds the property to be displayed
 * @param prop The property to be displayed - either the name of a property or a property metadata object.
 */
export function propDisplay<T extends Model<TMeta>, TMeta extends ClassType>(
  item: T,
  prop: Property | PropNames<TMeta>,
  options?: DisplayOptions | null
) {
  const propMeta = resolvePropMeta(item.$metadata, prop);

  var value = (item as Indexable<T>)[propMeta.name];
  return (options ? new DisplayVisitor(options) : displayVisitor).visitValue(
    value,
    propMeta
  );
}

/**
 * Given a value and metadata which describes that value,
 * return a string representation of the value suitable for display.
 * @param value Any value which is valid for the metadata provided.
 * @param metadata The metadata which describes the value given.
 */
export function valueDisplay(
  value: any,
  metadata: Value,
  options?: DisplayOptions | null
) {
  return (options ? new DisplayVisitor(options) : displayVisitor).visitValue(
    value,
    metadata
  );
}

// Type import to get vue-router's type augmentations
import type VueRouter from "vue-router";

export function bindToQueryString(
  vue: VueInstance,
  obj: any, // TODO: Maybe only support objects with $metadata? Would eliminate need for `parse`, and could allow for very strong typings.
  key: string,
  queryKey: string = key,
  parse?: (v: string) => any,
  mode: "push" | "replace" = "replace"
) {
  const defaultValue = obj[key];

  // When the value changes, persist it to the query.
  vue.$watch(
    () => obj[key],
    (v: any) => {
      if (!vue.$router || !vue.$route) {
        throw new Error(
          "Could not find $router or $route on the component instance. Is vue-router installed?"
        );
      }
      vue.$router[mode]({
        query: {
          ...vue.$route.query,
          [queryKey]:
            v == null || v === ""
              ? undefined
              : // Use metadata to format the value if the obj has any.
              obj?.$metadata?.params?.[key]
              ? mapToDto(v, obj.$metadata.params[key])?.toString()
              : obj?.$metadata?.props?.[key]
              ? mapToDto(v, obj.$metadata.props[key])?.toString()
              : // TODO: Add $metadata to DataSourceParameters/FilterParameters/ListParameters, and then support that as well.
                // Fallback to .tostring()
                String(v) ?? undefined,
        },
      }).catch((err: any) => {});
    }
  );

  // When the query changes, grab the new value.
  vue.$watch(
    () => vue.$route?.query[queryKey],
    (v: any) => {
      obj[key] =
        // Use the default value if null or undefined
        v == null
          ? defaultValue
          : // Use provided parse function, if provided.
          parse
          ? parse(v)
          : // Use metadata to parse the value if the obj is a DataSource.
          obj?.$metadata?.params?.[key]
          ? mapToModel(v, obj.$metadata.params[key])
          : obj?.$metadata?.props?.[key]
          ? mapToModel(v, obj.$metadata.props[key])
          : // TODO: Add $metadata to DataSourceParameters/FilterParameters/ListParameters, and then support that as well.
            // Fallback to the raw value
            v;
    },
    { immediate: true }
  );
}

export function bindKeyToRouteOnCreate(
  vue: VueInstance,
  model: Model<ModelType>,
  routeParamName: string = "id",
  keepQuery: boolean = false,
  routeName?: VueInstance["$route"]["name"]
) {
  if (!vue.$router || !vue.$route) {
    throw new Error(
      "Could not find $router or $route on the component instance. Is vue-router installed?"
    );
  }

  routeName = routeName ?? vue.$route.name;
  vue.$watch(
    () => (model as any)[model.$metadata.keyProp.name],
    (pk: any, o: any) => {
      if (!routeName) {
        throw Error("Cannot use bindKeyToRouteOnCreate with unnamed routes.");
      }
      if (pk && !o) {
        const { href } = vue.$router!.resolve({
          name: routeName ?? vue.$route!.name,
          query: keepQuery ? vue.$route!.query : {},
          params: {
            ...vue.$route!.params,
            [routeParamName]: pk,
          },
        });
        // Manually replace state with the HTML5 history API
        // so that vue-router doesn't notice the route change
        // and therefore won't trigger any route transitions
        // or router-view component reconstitutions.
        window.history.replaceState(history.state, "", href);
      }
    }
  );
}
