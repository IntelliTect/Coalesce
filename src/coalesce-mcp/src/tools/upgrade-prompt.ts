import { promises as fs } from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { McpServer } from "tmcp";
import { z } from "zod";

type McpServerType = McpServer<any>;

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function getTemplatePath(): string {
  return path.join(__dirname, "..", "template");
}

function getChangelogPath(): string {
  return path.join(__dirname, "..", "CHANGELOG.md");
}

async function readFileContent(fullPath: string): Promise<string> {
  let content = await fs.readFile(fullPath, "utf8");
  if (content.charCodeAt(0) === 0xfeff) {
    content = content.slice(1);
  }
  return content;
}

async function findTemplateFiles(
  dir: string,
  relativePath: string = "",
): Promise<string[]> {
  const files: string[] = [];
  const entries = await fs.readdir(dir);
  for (const entryName of entries) {
    const fullPath = path.join(dir, entryName);
    const relPath = path.join(relativePath, entryName).replace(/\\/g, "/");
    const stat = await fs.stat(fullPath);
    if (stat.isDirectory()) {
      if (
        entryName === ".template.config" ||
        entryName === "bin" ||
        entryName === "obj" ||
        entryName === "node_modules" ||
        entryName === "wwwroot"
      ) {
        continue;
      }
      files.push(...(await findTemplateFiles(fullPath, relPath)));
    } else {
      files.push(relPath);
    }
  }
  return files;
}

