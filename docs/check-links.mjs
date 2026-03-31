import { createServer } from "http";
import { chmod, mkdir, access, rm, writeFile } from "fs/promises";
import { spawn } from "child_process";
import path from "path";
import handler from "serve-handler";
import waitOn from "wait-on";

const LINKCHECK_VERSION = "3.0.0";

function getLinkcheckPlatformInfo() {
  const { platform, arch } = process;
  if (platform === "darwin" && ["x64", "arm64"].includes(arch))
    return {
      filename: `linkcheck-${LINKCHECK_VERSION}-macos-${arch}.tar.gz`,
      bin: "linkcheck/linkcheck",
    };
  if (platform === "win32" && arch === "x64")
    return {
      filename: `linkcheck-${LINKCHECK_VERSION}-windows-${arch}.zip`,
      bin: "linkcheck/linkcheck.bat",
    };
  if (platform === "linux" && ["x64", "arm64"].includes(arch))
    return {
      filename: `linkcheck-${LINKCHECK_VERSION}-linux-${arch}.tar.gz`,
      bin: "linkcheck/linkcheck",
    };
  throw new Error(`Unsupported platform: ${platform} ${arch}`);
}

async function fileExists(filePath) {
  try {
    await access(filePath);
    return true;
  } catch {
    return false;
  }
}

async function extractTarGz(buffer, destDir) {
  const tarball = path.join(destDir, "_archive.tar.gz");
  await writeFile(tarball, buffer);
  await runProcess("tar", ["xzf", tarball, "-C", destDir]);
  await rm(tarball);
}

async function extractZip(buffer, destDir) {
  const zipPath = path.join(destDir, "_archive.zip");
  await writeFile(zipPath, buffer);
  // Use PowerShell on Windows to extract
  await runProcess("powershell", [
    "-NoProfile",
    "-Command",
    `Expand-Archive -Path '${zipPath}' -DestinationPath '${destDir}' -Force`,
  ]);
  await rm(zipPath);
}

async function downloadAndVerifyLinkcheck(destDir) {
  const info = getLinkcheckPlatformInfo();
  const binPath = path.join(destDir, info.bin);

  if (await fileExists(binPath)) {
    console.log(`linkcheck already downloaded at ${binPath}`);
    return binPath;
  }

  await mkdir(destDir, { recursive: true });

  const releaseUrl = `https://github.com/filiph/linkcheck/releases/download/${LINKCHECK_VERSION}/${info.filename}`;

  console.log(`Downloading linkcheck ${LINKCHECK_VERSION} from ${releaseUrl}`);

  // Download the release
  const response = await fetch(releaseUrl);
  if (!response.ok)
    throw new Error(
      `Failed to download linkcheck: ${response.status} ${response.statusText}`,
    );
  const buffer = Buffer.from(await response.arrayBuffer());

  // Extract
  if (info.filename.endsWith(".tar.gz")) {
    await extractTarGz(buffer, destDir);
  } else if (info.filename.endsWith(".zip")) {
    await extractZip(buffer, destDir);
  }

  // Make executable on non-Windows
  if (process.platform !== "win32") {
    await chmod(binPath, 0o755);
  }

  console.log(`linkcheck installed to ${binPath}`);
  return binPath;
}

function runProcess(command, args, options = {}) {
  return new Promise((resolve, reject) => {
    // On Windows, .bat files need cmd.exe /c to avoid shell:true deprecation
    const proc =
      process.platform === "win32" && command.endsWith(".bat")
        ? spawn("cmd.exe", ["/c", command, ...args], {
            stdio: "inherit",
            ...options,
          })
        : spawn(command, args, {
            stdio: "inherit",
            ...options,
          });
    proc.on("close", (code) => {
      code === 0
        ? resolve()
        : reject(new Error(`${command} failed with code ${code}`));
    });
  });
}

const vendorDir = path.join(
  path.dirname(new URL(import.meta.url).pathname.replace(/^\/(\w:)/, "$1")),
  "node_modules",
  `.linkcheck-${LINKCHECK_VERSION}`,
);

let server;

try {
  // Download linkcheck binary if needed
  const linkcheckBin = await downloadAndVerifyLinkcheck(vendorDir);

  // Start static file server
  server = createServer((req, res) =>
    handler(req, res, { public: ".vitepress/dist" }),
  );

  await new Promise((resolve) => server.listen(8087, resolve));

  // Wait for server to be ready
  await waitOn({ resources: ["http://localhost:8087"], timeout: 30000 });

  // Choose skip file based on environment
  const isCI =
    process.env.CI === "true" || process.env.GITHUB_ACTIONS === "true";
  const skipFile = isCI
    ? "./.vitepress/linkcheck-skip-file-ci.txt"
    : "./.vitepress/linkcheck-skip-file.txt";

  console.log(`Running linkcheck with skip file: ${skipFile} (CI: ${isCI})`);

  // Run linkcheck
  await runProcess(linkcheckBin, [
    "localhost:8087",
    "-e",
    "--skip-file",
    skipFile,
  ]);

  process.exit(0);
} catch (err) {
  console.error("Error:", err.message);
  process.exit(1);
} finally {
  if (server) {
    server.close(() => {
      console.log("Server closed.");
    });
  }
}
