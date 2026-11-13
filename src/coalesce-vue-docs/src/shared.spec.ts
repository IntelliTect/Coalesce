import { canonicalDocPath, docDir, docPath, withBaseSidebar } from "./shared";

describe("docPath", () => {
  test.each([
    ["index.md", "/docs"],
    ["guide/index.md", "/docs/guide"],
    ["guide/getting-started.md", "/docs/guide/getting-started"],
  ])("%s", (relativePath, expected) => {
    expect(docPath("/docs/", relativePath)).toBe(expected);
  });
});

describe("docDir", () => {
  test.each([
    ["index.md", "/docs/"],
    ["guide/index.md", "/docs/guide/"],
    ["guide/getting-started.md", "/docs/guide/"],
  ])("%s", (relativePath, expected) => {
    expect(docDir("/docs/", relativePath)).toBe(expected);
  });
});

describe("canonicalDocPath", () => {
  test.each([
    ["/docs", "/docs"],
    ["/docs/", "/docs"],
    ["/docs/index", "/docs"],
    ["/docs/guide/", "/docs/guide"],
    ["/docs/guide/index", "/docs/guide"],
    ["/docs/guide/x", "/docs/guide/x"],
  ])("%s", (path, expected) => {
    expect(canonicalDocPath(path, "/docs/")).toBe(expected);
  });
});

describe("withBaseSidebar", () => {
  test("prefixes root-relative links", () => {
    expect(
      withBaseSidebar(
        [
          {
            text: "Guide",
            items: [
              { text: "Home", link: "/" },
              { text: "Intro", link: "/guide/intro" },
              { text: "Site", link: "https://example.com" },
            ],
          },
        ],
        "/docs/",
      ),
    ).toEqual([
      {
        text: "Guide",
        link: undefined,
        items: [
          { text: "Home", link: "/docs", items: undefined },
          { text: "Intro", link: "/docs/guide/intro", items: undefined },
          { text: "Site", link: "https://example.com", items: undefined },
        ],
      },
    ]);
  });

  test("prefixes multi-sidebar keys", () => {
    expect(
      Object.keys(
        withBaseSidebar(
          { "/guide/": [{ text: "A", link: "/guide/a" }] },
          "/docs/",
        ),
      ),
    ).toEqual(["/docs/guide"]);
  });
});
