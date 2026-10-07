import { useRouter } from "vue-router";

/** Languages whose prompt characters are stripped when copying. */
const shellLanguages =
  /^(?:shellscript|shell|bash|sh|zsh|ash|dash|ksh|powershell|ps1)$/i;

const copyIgnoredNodes = [".vp-copy-ignore", ".diff.remove"].join(", ");

export interface DocsContentOptions {
  /** URL directory the current page's relative links resolve against (`meta.docDir`). */
  baseDir: () => string | undefined;
  /** URL of the current page (`meta.docPath`), which in-page `#` links resolve against. */
  path?: () => string | undefined;
  /**
   * Handles a click on a same-origin link, including in-page `#` links.
   * Defaults to `router.push`, leaving in-page links to the browser.
   */
  navigate?: (url: URL) => void;
}

/**
 * Behavior for the markup VitePress's markdown plugins emit: copy buttons,
 * code-group tabs, and routing relative links through vue-router.
 * Returns a delegated click handler to put on the element containing the page.
 */
export function useDocsContent(options: DocsContentOptions) {
  const router = useRouter();
  const copyResetTimers = new WeakMap<Element, number>();

  function onClick(event: MouseEvent) {
    const target = event.target as HTMLElement | null;
    if (!target) return;

    if (handleCopyButton(target)) return;
    if (handleCodeGroupTab(target)) return;
    handleLink(event, target);
  }

  function handleCopyButton(target: HTMLElement) {
    if (!target.matches('div[class*="language-"] > button.copy')) return false;

    const block = target.parentElement!;
    const pre = block.querySelector("pre");
    if (!pre) return true;

    const clone = pre.cloneNode(true) as HTMLElement;
    clone.querySelectorAll(copyIgnoredNodes).forEach((node) => node.remove());
    // Removing nodes leaves runs of blank lines behind. Real newlines in the
    // code are separate `.line` elements, so they survive this.
    clone.innerHTML = clone.innerHTML.replace(/\n+/g, "\n");

    let text = clone.textContent ?? "";
    const lang = /language-([\w-]+)/.exec(block.className)?.[1] ?? "";
    if (shellLanguages.test(lang)) {
      text = text.replace(/^ *(\$|>) /gm, "").trim();
    }

    void navigator.clipboard.writeText(text).then(() => {
      target.classList.add("copied");
      clearTimeout(copyResetTimers.get(target));
      copyResetTimers.set(
        target,
        window.setTimeout(() => {
          target.classList.remove("copied");
          target.blur();
          copyResetTimers.delete(target);
        }, 2000),
      );
    });

    return true;
  }

  function handleCodeGroupTab(target: HTMLElement) {
    if (!target.matches(".vp-code-group input")) return false;

    // input <- .tabs <- .vp-code-group
    const group = target.parentElement?.parentElement;
    const blocks = group?.querySelector(".blocks");
    if (!group || !blocks) return true;

    const index = Array.from(group.querySelectorAll("input")).indexOf(
      target as HTMLInputElement,
    );
    const next = blocks.children[index];
    if (index < 0 || !next) return true;

    Array.from(blocks.children).forEach((child) =>
      child.classList.remove("active"),
    );
    next.classList.add("active");

    return true;
  }

  function handleLink(event: MouseEvent, target: HTMLElement) {
    if (
      event.defaultPrevented ||
      event.button !== 0 ||
      event.metaKey ||
      event.ctrlKey ||
      event.shiftKey ||
      event.altKey
    ) {
      return;
    }

    const anchor = target.closest("a");
    if (
      !anchor ||
      anchor.target === "_blank" ||
      anchor.hasAttribute("download")
    ) {
      return;
    }

    const href = anchor.getAttribute("href");
    if (!href) return;
    if (href.startsWith("#") && !options.navigate) return;
    // mailto:, tel:, https: — anything the app has no route for.
    if (/^[a-z][a-z0-9+.-]*:/i.test(href)) return;

    // The raw attribute, not `anchor.href`: the browser resolved that one
    // against the current URL, which is not where the page's markdown sits.
    const relativeTo = href.startsWith("#")
      ? (options.path?.() ?? location.pathname)
      : (options.baseDir() ?? "/");
    const url = new URL(href, location.origin + relativeTo);
    if (url.origin !== location.origin) return;

    event.preventDefault();
    if (options.navigate) options.navigate(url);
    else void router.push(url.pathname + url.search + url.hash);
  }

  return { onClick };
}
