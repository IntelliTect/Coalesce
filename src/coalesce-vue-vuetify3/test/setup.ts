// Suppress jsdom's "Not implemented" warnings for getComputedStyle with pseudo-elements.
// These are triggered by Vuetify internals and are not actionable.
// jsdom emits these via the virtual console which writes to stderr directly.
const originalStderrWrite = process.stderr.write.bind(process.stderr);
process.stderr.write = ((chunk: any, encodingOrCb?: any, cb?: any): boolean => {
  const str = typeof chunk === "string" ? chunk : chunk.toString();
  if (
    str.includes(
      "Not implemented: Window's getComputedStyle() method: with pseudo-elements",
    )
  )
    return true;
  return originalStderrWrite(chunk, encodingOrCb, cb);
}) as typeof process.stderr.write;
