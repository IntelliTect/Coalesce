import { promises as fs } from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { minimatch } from "minimatch";
import {
  McpServer,
  ResourceTemplate,
} from "@modelcontextprotocol/sdk/server/mcp.js";

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

      let isExcludedByCondition = false;
      let forceInclude = false;
      for (const [excludePattern, excludeCondition] of excludeConditions) {
        if (minimatch(relativePath, excludePattern)) {
          // This file is governed by an exclude condition
          // Wrap the entire content in the inverse condition
          const invertedCondition = invertCondition(excludeCondition);
          if (file.endsWith(".cs")) {
            content = `#if (${invertedCondition})\n${content}\n#endif`;
            isExcludedByCondition = true;
          } else if (file.endsWith(".ts")) {
            content = `//#if (${invertedCondition})\n${content}\n//#endif`;
            isExcludedByCondition = true;
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
  if (condition.startsWith("!")) {
    return condition.substring(1).trim();
  }

  // If the condition has parentheses around the entire thing, add negation inside
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
  const directiveRegex = /#(?:if|elif|elseif)\s+([^#\n\r]+)/gi;

  let match;
  while ((match = directiveRegex.exec(content)) !== null) {
    const condition = match[1].trim();

    if (condition.includes(feature)) {
      return true;
    }
  }

  return false;
}

function formatFeatureFilesResponse(
  feature: string,
  files: Map<string, string>,
): string {
  let response = `Template feature files for: ${feature}\n\n`;
  response += `Found ${files.size} files:\n\n`;

  const templatePath = getDefaultTemplatePath();

  for (const [filePath, content] of files) {
    // Get relative path and replace Coalesce.Starter.Vue with Coalesce.Template
    const relativePath = path
      .relative(templatePath, filePath)
      .replace(/\\/g, "/")
      .replaceAll("Coalesce.Starter.Vue", "Coalesce.Template");

    response += `--- ${relativePath} ---\n`;
    response += content;
    response += `\n\n--- End ${relativePath} ---\n\n`;
  }

  return response;
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
        description:
          symbolConfig.description ||
          symbolConfig.displayName ||
          "No description available",
        requires: undefined as string[] | undefined,
      };

      // Extract requirements if they exist
      if (symbolConfig.$coalesceRequires) {
        feature.requires = extractRequirements(symbolConfig.$coalesceRequires);
      }

      features.push(feature);
    }
  }

  return features.sort((a, b) => a.name.localeCompare(b.name));
}

function extractRequirements(requires: any): string[] {
  if (Array.isArray(requires)) {
    if (requires[0] === "and" && Array.isArray(requires[1])) {
      return requires.slice(1).flat();
    } else if (requires[0] === "or" && Array.isArray(requires[1])) {
      return requires.slice(1).flat();
    } else {
      return requires.flat();
    }
  }
  return [];
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

// Register the template feature resource with completion
export function registerTemplateFeatureResource(server: McpServer) {
  server.registerResource(
    "template-features",
    new ResourceTemplate("coalesce://template-feature/{feature}", {
      list: undefined,
      complete: {
        feature: async (value) => {
          try {
            // Get available features directly from config
            const templateConfig = await loadTemplateConfig();
            const features = extractFeaturesFromConfig(templateConfig);

            // Return feature names that match the current input
            return features
              .map((f) => f.name)
              .filter((name) =>
                name.toLowerCase().startsWith(value.toLowerCase()),
              );
          } catch {
            return [];
          }
        },
      },
    }),
    {
      title: "Template Features",
      description:
        "Get template feature files for a specific feature, or list all available features",
    },
    async (uri, { feature }) => {
      try {
        const templateConfig = await loadTemplateConfig();

        if (!feature || feature === "") {
          // No feature specified - return list of all features
          const features = extractFeaturesFromConfig(templateConfig);
          const responseText = formatFeaturesListResponse(features);

          return {
            contents: [
              {
                uri: uri.href,
                text: responseText,
                mimeType: "text/plain",
              },
            ],
          };
        } else {
          // Feature specified - return files for that feature
          const featureStr = Array.isArray(feature) ? feature[0] : feature;
          const featureFiles = await getFilesForFeature(
            featureStr,
            templateConfig,
          );
          const responseText = formatFeatureFilesResponse(
            featureStr,
            featureFiles,
          );

          return {
            contents: [
              {
                uri: uri.href,
                text: responseText,
                mimeType: "text/plain",
              },
            ],
          };
        }
      } catch (error) {
        const errorMessage =
          error instanceof Error ? error.message : "Unknown error occurred";
        return {
          contents: [
            {
              uri: uri.href,
              text: `Error: ${errorMessage}`,
              mimeType: "text/plain",
            },
          ],
        };
      }
    },
  );
}