export function registerUpgradeTools(server: McpServerType) {
  // Tool: Read a template file by path
  server.tool(
    {
      name: "read_template_file",
      title: "Read Coalesce template file",
      description:
        "Read the contents of a file from the Coalesce project template. " +
        "Call with no arguments to list all available template files. ",
      schema: z.object({
        filePath: z
          .string()
          .optional()
          .describe(
            "Path or partial filename to match within the template " +
              "(e.g. 'package.json', 'Web/vite.config.ts', 'Data.csproj'). " +
              "Case-insensitive. If multiple files match, all matches are listed. " +
              "Omit to list all available files.",
          ),
      }),
    },
    async ({ filePath }) => {
      const templatePath = getTemplatePath();
      const allFiles = await findTemplateFiles(templatePath);

      if (!filePath) {
        return {
          content: [
            {
              type: "text" as const,
              text:
                "Available template files:\n\n" +
                allFiles.map((f) => `- ${f}`).join("\n"),
            },
          ],
        };
      }

      const query = filePath.replace(/\\/g, "/").toLowerCase();

      // Try matching strategies in order of specificity:
      // 1. Exact match
      // 2. Ends-with match (partial path from the right, e.g. "Web/package.json")
      // 3. Filename-only match (e.g. "package.json")
      // 4. Substring match (e.g. "vite.config")
      let matches = allFiles.filter((f) => f.toLowerCase() === query);
      if (matches.length === 0) {
        matches = allFiles.filter(
          (f) =>
            f.toLowerCase().endsWith("/" + query) ||
            f.toLowerCase().endsWith(query),
        );
      }
      if (matches.length === 0) {
        const queryFilename = query.split("/").pop()!;
        matches = allFiles.filter(
          (f) => f.toLowerCase().split("/").pop() === queryFilename,
        );
      }
      if (matches.length === 0) {
        matches = allFiles.filter((f) => f.toLowerCase().includes(query));
      }

      if (matches.length === 0) {
        return {
          content: [
            {
              type: "text" as const,
              text: `No template files matched '${filePath}'.`,
            },
          ],
          isError: true,
        };
      }

      if (matches.length === 1) {
        const fullPath = path.resolve(templatePath, matches[0]);
        if (!fullPath.startsWith(path.resolve(templatePath))) {
          return {
            content: [
              {
                type: "text" as const,
                text: "Error: Path is outside the template directory.",
              },
            ],
            isError: true,
          };
        }
        let content = await readFileContent(fullPath);
        return {
          content: [
            {
              type: "text" as const,
              text: `File: ${matches[0]}\n\n${content}`,
            },
          ],
        };
      }

      // Multiple matches — return the list so the caller can refine
      return {
        content: [
          {
            type: "text" as const,
            text:
              `Multiple template files matched '${filePath}':\n\n` +
              matches.map((f) => `- ${f}`).join("\n"),
          },
        ],
      };
    },
  );

  // Tool: Read the Coalesce changelog
  server.tool(
    {
      name: "read_changelog",
      title: "Read Coalesce changelog",
      description:
        "Read the Coalesce changelog. Optionally filter to only show entries " +
        "for versions newer than a specified version.",
      schema: z.object({
        sinceVersion: z
          .string()
          .optional()
          .describe(
            "If provided, only return changelog entries for versions newer than this version " +
              "(e.g. '6.0.0'). The entry for the specified version itself is excluded.",
          ),
      }),
    },
    async ({ sinceVersion }) => {
      let changelog: string;
      try {
        changelog = await readFileContent(getChangelogPath());
      } catch {
        return {
          content: [
            {
              type: "text" as const,
              text: "Error: Changelog not available.",
            },
          ],
          isError: true,
        };
      }

      if (sinceVersion) {
        // Strip prerelease suffix (e.g. "6.4.0-beta.20260415.3" -> "6.4.0")
        // so we match the release heading in the changelog.
        const baseVersion = sinceVersion.replace(/-.*$/, "");
        const versionHeadingRegex = new RegExp(
          `^# ${baseVersion.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}\\b`,
          "m",
        );
        const match = versionHeadingRegex.exec(changelog);
        if (match) {
          // Include the current version's entries (find the next heading after it)
          const nextHeading = changelog.indexOf("\n# ", match.index + 1);
          changelog =
            nextHeading >= 0
              ? changelog.slice(0, nextHeading).trim()
              : changelog.trim();
        }
        if (!changelog) {
          return {
            content: [
              {
                type: "text" as const,
                text: `No changelog entries found newer than version ${sinceVersion}. The project may already be on the latest version.`,
              },
            ],
          };
        }
      }

      return {
        content: [
          {
            type: "text" as const,
            text: changelog,
          },
        ],
      };
    },
  );

  // Prompt: Upgrade guide
  server.prompt(
    {
      name: "upgrade",
      title: "Upgrade Coalesce",
      description:
        "Guide to upgrade a Coalesce project's dependencies and features",
    },
    async () => {
      const instructions = `You are helping the user upgrade their Coalesce project to the latest version.

Use the \`read_template_file\` and \`read_changelog\` tools to read the Coalesce template files and changelog as needed throughout this process.

## Step 1: Determine Current Version

Find and read the user's \`Directory.Build.props\` to get their current \`CoalesceVersion\`. Also read their web project's \`package.json\` to find the current versions of \`coalesce-vue\`, \`coalesce-vue-vuetify3\`, and \`eslint-plugin-coalesce\`.

If the NPM and NuGet versions don't match each other, warn the user immediately — mismatched versions cause bugs.

Then use \`read_template_file\` to read \`Directory.Build.props\` and \`package.json\` from the template to determine the latest Coalesce version.

If the user is already on the latest version, tell them and stop. Let them know that if this is unexpected, their coalesce-mcp may be outdated.

If upgrading would cross a major version boundary (e.g. 5.x → 6.x), warn the user that major upgrades may require significant changes and suggest upgrading one major version at a time.

If the upgrade includes a Vuetify major version change (e.g. Vuetify 3 to 4), warn the user that the Vuetify upgrade is non-trivial for most apps. Suggest they follow the official Vuetify upgrade guide at https://vuetifyjs.com/en/getting-started/upgrade-guide/ alongside the Coalesce upgrade.

## Step 2: Review the Changelog

Use the \`read_changelog\` tool with the user's current version as \`sinceVersion\` to get all changes since their version. Pay special attention to:
- **Breaking Changes** sections — these require code modifications
- **Template Changes** sections — these indicate configuration/pattern updates
- New features that may require opt-in

Summarize the relevant changes for the user before making any modifications.

## Step 3: Update Coalesce Package Versions

1. Update \`CoalesceVersion\` in \`Directory.Build.props\` to match the template's version.
2. Update \`coalesce-vue\`, \`coalesce-vue-vuetify3\`, and \`eslint-plugin-coalesce\` in the web project's \`package.json\` to match the template's versions.

## Step 4: Compare Third-Party Dependencies

### NPM
Use \`read_template_file\` to read the template's \`package.json\`, then compare it against the user's \`package.json\`. For each dependency that the user already has:
- If the template has a newer version, suggest upgrading it.
- If the template has removed it, mention that (but don't remove it without asking).
- If the template has added a new package that the user doesn't have, only mention it if it seems universally useful (not gated behind a feature flag).

Do NOT add packages the user doesn't already have if they're only relevant to specific template features (e.g. \`@microsoft/applicationinsights-web\` is only for AppInsights).

### NuGet
Use \`read_template_file\` to read the template's \`.csproj\` files (\`Web.csproj\`, \`Data.csproj\`) and compare NuGet package versions against the user's \`.csproj\` files:
- Only suggest upgrades for packages the user already references.
- Note target framework changes.
- Packages using \`$(CoalesceVersion)\` will be updated automatically from Step 3.

The template \`.csproj\` files contain \`#if\` directives for optional features — only consider packages the user actually has.

## Step 5: Compare Configuration Files

Use \`read_template_file\` to read the template versions of these files, then compare against the user's versions:

- **vite.config.ts** — New plugins, build config changes, code splitting strategy
- **eslint.config.mjs** — New rules, plugin changes. If the user is still on \`.eslintrc\`, suggest migrating to flat config.
- **tsconfig.json**, **tsconfig.node.json**
- **.npmrc**
- **.editorconfig**
- **.vscode/settings.json**
- **.vscode/mcp.json**
- **AGENTS.md**
- **dependabot.yml**

For each file, only suggest changes that are meaningful. Don't rewrite the user's file to match the template exactly — they may have intentional customizations.

## Step 6: Address Breaking Changes

Based on the changelog review from Step 2, make any code changes needed to address breaking changes.

## Step 7: Post-Upgrade Validation

Run these commands in order, fixing any errors before proceeding to the next:

1. \`npm install\` in the web project. If it fails due to a recently-published package being blocked by npm's minimum release age policy, offer to re-run with \`npm install --min-release-age=0\` after explaining the risk. Always check for warnings in the npm install output.
2. \`dotnet restore\` in the solution root
3. Run Coalesce code generation (use the \`coalesce_generate\` tool, or run \`dotnet coalesce\` in the web project)
4. \`dotnet build\` to verify .NET compilation
5. \`npm run build\` in the web project (runs \`vite build && vue-tsc --noEmit\`)
6. If tests exist, run \`dotnet test\` and \`npm test run\`
`;

      return {
        messages: [
          {
            role: "user" as const,
            content: {
              type: "text" as const,
              text: instructions,
            },
          },
        ],
      };
    },
  );
}
