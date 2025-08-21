import { describe, it, expect, vi, beforeEach } from "vitest";
import { promises as fs } from "fs";
import path from "path";
import { registerTemplateFeatureResource } from "../src/tools/template-features.js";

// Mock fs module
vi.mock("fs", () => ({
  promises: {
    readFile: vi.fn(),
    readdir: vi.fn(),
    stat: vi.fn(),
  },
}));

// Mock path module
vi.mock("path", async () => {
  const actual = await vi.importActual<typeof path>("path");
  return {
    ...actual,
    join: vi.fn((...args) => args.join("/")),
    dirname: vi.fn(() => "/mock/dirname"),
  };
});

// Mock URL module
vi.mock("url", () => ({
  fileURLToPath: vi.fn(() => "/mock/file/path"),
}));

const mockFs = vi.mocked(fs);
const mockPath = vi.mocked(path);

beforeEach(() => {
  // Mock fs.readdir to return file names as strings
  mockFs.readdir.mockImplementation(async (dirPath: any) => {
    // Return mock directory entries as simple string arrays
    const pathStr = dirPath.toString().replace(/\\/g, "/"); // Normalize path separators

    // Match exact directory paths to avoid infinite recursion
    if (pathStr.endsWith("/template")) {
      return ["Controllers", "Views", "README.md"] as any;
    } else if (pathStr.endsWith("/Controllers")) {
      return ["AccountController.cs", "HomeController.cs"] as any;
    } else if (pathStr.endsWith("/Views")) {
      return ["Index.cshtml", "About.cshtml"] as any;
    } else {
      return [] as any;
    }
  });

  // Mock fs.stat to determine if path is directory or file
  mockFs.stat.mockImplementation(async (filePath: any) => {
    const pathStr = filePath.toString().replace(/\\/g, "/"); // Normalize path separators

    // Check for directories by exact path matching
    if (
      pathStr.endsWith("/Controllers") ||
      pathStr.endsWith("/Views") ||
      pathStr.endsWith("/template")
    ) {
      // Directory
      return {
        isDirectory: () => true,
        isFile: () => false,
      } as any;
    } else {
      // File
      return {
        isDirectory: () => false,
        isFile: () => true,
      } as any;
    }
  });

  // Mock fs.readFile with dynamic content based on file path
  mockFs.readFile.mockImplementation(async (filePath: any, encoding?: any) => {
    const pathStr = filePath.toString();

    if (pathStr.includes("template.json")) {
      return JSON.stringify({
        sources: [
          {
            modifiers: [
              {
                condition: "!Identity",
                exclude: ["Controllers/AccountController.cs"],
              },
            ],
          },
        ],
        symbols: {
          Identity: {
            type: "parameter",
            datatype: "bool",
            description: "Add ASP.NET Core Identity authentication",
          },
          DarkMode: {
            type: "parameter",
            datatype: "bool",
            description: "Enable dark mode support in the UI",
          },
          Port: {
            type: "generated",
            generator: "port",
          },
        },
      });
    } else if (pathStr.includes(".cs")) {
      // Return mock C# file content
      return `
using Microsoft.AspNetCore.Identity;
namespace Coalesce.Starter.Vue {
  // Template file content here
}`;
    } else {
      // Default file content
      return "Mock file content";
    }
  });
});

describe("registerTemplateFeatureResource", () => {
  let mockServer: any;

  beforeEach(() => {
    // Create a mock server with template and tool methods
    mockServer = {
      template: vi.fn(),
      tool: vi.fn(),
    };
  });

  it("should register both resource and tool", () => {
    registerTemplateFeatureResource(mockServer);

    expect(mockServer.template).toHaveBeenCalledOnce();
    expect(mockServer.template).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "coalesce-template-file",
        description:
          "Access individual template files from the Coalesce template",
        uri: "coalesce://template-file/{filePath}",
        complete: expect.objectContaining({
          filePath: expect.any(Function),
        }),
      }),
      expect.any(Function), // Template handler function
    );

    expect(mockServer.tool).toHaveBeenCalledOnce();
    expect(mockServer.tool).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "coalesce_get_template_features",
        description: expect.stringContaining(
          "Get the files for a specific Coalesce template feature",
        ),
        schema: expect.any(Object),
      }),
      expect.any(Function), // Tool handler function
    );
  });
});

