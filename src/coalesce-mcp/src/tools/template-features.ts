import { promises as fs } from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { minimatch } from "minimatch";
import {
  McpServer,
  ResourceTemplate,
} from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function getDefaultTemplatePath(): string {
  // This will point to the bundled template files copied during build
  return path.join(__dirname, "..", "template");
}

async function loadTemplateConfig(): Promise<any> {
  const templatePath = getDefaultTemplatePath();
  const configPath = path.join(
    templatePath,
    ".template.config",
    "template.json",
  );

  try {
    const configContent = await fs.readFile(configPath, "utf8");
    return JSON.parse(configContent);
  } catch (error) {
    throw new Error(
      `Failed to load template configuration from ${configPath}: ${error instanceof Error ? error.message : "Unknown error"}`,
    );
  }
}

async function getFilesForFeature(
  feature: string,
  templateConfig: any,
): Promise<Map<string, string>> {
  const featureFiles = new Map<string, string>();

  // Find the correct casing of the feature from template.json symbols
  const symbols = templateConfig.symbols || {};
  const correctFeatureName = Object.keys(symbols).find(
    (name) => name.toLowerCase() === feature.toLowerCase(),
  );

  if (!correctFeatureName) {
    throw new Error(`Feature '${feature}' not found in template configuration`);
  }

  // Use the correctly cased feature name for the rest of the processing
  feature = correctFeatureName;

  // Build a map of file patterns to their exclude conditions from template.json
  const excludeConditions = new Map<string, string>();
  const sources = templateConfig.sources || [];

  for (const source of sources) {
    const modifiers = source.modifiers || [];
    for (const modifier of modifiers) {
      if (modifier.condition && modifier.exclude) {
        for (const excludePattern of modifier.exclude) {
          excludeConditions.set(excludePattern, modifier.condition);
        }
      }
    }
  }

  // Read all files in the template
  const allFiles = await findTemplateFiles();

  for (const file of allFiles) {
    try {
      let content = await fs.readFile(file, "utf8");

      // Trim BOM if present
      if (content.charCodeAt(0) === 0xfeff) {
        content = content.slice(1);
      }

      // This might be slightly easier for LLMs to understand:
      content = content.replaceAll("Coalesce.Starter.Vue", "Coalesce.Template");

      // Check if this file matches any exclude pattern
      const templatePath = getDefaultTemplatePath();
      const relativePath = path
        .relative(templatePath, file)
        .replace(/\\/g, "/");

      let forceInclude = false;
      for (const [excludePattern, excludeCondition] of excludeConditions) {
        if (minimatch(relativePath, excludePattern)) {
          // This file is governed by an exclude condition
          // Wrap the entire content in the inverse condition
          const invertedCondition = invertCondition(excludeCondition);
          if (file.endsWith(".cs")) {
            content = `#if (${invertedCondition})\n${content}\n#endif`;
          } else if (file.endsWith(".ts")) {
            content = `//#if (${invertedCondition})\n${content}\n//#endif`;
          } else if (excludeCondition.includes(feature)) {
            // We can't put a directive in this type of file.
            // Just force include it because its probably relevant to the feature
            // if its exclude condition mentions the feature.
            forceInclude = true;
          }
          break; // Only apply the first matching exclude pattern
        }
      }

      // Now check if the content (potentially modified) contains feature directives for our feature
      if (forceInclude || containsFeatureDirectives(content, feature)) {
        featureFiles.set(file, content);
      }
    } catch (error) {
      // Skip files that can't be read (binary files, etc.)
      continue;
    }
  }

  return featureFiles;
}

function invertCondition(condition: string): string {
  // Clean up the condition string
  condition = condition.trim();

  // If the condition is already negated, remove the negation
  if (condition.startsWith("!") && !condition.includes("(")) {
    return condition.substring(1).trim();
  }

  // If the condition is negated with parentheses around the entire thing just remove the negation
  if (
    condition.startsWith("!(") &&
    condition.endsWith(")") &&
    condition.lastIndexOf("(") == 1
  ) {
    return condition.substring(1);
  }

  // If the condition has parentheses around the entire thing, add negation outside
  if (condition.startsWith("(") && condition.endsWith(")")) {
    return `!${condition}`;
  }

  // For other conditions, wrap in negation with parentheses
  return `!(${condition})`;
}

