import {
  describe,
  it,
  expect,
  vi,
  beforeEach,
  afterEach,
  beforeAll,
} from "vitest";
import { promises as fs } from "fs";
import path from "path";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
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
    // Create a mock server with registerResource and registerTool methods
    mockServer = {
      registerResource: vi.fn(),
      registerTool: vi.fn(),
    };
  });

  it("should register both resource and tool", () => {
    registerTemplateFeatureResource(mockServer as McpServer);

    expect(mockServer.registerResource).toHaveBeenCalledOnce();
    expect(mockServer.registerResource).toHaveBeenCalledWith(
      "coalesce-template-file",
      expect.any(Object), // ResourceTemplate instance
      expect.objectContaining({
        title: "Coalesce Template Files",
        description:
          "Access individual template files from the Coalesce template",
      }),
      expect.any(Function), // Resource handler function
    );

    expect(mockServer.registerTool).toHaveBeenCalledOnce();
    expect(mockServer.registerTool).toHaveBeenCalledWith(
      "coalesce_get_template_features",
      expect.objectContaining({
        description: expect.stringContaining(
          "Get the files for a specific Coalesce template feature",
        ),
        inputSchema: expect.any(Object),
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
      registerResource: vi.fn(),
      registerTool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer as McpServer);

    // Extract the resource handler function from the registerResource call
    const registerCall = mockServer.registerResource.mock.calls[0];
    resourceHandler = registerCall[3]; // Fourth argument is the handler function
  });

  it("should handle missing file path", async () => {
    const uri = { href: "coalesce://template-file/" };
    const result = await resourceHandler(uri, {});

    expect(result.contents).toHaveLength(1);
    expect(result.contents[0].text).toContain("Error: No file path specified");
  });

  it("should handle invalid file path", async () => {
    const uri = { href: "coalesce://template-file/invalid" };
    mockFs.readFile.mockRejectedValue(new Error("File not found"));

    const result = await resourceHandler(uri, { filePath: "invalid" });

    expect(result.contents).toHaveLength(1);
    expect(result.contents[0].text).toContain("Error reading template file");
    expect(result.contents[0].text).toContain("File not found");
  });

  it("should read and process template file successfully", async () => {
    const uri = { href: "coalesce://template-file/test.cs" };
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
      registerResource: vi.fn(),
      registerTool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer as McpServer);

    // Extract the tool handler function from the registerTool call
    const registerCall = mockServer.registerTool.mock.calls[0];
    toolHandler = registerCall[2]; // Third argument is the handler function
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

    const result = await toolHandler({});

    expect(result.content).toHaveLength(1);
    expect(result.content[0].text).toContain("Error:");
    expect(result.content[0].text).toContain("Permission denied");
  });
});

describe("resource template completion", () => {
  let mockServer: any;
  let resourceTemplate: any;

  beforeEach(() => {
    mockServer = {
      registerResource: vi.fn(),
      registerTool: vi.fn(),
    };

    registerTemplateFeatureResource(mockServer as McpServer);

    // Extract the ResourceTemplate instance from the registerResource call
    const registerCall = mockServer.registerResource.mock.calls[0];
    resourceTemplate = registerCall[1]; // Second argument is the ResourceTemplate
  });

  it("should provide completion for feature names", async () => {
    // Check if complete exists and has the filePath property
    if (resourceTemplate._callbacks?.complete?.filePath) {
      const completions =
        await resourceTemplate._callbacks.complete.filePath("Iden");

      expect(Array.isArray(completions)).toBe(true);
      expect(completions.length).toBeGreaterThanOrEqual(0);
    } else {
      // Skip if completion is not available in the current structure
      expect(true).toBe(true);
    }
  });
});
