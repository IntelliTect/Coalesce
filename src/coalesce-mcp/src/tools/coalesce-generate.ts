import { spawn } from "child_process";
import { promises as fs } from "fs";
import path from "path";
import { McpServer } from "tmcp";
import { z } from "zod";

type McpServerType = McpServer<any>;

async function runCoalesceGeneration(
  configPath: string,
): Promise<{ output: string; exitCode: number }> {
  // Read and parse the coalesce.json file
  const configContent = await fs.readFile(configPath, "utf8");
  const config = JSON.parse(configContent);

  // Get the web project configuration
  const webProject = config.webProject;
  if (!webProject) {
    throw new Error("No 'webProject' property found in coalesce.json");
  }

  // Get the project file path
  const projectFile = webProject.projectFile;
  if (!projectFile) {
    throw new Error(
      "No 'projectFile' property found in webProject configuration",
    );
  }

  // Resolve the project file path relative to the config file directory
  const configDir = path.dirname(configPath);
  const projectFilePath = path.isAbsolute(projectFile)
    ? projectFile
    : path.resolve(configDir, projectFile);

  // Get the web project directory (parent directory of the .csproj file)
  const webProjectPath = path.dirname(projectFilePath);

  // Get the absolute path to the config file
  const absoluteConfigPath = path.resolve(configPath);

  return new Promise((resolve, reject) => {
    const process = spawn("dotnet", ["coalesce", absoluteConfigPath], {
      cwd: webProjectPath,
      stdio: ["pipe", "pipe", "pipe"],
    });

    let stdout = "";
    let stderr = "";

    process.stdout?.on("data", (data) => {
      stdout += data.toString();
    });

    process.stderr?.on("data", (data) => {
      stderr += data.toString();
    });

    process.on("close", (code) => {
      const output = stdout + (stderr ? `\nErrors:\n${stderr}` : "");

      if (code === 0) {
        resolve({ output, exitCode: code });
      } else {
        reject(
          new Error(
            `Coalesce generation failed with exit code ${code}:\n${output}`,
          ),
        );
      }
    });

    process.on("error", (error) => {
      reject(
        new Error(`Failed to start Coalesce generation: ${error.message}`),
      );
    });
  });
}

// Register the generate coalesce code tool
export function registerCoalesceCodeGenTool(server: McpServerType) {
  server.tool(
    {
      name: "coalesce_generate",
      title: "Coalesce Code Generation",
      description:
        "Runs Coalesce code generation to regenerate web project *.g.* files from existing data project files.",
      schema: z.object({
        configPath: z.string().describe("Path to the configuration file"),
      }),
    },
    async ({ configPath }) => {
      if (!configPath) {
        throw new Error("configPath parameter is required");
      }

      try {
        // Run dotnet coalesce command with the config file
        const result = await runCoalesceGeneration(configPath);

        return {
          content: [
            {
              type: "text",
              text: `Coalesce code generation completed successfully.\n\nOutput:\n${result.output}`,
            },
          ],
        };
      } catch (error) {
        const errorMessage =
          error instanceof Error ? error.message : "Unknown error occurred";
        return {
          content: [
            {
              type: "text",
              text: `Failed to run Coalesce code generation: ${errorMessage}`,
            },
          ],
          isError: true,
        };
      }
    },
  );
}
