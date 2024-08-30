import { watch } from "vue";

/** The segment to append to all page titles.
 * Usually the name of your application or company.
 * Defaults to the value at page load, which usually comes from index.html */
const titlePostfix = document.title || "";

/** The separator used between title segments. */
const titleSeparator = " - ";

/** Use on page/router components to set the document title.
 * @param title A constant string or array of strings (for a static title), or a function that returns a string or array of strings (for a dynamic title). Arrays will be concatenated with `titleSeparator`.
 */
export function useTitle(
  title:
    | string
    | (string | null | undefined)[]
    | (() => string | null | undefined | (string | null | undefined)[]),
) {
  onBeforeMount(() => {
    if (!title) title = "";

    if (typeof title == "string" || Array.isArray(title)) {
      const nonFnTitle = title;
      title = () => nonFnTitle;
    }

    watch(
      title,
      (newTitle) => {
        document.title = [
          ...(Array.isArray(newTitle) ? newTitle : [newTitle]),
          titlePostfix,
        ]
          .filter((t) => t)
          .join(titleSeparator);
      },
      { immediate: true, deep: true },
    );
  });
}