async function findTemplateFiles(pattern?: string): Promise<string[]> {
  const files: string[] = [];

  async function walkDir(
    dir: string,
    relativePath: string = "",
  ): Promise<void> {
    const entries = await fs.readdir(dir, { withFileTypes: true });

    for (const entry of entries) {
      const fullPath = path.join(dir, entry.name);
      const relativeFilePath = path
        .join(relativePath, entry.name)
        .replace(/\\/g, "/");

      if (entry.isDirectory()) {
        await walkDir(fullPath, relativeFilePath);
      } else {
        if (!pattern || minimatch(relativeFilePath, pattern)) {
          files.push(fullPath);
        }
      }
    }
  }

  await walkDir(getDefaultTemplatePath());
  return files;
}

function containsFeatureDirectives(content: string, feature: string): boolean {
  // Regex to match template preprocessor directives
  // Matches #if, #elif, #elseif followed by conditions
  const directiveRegex = /#(?:if|elif|elseif)[^\n\r]*/gim;

  let match;
  while ((match = directiveRegex.exec(content)) !== null) {
    const condition = match[0].trim();

    if (condition.includes(feature)) {
      return true;
    }
  }

  return false;
}

function extractFeaturesFromConfig(
  templateConfig: any,
): Array<{ name: string; description: string; requires?: string[] }> {
  const features: Array<{
    name: string;
    description: string;
    requires?: string[];
  }> = [];
  const symbols = templateConfig.symbols || {};

  for (const [name, config] of Object.entries(symbols)) {
    const symbolConfig = config as any;

    // Skip generated symbols (like ports)
    if (symbolConfig.type === "generated") {
      continue;
    }

    // Only include parameter symbols that are boolean (feature toggles)
    if (symbolConfig.type === "parameter" && symbolConfig.datatype === "bool") {
      const feature = {
        name,
        description: symbolConfig.description || symbolConfig.displayName,
      };

      features.push(feature);
    }
  }

  return features.sort((a, b) => a.name.localeCompare(b.name));
}

function formatFeaturesListResponse(
  features: Array<{ name: string; description: string; requires?: string[] }>,
): string {
  let response = `Available Coalesce Template Features (${features.length} total):\n\n`;

  for (const feature of features) {
    response += `• **${feature.name}**\n`;
    response += `  ${feature.description}\n`;
    response += "\n";
  }

  return response;
}

function getResourceTitle(filePath: string): string {
  const templatePath = getDefaultTemplatePath();
  return path
    .relative(templatePath, filePath)
    .replace(/\\/g, "/")
    .replaceAll("Coalesce.Starter.Vue", "Coalesce.Template");
}

function getResourceSlug(filePath: string): string {
  return getResourceTitle(filePath).replaceAll("/", "__"); // since we're building a URI, we can't have slashes within the segment of the URI.;
}

function createTemplateFileResourceLink(filePath: string) {
  const relativePath = getResourceSlug(filePath);

  return {
    type: "resource_link" as const,
    uri: `coalesce://template-file/${encodeURIComponent(relativePath)}`,
    name: relativePath,
    description: `Template file: ${relativePath}`,
  };
}

