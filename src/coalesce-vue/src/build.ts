import type { Plugin, ViteDevServer } from "vite";
import path from "path";
import { existsSync, readFileSync, writeFile } from "fs";
import { spawn } from "child_process";
import { TLSSocket } from "tls";
import { addHours } from "date-fns";
import MagicString from "magic-string";

import type * as https from "https";

export interface AspNetCoreHmrPluginOptions {
  /** The base path for vite when running with HMR.
   * Must correlate with `ViteServerOptions.PathBase` in aspnetcore. */
  base?: string;

  /** If true (default), will inject https key and cert from `dotnet dev-certs` */
  https?: boolean;

  /** If true (default), most relative imports will be rewritten to include the host
   * of the underlying Vite dev server, allowing these requests to bypass the
   * ASP.NET Core Server. This results in a significant performance boost when a debugger is attached to .NET.
   */
  assetBypass?: boolean;
}

/**
 * A plugin that works with IntelliTect.Coalesce.Vue.ViteDevelopmentServerMiddleware:
 * - Writes `index.html` to the build output directory during HMR development,
 *   allowing any transformations in HomeController.cs to work identically in both dev and prod.
 * - Shuts down the HMR server when the parent ASP.NET Core application shuts down, preventing process orphaning.
 * - Automatically obtains certs from `dotnet-devcerts` and injects them into the Vite configuration.
 */
export function createAspNetCoreHmrPlugin({
  base = "/vite_hmr/",
  https = true,
  assetBypass = true,
}: AspNetCoreHmrPluginOptions = {}) {
  // We are passed in the PID of the parent .NET process so that when it aborts,
  // we can shut ourselves down. Otherwise the vite server will end up orphaned.
  // Technique adopted from https://github.com/dotnet/aspnetcore/blob/v3.0.0/src/Middleware/NodeServices/src/Content/Node/entrypoint-http.js#L369-L395
  const parentPid = process.env.ASPNETCORE_VITE_PID;
  if (!parentPid) return;

  const plugins = <Plugin[]>[
    {
      name: "coalesce-vite-hmr",
      async config(config, env) {
        const server = (config.server ??= {});

        config.base = base;

        // The development server launched by UseViteDevelopmentServer must be HTTPS
        // to avoid issues with mixed content:
        if (https && server.https != false) {
          const httpsOptions = (server.https ??= {}) as https.ServerOptions;

          const { keyFilePath, certFilePath } = await getCertPaths();

          httpsOptions.key ??= readFileSync(keyFilePath);
          httpsOptions.cert ??= readFileSync(certFilePath);
        }
      },

      async configureServer(server) {
        // We are passed in the parent .NET process's PID so that when it aborts,
        // we can shut ourselves down. Otherwise the vite server will end up orphaned.
        // Technique adopted from https://github.com/dotnet/aspnetcore/blob/v3.0.0/src/Middleware/NodeServices/src/Content/Node/entrypoint-http.js#L369-L395

        setInterval(async function () {
          let parentExists = true;
          try {
            // Sending signal 0 - on all platforms - tests whether the process exists. As long as it doesn't
            // throw, that means it does exist.
            process.kill(+parentPid, 0);
            parentExists = true;
          } catch (ex) {
            // If the reason for the error is that we don't have permission to ask about this process,
            // report that as a separate problem.
            if ((ex as any).code === "EPERM") {
              throw new Error(
                `Attempted to check whether process ${parentPid} was running, but got a permissions error.`
              );
            }
            parentExists = false;
          }

          if (!parentExists) {
            try {
              await server.close();
            } finally {
              process.exit(0);
            }
          }
        }, 1000);

        // Write the index.html file once on startup so it can be picked up immediately by aspnetcore.
        writeHtml(server);
      },

      async handleHotUpdate(ctx) {
        if (ctx.server.config.root + "/index.html" == ctx.file) {
          // Rewrite the index.html file whenever it changes.
          writeHtml(ctx.server);
        }
      },
    },
  ];

  if (assetBypass) {
    let port: number | undefined;
    let base: string;
    let https = true;

    plugins.push({
      name: "coalesce-vite-hmr-bypass",
      configResolved(config) {
        const plugins = config.plugins as any[];
        // HACK: Forcibly move this plugin to the end, after the built-in importAnalysisPlugin
        // Even if we used `enforce: "post"`, we otherwise wouldn't be able to run late enough.
        plugins.push(...plugins.splice(plugins.indexOf(this), 1));
        port = config.server.port;
        base = config.base;
        https = !!config.server.https
      },
      transform(code, id) {
        // Search for strings like:
        // - `"/vite_hmr/..."` (double quote import in js, etc)
        // - `'/vite_hmr/...'` (single quote import in js, etc)
        // - `=/vite_hmr/...` (HTML src attibutes)
        const regex = new RegExp(`(["'=])(${escapeRegex(base)})`, 'g');
        let s: MagicString | undefined;
        let match;
        while ((match = regex.exec(code))) {
          s ??= new MagicString(code);
          // Insert the hostname of the vite server at the start of the relative path
          // so that the request will go directly to the vite server
          // rather than having to traverse the aspnetcore server proxy first.
          s.appendLeft(
            match.index + match[1].length,
            `http${https ? 's' : ''}://localhost:${port}`
          );
        }
        return s
          ? {
              code: s.toString(),
              // Including sourcemaps is generating frivolous warnings
              // about missing sources. Maybe revisit for Vite3?
              //map: s.generateMap({ hires: true }),
            }
          : undefined;
      },
    });
  }

  return plugins;
}

