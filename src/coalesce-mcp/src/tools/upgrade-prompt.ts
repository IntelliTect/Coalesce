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

## Phase 1: Investigate

Gather all information needed to understand what's changed and what needs updating. Do NOT make any changes yet.

### Current State
Find and read the user's \`Directory.Build.props\` to get their current \`CoalesceVersion\`. Read their web project's \`package.json\` for NPM package versions. Read all \`.csproj\` files for NuGet packages and target frameworks.

Use \`read_template_file\` to read the template's \`Directory.Build.props\`, \`package.json\`, \`Web.csproj\`, and \`Data.csproj\` to determine the latest versions.

If the user is already on the latest version, tell them and stop. Let them know that if this is unexpected, their coalesce-mcp may be outdated.

### Changelog
Use the \`read_changelog\` tool with the user's current version as \`sinceVersion\`. Identify:
- Breaking changes that require code modifications
- Template changes that indicate configuration/pattern updates
- New opt-in features

### Dependencies
Compare ALL dependency versions between the template and the user's project:

- **.NET Target Framework** — Does the template target a newer .NET version?
- **Coalesce packages** — \`CoalesceVersion\` (NuGet), \`coalesce-vue\`, \`coalesce-vue-vuetify3\`, \`eslint-plugin-coalesce\` (NPM)
- **NuGet packages** — Compare template \`.csproj\` files against user's. Also scan ALL user \`.csproj\` files for Microsoft packages (\`Microsoft.EntityFrameworkCore.*\`, \`Microsoft.Extensions.*\`, \`Microsoft.AspNetCore.*\`, etc.) that follow the .NET release cadence — these should all be on the same version as each other.
- **NPM packages** — Packages the user already has that have newer versions in the template. New packages in the template that the user doesn't have. Packages removed from the template.

The template \`.csproj\` files contain \`#if\` directives for optional features — only consider packages the user actually has. Don't suggest adding packages only relevant to specific template features (e.g. \`@microsoft/applicationinsights-web\` is only for AppInsights).

### Configuration Files
Use \`read_template_file\` to compare these files against the user's versions and note meaningful differences:

- **vite.config.ts** — New plugins, build config changes, code splitting strategy
- **eslint.config.mjs** — New rules, plugin changes. Note if user is still on \`.eslintrc\`.
- **tsconfig.json**, **tsconfig.node.json**
- **pnpm-workspace.yaml** — overrides, allowBuilds, and other pnpm settings
- **.npmrc**
- **.editorconfig**
- **.vscode/settings.json**
- **.vscode/mcp.json**
- **AGENTS.md**
- **dependabot.yml**

Don't flag differences that are clearly intentional user customizations.

### Summary
Present a clear summary of everything found, organized by category.

If upgrading would cross a major version boundary (e.g. 5.x → 6.x), warn the user that major upgrades may require significant changes and suggest upgrading one major version at a time.

If the upgrade includes a Vuetify major version change (e.g. Vuetify 3 to 4), warn the user that the Vuetify upgrade is non-trivial. Suggest they follow the official Vuetify upgrade guide at https://vuetifyjs.com/en/getting-started/upgrade-guide/ alongside the Coalesce upgrade.

## Phase 2: Ask

Use \`vscode_askQuestions\` with \`multiSelect: true\` to let the user choose which significant changes to apply. If you don't have that tool or a similar alternative, ask with clearly numbered items in regular output.

Do NOT ask about:
- Coalesce package updates (always do these)
- Breaking changes from the changelog (always apply)
- Minor/patch version bumps of dependencies
- devDependency upgrades
- Routine NuGet package bumps that simply track .NET SDK versions

Only present options for decisions that meaningfully affect the project:
- .NET target framework upgrade (e.g. net9.0 → net10.0)
- Major version upgrades of app dependencies (e.g. Vuetify 3 → 4)
- New optional features from the changelog
- Significant configuration file changes (new files, structural changes — not minor tweaks)
- New packages from the template that the user doesn't have yet

Each option should have a brief description so the user can make informed choices. Inform the user that Coalesce packages, breaking changes, and routine dependency bumps will be applied automatically.

## Phase 3: Execute

Create a TODO for EACH individual item selected by the user (not just the high-level categories — one TODO per package, per breaking change, per config file, etc.). Then work through them:

1. Update all selected dependency versions (Coalesce packages, .NET framework, NuGet, NPM)
2. Apply selected configuration file changes
3. Address breaking changes identified in the changelog
4. Mark each TODO complete as it's resolved

### Validation

After all changes are made, run these commands in order, fixing errors before proceeding (if the project uses pnpm, use it in place of npm):

1. \`npm install\` in the web project. If it fails due to a recently-published package being blocked by npm's minimum release age policy, offer to re-run with \`--min-release-age=0\` after explaining the risk. Always check for warnings in the install output.
2. \`dotnet restore\` in the solution root
3. Run Coalesce code generation with \`dotnet coalesce\` in the web project
4. \`dotnet build\` to verify .NET compilation
5. \`npm run build\` in the web project
6. \`npm run lint\` in the web project — fix all warnings and errors
7. If tests exist, run \`dotnet test\` and \`npm test run\`
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
