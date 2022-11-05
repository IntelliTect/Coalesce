import type { Plugin, ResolvedConfig, ViteDevServer } from "vite";
import MagicString from "magic-string";

import path from "node:path";
import os from "node:os";
import { existsSync } from "node:fs";
import { readFile, writeFile, mkdir } from "node:fs/promises";
import { spawn } from "node:child_process";
import { TLSSocket } from "node:tls";

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

  /** If true (default), index.html will be written to the output path on disk as it updates.
   * This mirrors production behavior where index.html will be served from wwwroot.
   * However, during development, if the port of the HMR server ever changes, the copy on disk will briefly
   * contain the wrong port on app startup.
   */
  writeIndexHtmlToDisk?: boolean;

  /** If set, will override the hostname that is injected into index.html
   * and other assets when `assetBypass` is true. Can be configured to access the
   * local development app instance from a network computer, e.g. a phone or
   * tablet for mobile testing. */
  host?: string;
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
  writeIndexHtmlToDisk = true,
  host: configuredHost,
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

        // Make sure we can listen on `localhost`, which is what is
        // expected by ViteDevelopmentServerMiddleware.
        // Listening on any host also allows for "local remote" development,
        // e.g. pulling up the app on your phone to work on mobile interactions.
        server.host ??= "0.0.0.0";

        config.base = base;

        // The development server launched by UseViteDevelopmentServer must be HTTPS
        // if the aspnetcore server is HTTPs to avoid issues with mixed content:
        if (https && server.https != false) {
          const httpsOptions = (server.https ??= {}) as https.ServerOptions;

          const { keyFilePath, certFilePath } = await getCertPaths();

          httpsOptions.key ??= await readFile(keyFilePath);
          httpsOptions.cert ??= await readFile(certFilePath);
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
        if (writeIndexHtmlToDisk) {
          writeHtml(server);
        }
      },

      async handleHotUpdate(ctx) {
        if (
          writeIndexHtmlToDisk &&
          ctx.server.config.root + "/index.html" == ctx.file
        ) {
          // Rewrite the index.html file whenever it changes.
          writeHtml(ctx.server);
        }
      },
    },
  ];

  if (assetBypass) {
    let port: number | undefined;
    let base: string;
    let resolvedConfig: ResolvedConfig;

    function getOrigin() {
      const serverConfig = resolvedConfig.server;

      const host =
        configuredHost ||
        (typeof serverConfig.host == "string" && serverConfig.host == "0.0.0.0"
          ? undefined
          : serverConfig.host) ||
        "localhost";

      return `http${serverConfig.https ? "s" : ""}://${host}:${port}`;
    }

    function transformCode(code: string) {
      // Search for strings like:
      // - `"/vite_hmr/..."` (double quote import in js, etc)
      // - `'/vite_hmr/...'` (single quote import in js, etc)
      // - `=/vite_hmr/...` (HTML src attibutes)
      const regex = new RegExp(`(["'=])(${escapeRegex(base)})`, "g");
      let s: MagicString | undefined;
      let match;
      while ((match = regex.exec(code))) {
        s ??= new MagicString(code);
        // Insert the hostname of the vite server at the start of the relative path
        // so that the request will go directly to the vite server
        // rather than having to traverse the aspnetcore server proxy first.
        s.appendLeft(match.index + match[1].length, getOrigin());
      }

      return s;
    }

    function escapeRegex(string: string) {
      return string.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&");
    }

    plugins.unshift({
      name: "coalesce-vite-hmr-bypass-aspnetcore",
      enforce: "pre",
      configResolved(config) {
        resolvedConfig = config;
        // This might be wrong if no port was provided or if this port wasn't available.
        // Will get overridden with the real port below.
        port = config.server.port;
        base = config.base;
      },

      configureServer(server) {
        // Transform imports in index.html to go to the HMR server instead of the aspnetcore server.
        // Ideally we'd do this in the transformIndexHtml hook, but cant because of
        // https://github.com/vitejs/vite/issues/5851 (fix merged, but not yet released).
        // For this workaround, this must go before coalesce-vite-hmr so we can hook
        // transformIndexHtml before coalesce-vite-hmr attempts to read from it.
        const original = server.transformIndexHtml;
        server.transformIndexHtml = async function (...args) {
          const res = await original.apply(this, args);
          return transformCode(res)?.toString() ?? res;
        };

        // Transform assets (fonts, mainly) to go to the HMR server instead of the aspnetcore server.
        server.httpServer!.on("listening", () => {
          port = (server.httpServer!.address() as any)?.port;

          // Static assets are normally loaded relative to the browser's origin
          // since they're often originating from things like font rules in <style> tags.
          // This will repoint them at the HMR server.
          server.config.server.origin = getOrigin();
        });
      },
    });

    // Web worker instantiation MUST reference a URL against the aspnetcore server.
    // It cannot be loaded directly from the vite server (i.e. cannot bypass aspnetcore)
    // due to same origin rules. This covers cases like `import foo from ./file?worker`.
    // Vite generates the worker instantiations with an absolute host and port,
    // which we need to then undo and make relative to the current origin.
    plugins.push({
      name: "coalesce-vite-hmr-unbypass-workers",
      enforce: "post",

      transform(code, id) {
        return code.replace(
          // Look for:
          // * new Worker(new URL("https://
          // * new Worker("https://
          // where the URL is the vite server
          new RegExp(
            `(new\\s+(?:Shared)?Worker\\s*\\(\\s*(?:new\\s+URL\\s*\\(\\s*)?)(["'])https?://[^:]+:${port}${escapeRegex(
              base
            )}`,
            "g"
          ),
          "$1window.location.origin + $2" + base
        );
      },
    });

    // This should be unnecessary as long as all non-script assets obey `server.origin`
    // and all script assets are loaded as descendants of index.html.
    // This can be registered in any order because it will move itself to the end.
    // plugins.push({
    //   name: "coalesce-vite-hmr-bypass",
    //   configResolved(config) {
    //     const plugins = config.plugins as any[];
    //     // HACK: Forcibly move this plugin to the end, after the built-in importAnalysisPlugin
    //     // Even if we used `enforce: "post"`, we otherwise wouldn't be able to run late enough.
    //     // We have to run late because importAnalysisPlugin is what puts `base` in the import paths.
    //     plugins.push(...plugins.splice(plugins.indexOf(this), 1));
    //   },
    //   transform(code, id) {
    //     const s = transformCode(code);
    //     return s
    //       ? {
    //           code: s.toString(),
    //           // Including sourcemaps is generating frivolous warnings
    //           // about missing sources. Maybe revisit for Vite3?
    //           //map: s.generateMap({ hires: true }),
    //         }
    //       : undefined;
    //   },
    // });
  }

  return plugins;
}

