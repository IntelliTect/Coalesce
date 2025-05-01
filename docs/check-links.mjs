import { createServer } from "http";
import handler from "serve-handler";
import waitOn from "wait-on";
import { spawn } from "child_process";

function run(command, args, options = {}) {
  return new Promise((resolve, reject) => {
    const proc = spawn(command, args, {
      shell: true,
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

let server;

try {
  // Start static file server
  server = createServer((req, res) =>
    handler(req, res, { public: ".vitepress/dist" })
  );

  await new Promise((resolve) => server.listen(8087, resolve));

  // Wait for server to be ready
  await waitOn({ resources: ["http://localhost:8087"], timeout: 30000 });

  // Run linkcheck
  await run("npm", ["run", "linkcheck"]);

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
