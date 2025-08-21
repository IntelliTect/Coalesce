#!/usr/bin/env node

import { promises as fs } from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { spawn } from "child_process";
import { minimatch } from "minimatch";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectRoot = path.resolve(__dirname);
const distPath = path.resolve(projectRoot, "dist");
const templateSourcePath = path.resolve(
  projectRoot,
  "..",
  "..",
  "templates",
  "Coalesce.Vue.Template",
  "content",
);
const templateDestPath = path.resolve(projectRoot, "dist", "template");

async function main() {
  console.log("Starting MCP server build process...");

  try {
    // Clean build directory
    console.log("Cleaning dist directory...");
    await cleanDistDirectory();

    // Run TypeScript compilation
    console.log("Compiling TypeScript...");
    await runCommand("npm", ["run", "build:tsc-only"], { cwd: projectRoot });

    // Copy template files
    console.log("Copying template files...");
    await copyTemplateFiles();

    // Copy template.json configuration
    console.log("Copying template configuration...");
    await copyTemplateConfig();

    console.log("Build completed successfully!");
  } catch (error) {
    console.error("Build failed:", error);
    process.exit(1);
  }
}

async function runCommand(command, args, options = {}) {
  return new Promise((resolve, reject) => {
    const process = spawn(command, args, {
      stdio: "inherit",
      shell: true,
      ...options,
    });

    process.on("close", (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(
          new Error(
            `Command failed with exit code ${code}: ${command} ${args.join(" ")}`,
          ),
        );
      }
    });

    process.on("error", (error) => {
      reject(error);
    });
  });
}

async function cleanDistDirectory() {
  try {
    await fs.access(distPath);
    // Directory exists, remove it
    await fs.rm(distPath, { recursive: true, force: true });
    console.log(`   Cleaned dist directory: ${distPath}`);
  } catch (error) {
    // Directory doesn't exist, nothing to clean
    console.log(`   Build directory doesn't exist, skipping clean`);
  }
}

async function copyTemplateFiles() {
  // Ensure destination directory exists
  await fs.mkdir(templateDestPath, { recursive: true });

  // Copy all git-tracked source files
  await copyDirectory(templateSourcePath, templateDestPath, {
    excludePatterns: [
      "**/bin",
      "**/obj",
      "**/node_modules",
      "**/wwwroot",
      "**/.vs",
      "**/*.user",
      "**/*.g.cs",
      "**/*.g.ts",
      "**/debug.log",
      "**/appsettings.localhost.json",
      "**/*.lock.json",
    ],
  });

  console.log(
    `   Copied template files from ${templateSourcePath} to ${templateDestPath}`,
  );
}

async function copyTemplateConfig() {
  const configSource = path.join(
    templateSourcePath,
    ".template.config",
    "template.json",
  );
  const configDest = path.join(
    templateDestPath,
    ".template.config",
    "template.json",
  );

  // Ensure destination directory exists
  await fs.mkdir(path.dirname(configDest), { recursive: true });

  // Copy template.json
  await fs.copyFile(configSource, configDest);

  console.log(`   Copied template.json to ${configDest}`);
}

async function copyDirectory(src, dest, options = {}) {
  const { excludePatterns = [] } = options;

  await fs.mkdir(dest, { recursive: true });

  const entries = await fs.readdir(src, { withFileTypes: true });

  for (const entry of entries) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);
    const relativePath = path
      .relative(templateSourcePath, srcPath)
      .replace(/\\/g, "/");

    // Check if this path should be excluded
    const shouldExclude = excludePatterns.some((pattern) => {
      return minimatch(relativePath, pattern) || minimatch(entry.name, pattern);
    });

    if (shouldExclude) {
      continue;
    }

    if (entry.isDirectory()) {
      await copyDirectory(srcPath, destPath, options);
    } else {
      await fs.copyFile(srcPath, destPath);
    }
  }
}

// Run the build
main().catch(console.error);

export { main as buildMcpServer };