/** Write the index.html file to the server's web root so that it can be
 * picked up normally as the fallback file during development in the same
 * way that happens in production. */
async function writeHtml(server: ViteDevServer) {
  const filename = server.config.root + "/index.html";
  if (existsSync(filename)) {
    let html = await readFile(filename, "utf-8");
    html = await server.transformIndexHtml("/index.html", html, "/");

    const targetDir = path.join(server.config.root, server.config.build.outDir);

    try {
      if (!existsSync(targetDir)) {
        await mkdir(targetDir);
      }

      await writeFile(path.join(targetDir, "index.html"), html, "utf-8");
      server.config.logger.info(`  Coalesce: Wrote index.html to ${targetDir}`);
    } catch (e) {
      server.config.logger.error(
        `  Coalesce: Error writing index.html to ${targetDir}: ${e}`
      );
    }
  }
}

/** Create a plugin that will apply transforms to work around
 * https://github.com/rollup/rollup/issues/4637, eliminating
 * errors in production caused by class names getting mangled by Rollup
 * (when those class names become component names via vue-class-component).
 * Not needed as of rollup 3.2.1.
 */
export function createClassNameFixerPlugin() {
  let command: string;
  return {
    name: "coalesce-vite-restore-class-names",
    enforce: "post",
    config(config, env) {
      // Auto set esbuild to keep names as well,
      // which is also an important part of making vue-class-component work.
      config.esbuild ??= {};
      config.esbuild.keepNames ??= true;
    },
    configResolved(config) {
      command = config.command;
    },
    transform(code, id, options) {
      // This part is only for https://github.com/rollup/rollup/issues/4637
      // This is only an issue when rollup is involved.
      if (command !== "build") return;

      if (id.endsWith(".ts") && code.includes("__decorateClass")) {
        const regex = /let (\w+) = class extends/g;
        let s: MagicString | undefined;
        let match;
        while ((match = regex.exec(code))) {
          s ??= new MagicString(code);
          // Insert the hostname of the vite server at the start of the relative path
          // so that the request will go directly to the vite server
          // rather than having to traverse the aspnetcore server proxy first.
          s.appendLeft(
            match.index + match[0].length - "extends".length,
            match[1] + " "
          );
        }

        return s
          ? {
              code: s.toString(),
              map: s.generateMap({ hires: true }),
            }
          : undefined;
      }
    },
  } as Plugin;
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
      cert: await readFile(certFilePath),
    });
    const cert = socket.getCertificate();
    socket.destroy();

    if (
      cert &&
      "valid_to" in cert &&
      // Expires within 4 hours
      (new Date(cert.valid_to).valueOf() - new Date().valueOf()) / 36e5 < 4
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
