
export const MetadataSymbol = Symbol("metadata");

export const shortStringify = (value: any, maxLen = 55) => {
  let str = JSON.stringify(value, (key, value) => {
    if (key === "$metadata") return MetadataSymbol;
    return value;
  })
  if (str.length > maxLen) {
    str = str.substr(0, maxLen) + "..."
  }
  return str;
};