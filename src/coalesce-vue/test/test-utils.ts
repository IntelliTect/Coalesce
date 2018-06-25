
export const MetadataSymbol = Symbol("metadata");

export const shortStringify = (value: any, maxLen = 55) => {
  
  // InvalidDate stringifies to null - we want to preserve the invalidity in the display.
  if (value instanceof Date) return `Date(${value.toString()})`;
  if (value === undefined) return "undefined";

  let str = JSON.stringify(value, (key, value) => {
    if (key === "$metadata") return MetadataSymbol;
    return value;
  })
  if (str.length > maxLen) {
    str = str.substr(0, maxLen) + "..."
  }
  return str;
};