function escapeRegex(string: string) {
  return string.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
}

/** Write the index.html file to the server's web root so that it can be
 * picked up normally as the fallback file during development in the same
 * way that happens in production. */
async function writeHtml(server: ViteDevServer) {
  const filename = server.config.root + "/index.html";
  if (existsSync(filename)) {
    let html = readFileSync(filename, "utf-8");
    html = await server.transformIndexHtml("/index.html", html, "/");
    writeFile(
      path.join(server.config.root, server.config.build.outDir, "index.html"),
      html,
      "utf-8",
      () => {
        /*nothing*/
      }
    );
    server.config.logger.info(
      `Wrote index.html to ${server.config.build.outDir}`
    );
  }
}

/** Use the dotnet CLI's `dev-certs` tool to create certs to use for the vite server.
 * The vite server needs to run as HTTPS so that if the aspnetcore server is also HTTPS,
 * the browser isn't trying to connect to the HMR server's websocket endpoint as mixed content (which fails).
 */
export async function getCertPaths(certName?: string) {
  // Technique is adapted from MSFT's SPA templates.
  // https://github.com/dotnet/spa-templates/blob/800ef5837e1a23da863001d2448df67ec31ce2a2/src/content/Angular-CSharp/ClientApp/aspnetcore-https.js

  const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ""
      ? `${process.env.APPDATA}/ASP.NET/https`
      : `${process.env.HOME}/.aspnet/https`;

  const certificateArg =
    certName ??
    process.argv
      .map((arg) => arg.match(/--name=(?<value>.+)/i))
      .filter(Boolean)[0]?.groups?.value;

  const certificateName = certificateArg || process.env.npm_package_name;

  if (!certificateName) {
    console.error(
      "getCertPaths: Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly."
    );
    process.exit(-1);
  }

  const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
  const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

  let valid = existsSync(certFilePath) && existsSync(keyFilePath);

  if (valid) {
    // The certs exist. Check that they're not expired.

    // Passing null to TLSSocket here doesn't seem to cause any errors.
    // Since we're not actually communicating over the socket, no reason to provide a real stream.
    const socket = new TLSSocket(null as any, {
      cert: readFileSync(certFilePath),
    });
    const cert = socket.getCertificate();
    socket.destroy();

    if (
      cert &&
      "valid_to" in cert &&
      new Date(cert.valid_to) < addHours(new Date(), 4)
    ) {
      console.log(
        "Local certs are expired, or almost expired. Will regenerate."
      );
      valid = false;
    }
  }

  if (!valid) {
    console.log("Launching dotnet dev-certs to generate local certs");
    await new Promise((resolve) => {
      const proc = spawn(
        "dotnet",
        [
          "dev-certs",
          "https",
          "--export-path",
          certFilePath,
          "--format",
          "Pem",
          "--no-password",
        ],
        { stdio: "inherit" }
      );

      proc.on("exit", (code) => {
        if (code !== null && code !== 0) {
          resolve(code);
          console.log(`dotnet dev-certs exited with code ${code}`);
        } else {
          resolve(0);
        }
      });
    });
  }

  return {
    certFilePath,
    keyFilePath,
  };
}