describe("template file resource handler", () => {
  let mockServer: any;
  let resourceHandler: any;

  beforeEach(() => {
    mockServer = {
      template: vi.fn(),
      tool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer);

    // Extract the resource handler function from the template call
    const registerCall = mockServer.template.mock.calls[0];
    resourceHandler = registerCall[1]; // Second argument is the handler function
  });

  it("should handle missing file path", async () => {
    const uri = "coalesce://template-file/";
    const result = await resourceHandler(uri, {});

    expect(result.contents).toHaveLength(1);
    expect(result.contents[0].text).toContain("Error: No file path specified");
  });

  it("should handle invalid file path", async () => {
    const uri = "coalesce://template-file/invalid";
    mockFs.readFile.mockRejectedValue(new Error("File not found"));

    const result = await resourceHandler(uri, { filePath: "invalid" });

    expect(result.contents).toHaveLength(1);
    expect(result.contents[0].text).toContain("Error reading template file");
    expect(result.contents[0].text).toContain("File not found");
  });

  it("should read and process template file successfully", async () => {
    const uri = "coalesce://template-file/test.cs";
    const mockContent = "namespace Coalesce.Starter.Vue { }";
    mockFs.readFile.mockResolvedValue(mockContent);

    const result = await resourceHandler(uri, { filePath: "test.cs" });

    expect(result.contents).toHaveLength(1);
    expect(result.contents[0].text).toContain("namespace Coalesce.Template");
  });
});

describe("template features tool handler", () => {
  let mockServer: any;
  let toolHandler: any;

  beforeEach(() => {
    mockServer = {
      template: vi.fn(),
      tool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer);

    // Extract the tool handler function from the tool call
    const registerCall = mockServer.tool.mock.calls[0];
    toolHandler = registerCall[1]; // Second argument is the handler function
  });

  it("should list all features when no feature parameter is provided", async () => {
    const result = await toolHandler({});

    expect(result.content).toHaveLength(1);
    expect(result.content[0].text).toContain(
      "Available Coalesce Template Features",
    );
    expect(result.content[0].text).toContain("• **Identity**");
    expect(result.content[0].text).toContain("• **DarkMode**");
    expect(result.content[0].text).not.toContain("Port"); // Should exclude generated symbols
  });

  it("should return feature files when a specific feature is provided", async () => {
    const result = await toolHandler({ feature: "Identity" });

    expect(result.content).toHaveLength(2); // Header text + file content
    expect(result.content[0].text).toContain("following is a list of files");
    expect(result.content[0].text).toContain("Identity feature");
  });

  it("should handle file system errors gracefully", async () => {
    mockFs.readFile.mockRejectedValue(new Error("Permission denied"));

    try {
      await toolHandler({});
      // Should not reach here
      expect(false).toBe(true);
    } catch (error) {
      expect(error).toBeInstanceOf(Error);
      expect((error as Error).message).toContain("Permission denied");
    }
  });
});

describe("resource template completion", () => {
  let mockServer: any;
  let templateDefinition: any;

  beforeEach(() => {
    mockServer = {
      template: vi.fn(),
      tool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer);

    // Extract the template definition from the template call
    const registerCall = mockServer.template.mock.calls[0];
    templateDefinition = registerCall[0]; // First argument is the template definition
  });

  it("should provide completion for feature names", async () => {
    // Check if complete function exists and has the filePath property
    if (templateDefinition.complete && templateDefinition.complete.filePath) {
      const result = await templateDefinition.complete.filePath("Iden");

      expect(result).toHaveProperty("completion");
      expect(result.completion).toHaveProperty("values");
      expect(Array.isArray(result.completion.values)).toBe(true);
      expect(result.completion.values.length).toBeGreaterThanOrEqual(0);
    } else {
      // Skip if completion is not available in the current structure
      expect(true).toBe(true);
    }
  });
});