// Register the template feature resource with completion
export function registerTemplateFeatureResource(server: McpServer) {
  server.registerResource(
    "coalesce-template-file",
    new ResourceTemplate("coalesce://template-file/{filePath}", {
      list: undefined,
      complete: {
        filePath: async (value) => {
          const allFiles = await findTemplateFiles();

          return allFiles
            .map(getResourceSlug)
            .filter((path) => !path.includes(".template.config"))
            .filter((path) => path.toLowerCase().includes(value.toLowerCase()))
            .sort();
        },
      },
    }),
    {
      title: "Coalesce Template Files",
      description:
        "Access individual template files from the Coalesce template",
    },
    async (uri, { filePath }) => {
      console.error("path:" + filePath);
      try {
        if (!filePath) {
          return {
            contents: [
              {
                uri: uri.href,
                text: "Error: No file path specified",
                mimeType: "text/plain",
              },
            ],
          };
        }

        const templatePath = getDefaultTemplatePath();
        const decodedPath = decodeURIComponent(
          Array.isArray(filePath) ? filePath[0] : filePath,
        );

        // Convert template path back to actual path
        const actualPath = decodedPath
          .replaceAll("Coalesce.Template", "Coalesce.Starter.Vue")
          .replaceAll("__", "/");
        const fullPath = path.join(templatePath, actualPath);

        // Security check - ensure we're still within the template directory
        const normalizedTemplatePath = path.resolve(templatePath);
        const normalizedFullPath = path.resolve(fullPath);
        if (!normalizedFullPath.startsWith(normalizedTemplatePath)) {
          throw new Error("Invalid file path - outside template directory");
        }

        let content = await fs.readFile(fullPath, "utf8");

        // Trim BOM if present
        if (content.charCodeAt(0) === 0xfeff) {
          content = content.slice(1);
        }

        // Replace namespace name for perhaps better LLM interpretation
        content = content.replaceAll(
          "Coalesce.Starter.Vue",
          "Coalesce.Template",
        );

        return {
          contents: [
            {
              uri: uri.href,
              text: content,
            },
          ],
        };
      } catch (error) {
        console.error("err path:" + filePath);
        const errorMessage =
          error instanceof Error ? error.message : "Unknown error occurred";
        return {
          contents: [
            {
              uri: uri.href,
              text: `Error reading template file: ${errorMessage}`,
              mimeType: "text/plain",
            },
          ],
        };
      }
    },
  );

  // Register tool function to expose the resource functionality directly.
  //
  server.registerTool(
    "get_coalesce_template_features",
    {
      description:
        "Get the files for a specific Coalesce template feature, or list all available features if no feature is specified",
      inputSchema: {
        feature: z
          .string()
          .optional()
          .describe(
            "The name of the template feature to get files for. If not provided, lists all available features.",
          ),
      },
    },
    async ({ feature }) => {
      try {
        const templateConfig = await loadTemplateConfig();

        if (!feature) {
          // No feature specified - return list of all features
          const features = extractFeaturesFromConfig(templateConfig);
          return {
            content: [
              {
                type: "text" as const,
                text: formatFeaturesListResponse(features),
              },
            ],
          };
        } else {
          // Feature specified - return resource links for that feature's files
          const featureFiles = await getFilesForFeature(
            feature,
            templateConfig,
          );

          // Responding with these as resource links works really poorly.
          // Sometimes the agent will just not even bother to read the resources.
          const resourceLinks = [...featureFiles.keys()].map((filePath) =>
            createTemplateFileResourceLink(filePath),
          );

          const textContents = [...featureFiles.entries()].map(
            ([filePath, content]) => ({
              type: "text" as const,
              text: getResourceTitle(filePath) + "\n\n" + content,
            }),
          );

          return {
            content: [
              {
                type: "text" as const,
                text: `The following is a list of files from the Coalesce project template that are included by or affected by the ${feature} feature. If you have been asked to implement the feature, carefully examine the existing code base to see if each part of the feature might already exist in some form. Some of them may include conditional syntax like \`#if\` that should be interpreted while applying changes, but not copied directly to the output. It is vitally important that you do not make mistakes or functional alterations to the code when copying it over. Do not run Coalesce code generation until you're done.`,
                annotations: {
                  audience: ["assistant"],
                },
              },
              // ...resourceLinks,
              ...textContents,
            ],
          };
        }
      } catch (error) {
        const errorMessage =
          error instanceof Error ? error.message : "Unknown error occurred";
        return {
          content: [
            {
              type: "text" as const,
              text: `Error: ${errorMessage}`,
            },
          ],
        };
      }
    },
  );
}
