import type {
  HtmlTagDescriptor,
  Plugin,
  ResolvedConfig,
  ViteDevServer,
} from "vite";
import MagicString from "magic-string";

import * as path from "node:path";
import * as os from "node:os";
import * as fs from "node:fs";
import { readFile, writeFile, mkdir } from "node:fs/promises";
import { spawn, exec } from "node:child_process";
import { TLSSocket } from "node:tls";
import { promisify } from "node:util";
import type { AddressInfo, Server } from "node:net";

const execPromise = promisify(exec);

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

  /** If true (default), some code will be injected during development
   * that attempts to detect and suggest fixes for common browser configuration issues.
   */
  offerConfigurationSuggestions?: boolean;

  /** If true (default), will invoke `npm ls` on start to validate that actual installed packages
   * match the versions defined in package.json.
   */
  checkPackageVersions?: boolean;
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
  offerConfigurationSuggestions = true,
  checkPackageVersions = true,
}: AspNetCoreHmrPluginOptions = {}) {
  // We are passed in the PID of the parent .NET process so that when it aborts,
  // we can shut ourselves down. Otherwise the vite server will end up orphaned.
  // Technique adopted from https://github.com/dotnet/aspnetcore/blob/v3.0.0/src/Middleware/NodeServices/src/Content/Node/entrypoint-http.js#L369-L395
  const parentPid = process.env.ASPNETCORE_VITE_PID;
  if (!parentPid) return;

  let certsExportPromise: Promise<boolean> | undefined;
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
        if (
          https &&
          //@ts-expect-error boolean check backwards combat with vite<5
          server.https != false
        ) {
          const httpsOptions = (server.https ??= {}) as https.ServerOptions;

          const {
            keyFilePath,
            certFilePath,
            certsExportPromise: certsExport,
          } = await getCertPaths();
          certsExportPromise = certsExport;

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

        server.httpServer!.on("listening", () => {
          // Write the index.html file once on startup so it can be picked up immediately by aspnetcore.
          if (writeIndexHtmlToDisk) {
            writeHtml(server);
          }

          // Wait for dotnet dev-certs to finish exporting the ssl cert.
          // If it did export a new cert, restart the server so it can be used.
          // We wait for the server to start listening because if we do this too soon,
          // things blow up a little bit.
          certsExportPromise?.then((certsWereRegenerated) => {
            if (certsWereRegenerated) {
              console.log(
                "dotnet dev-certs produced a different cert than the previous cached cert. Restarting the vite server..."
              );
              setTimeout(() => {
                // Wait a bit of time because things explode if we do this too soon after listening starts.
                server.restart();
              }, 1000);
            }
          });
        });

        if (checkPackageVersions) {
          const packageVersions = (async () => {
            const nodeModules = getNpmDependencies("");
            const packageLock = getNpmDependencies("--package-lock-only");

            return [
              {
                description: "package-lock.json",
                results: await packageLock,
                resolution: "npm install",
              },
              {
                description: "node_modules",
                results: await nodeModules,
                resolution: "npm ci",
              },
            ];
          })().then((data) => {
            for (const results of data) {
              const packageProblems = Object.entries(
                results.results?.dependencies ?? {}
              )
                .map(([packageName, data]) => {
                  if (!data.invalid) return;

                  return `<tr>
                    <td>${escapeHTML(packageName)}</td>
                    <td>${data.invalid.replace(
                      " from the root project",
                      ""
                    )}</td>
                    <td>${escapeHTML(data.version)}</td></tr>`;
                })
                .filter((x) => x);

              if (packageProblems.length) {
                return {
                  wasSuccessful: false,
                  message:
                    `NPM packages in <b>${results.description}</b> don't match the versions in <b>package.json</b>. Stop the application and run <code>${results.resolution}</code>.<table class=packages-table><thead><tr>
                    <td>Package</td>
                    <td>package.json</td>
                    <td>${results.description}</td></tr></thead>` +
                    packageProblems.join("") +
                    " </table>",
                };
              }
            }
            return { wasSuccessful: true };
          });

          server.ws.on("coalesce:npm-check", async (data, client) => {
            const results = await packageVersions;
            client.send("coalesce:npm-check-result", results);
          });
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

      transformIndexHtml: {
        order: "pre",
        handler(html) {
          return {
            html,
            tags: [...(checkPackageVersions ? getPackageCheckTag() : [])],
          };
        },
      },
    },
  ];

  if (assetBypass) {
    plugins.push(
      ...createAssetBypassPlugins(configuredHost, offerConfigurationSuggestions)
    );
  }

  return plugins;
}

function createAssetBypassPlugins(
  configuredHost: string | undefined,
  offerConfigurationSuggestions: boolean
) {
  let port: number | undefined;
  let base: string;
  let resolvedConfig: ResolvedConfig;

  const plugins: Plugin<any>[] = [];

  function getNetworkAddresses() {
    return Object.values(os.networkInterfaces())
      .flatMap((nInterface) => nInterface ?? [])
      .filter(
        (detail) =>
          detail &&
          detail.address &&
          // @ts-ignore Node < v18
          ((typeof detail.family === "string" && detail.family === "IPv4") ||
            // @ts-ignore Node >= v18
            (typeof detail.family === "number" && detail.family === 4))
      )
      .map((detail) => detail.address)
      .filter((host) => !host.includes("127.0.0.1"));
  }

  function getViteOrigin() {
    const serverConfig = resolvedConfig.server;

    const host =
      configuredHost ||
      (typeof serverConfig.host == "string" && serverConfig.host == "0.0.0.0"
        ? undefined
        : serverConfig.host) ||
      // Cannot use the network address because it isn't a valid name for dotnet dev-certs' certs.
      //getNetworkAddresses()[0] ||
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
      s.appendLeft(match.index + match[1].length, getViteOrigin());
    }

    return s;
  }

  plugins.unshift({
    name: "coalesce-vite-hmr-bypass-aspnetcore",
    enforce: "pre",
    configResolved(config) {
      resolvedConfig = config;
      base = config.base;
    },

    transformIndexHtml(html) {
      // Transform imports in index.html to go to the HMR server instead of the aspnetcore server.
      html = transformCode(html)?.toString() || html;

      return {
        html,
        tags: [
          ...(offerConfigurationSuggestions
            ? getConfigurationSuggestionTag(
                getViteOrigin(),
                base,
                path.join(getHtmlTargetDir(resolvedConfig), "index.html")
              )
            : []),
        ],
      };
    },

    configureServer(server) {
      // Transform assets (fonts, mainly) to go to the HMR server instead of the aspnetcore server.
      server.httpServer!.on("listening", () => {
        port = (server.httpServer!.address() as any)?.port;

        // Static assets are normally loaded relative to the browser's origin
        // since they're often originating from things like font rules in <style> tags.
        // This will repoint them at the HMR server.
        server.config.server.origin = getViteOrigin();
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
        // * new Worker(new URL(/* @vite-ignore */"https://
        // * new Worker("https://
        // where the URL is the vite server
        new RegExp(
          `(new\\s+(?:Shared)?Worker\\s*\\(\\s*(?:new\\s+URL\\s*\\(\\s*(?:/\\* @vite-ignore \\*/)?\\s+)?)(?:''\\s*\\+\\s*)?(["'])https?://[^:]+:${port}${escapeRegex(
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

  return plugins;
}

function getPackageCheckTag(): HtmlTagDescriptor[] {
  return [
    {
      tag: "script",
      attrs: {
        type: "module",
        src: "/node_modules/coalesce-vue/src/plugin-npm-package-check-client.ts",
      },
      injectTo: "body",
    },
  ];
}
function getConfigurationSuggestionTag(
  viteOrigin: string,
  base: string,
  indexHtmlPath: string
) {
  return [
    {
      tag: "script",
      children: `
    // This code injected by coalesce-vue's createAspNetCoreHmrPlugin.
    // It can be disabled with setting 'offerConfigurationSuggestions: false'.
    // Detect if requests to the vite server are failing, and offer suggestions.
    
    if ("${viteOrigin}" == window.location.origin) {
      alert("Coalesce/Vite: You seem to be hitting the vite server directly, rather than the ASP.NET Core server. API calls probably won't work. \\n\\nIf you were only hitting this URL to add a TLS exception, disregard and just close this tab.")
    }

    fetch("${viteOrigin}${base}", {
      method: 'HEAD',
      cache: 'no-cache',
    }).catch((e) => {
      // The only reason that fetch throws is because of network errors,
      // so no need to look carefully at the message.

      var messages = [];
      if (window.location.protocol == "https:") {
        if (navigator.userAgent?.includes("Firefox") && !navigator.userAgent?.includes("Mobile")) {
          messages.push(\`This might be a <b>Firefox-specific</b> TLS error. Firefox does not use the system cert store by default. To resolve this, navigate to <code>about:config</code> and enable setting 
          '<code>security.enterprise_roots.enabled</code>' 
          (<a href="https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-7.0&tabs=visual-studio#configure-trust-of-https-certificate-using-firefox-browser">MS Docs</a>).\`)
        }
      
        messages.push(\`The TLS certificate of <code>${
          new URL(viteOrigin).host
        }</code> might not be trusted. Open <a href="${viteOrigin}${base}" target=_blank>this link</a> in a new tab and add an additional certificate exception if prompted. This is especially useful if this browser is on a different device than the dev server.\`)
      }

      const isSameHostname = "${
        new URL(viteOrigin).hostname
      }" == window.location.hostname;
      messages[!isSameHostname ? 'unshift' : 'push'](\`If the dev server is not routable from this browser using URL <code>${viteOrigin}</code>, pass either: <ul><li>  <code>{host: 'network-hostname-or-ip-of-dev-server'}</code> (better)</li> <li> <code>{assetBypass: false}</code> (slower)</li></ul> to <code>createAspNetCoreHmrPlugin()</code> in vite.config.ts. Don't commit this change because it will break other developers.\`)

      messages.push("You launched locally without <code>UseViteDevelopmentServer()</code>, or didn't build for production before deploying, and are therefore operating off a stale <code>${escapeHTML(
        indexHtmlPath
      )}</code> file. Or, the vite server crashed.")

      document.body.insertAdjacentHTML("beforeend", \`
      <div style="height: 100vh;
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        background: #333;"
      >
      <div style="
        margin: 30px auto;
        max-width: 700px;
        font-family: sans-serif;
        padding: 16px;
        font-size: 16px;
        background: #680b0b;
        z-index: 10000;
        color: #cfcfcf;"
      >
        <b>Coalesce/Vite</b>: Requests to the Vite server (${viteOrigin}) are failing due to network errors. 
        <br>
        <br>
        Consider the following potential solutions:
        <ol>\${messages.map(m => '<li style="padding-bottom: 16px">' + m + '</li>').join('')}</ol>
        </div></div>\`)
    })
          `,
    },
  ];
}

/** Write the index.html file to the server's web root so that it can be
 * picked up normally as the fallback file during development in the same
 * way that happens in production. */
function getHtmlTargetDir(config: ResolvedConfig) {
  return path.join(config.root, config.build.outDir);
}

async function writeHtml(server: ViteDevServer) {
  const config = server.config;

  const htmlSourceFileName = config.root + "/index.html";
  const targetDir = getHtmlTargetDir(config);

  fs.mkdirSync(targetDir, { recursive: true });

  // Copy static public assets too
  if (
    config.build.copyPublicDir &&
    config.publicDir &&
    fs.existsSync(config.publicDir)
  ) {
    copyDir(config.publicDir, targetDir);
  }

  if (fs.existsSync(htmlSourceFileName)) {
    let outputHtml = await readFile(htmlSourceFileName, "utf-8");
    outputHtml = await server.transformIndexHtml(
      "/index.html",
      outputHtml,
      "/"
    );

    try {
      await writeFile(path.join(targetDir, "index.html"), outputHtml, "utf-8");
      config.logger.info(`  Coalesce: Wrote index.html to ${targetDir}`);
    } catch (e) {
      config.logger.error(
        `  Coalesce: Error writing index.html to ${targetDir}: ${e}`
      );
    }
  }
}

function copyDir(srcDir: string, destDir: string): void {
  fs.mkdirSync(destDir, { recursive: true });
  for (const file of fs.readdirSync(srcDir)) {
    const srcFile = path.resolve(srcDir, file);
    if (srcFile === destDir) {
      continue;
    }
    const destFile = path.resolve(destDir, file);
    const stat = fs.statSync(srcFile);
    if (stat.isDirectory()) {
      copyDir(srcFile, destFile);
    } else {
      fs.copyFileSync(srcFile, destFile);
    }
  }
}

function escapeRegex(string: string) {
  return string.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&");
}

function escapeHTML(str: string) {
  return str.replace(
    /[&<>'"\\/]/g,
    (tag) =>
      ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        "'": "&#39;",
        '"': "&quot;",
        "\\": "&#92;",
        "/": "&#47;",
      }[tag]!)
  );
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

  let valid = fs.existsSync(certFilePath) && fs.existsSync(keyFilePath);
  let certContent;

  if (valid) {
    certContent = await readFile(certFilePath);

    // The certs exist. Check that they're not expired.
    // This is very fast, on the order of ~10ms.

    // Passing null to TLSSocket here doesn't seem to cause any errors.
    // Since we're not actually communicating over the socket, no reason to provide a real stream.
    try {
      const socket = new TLSSocket(null as any, {
        cert: certContent,
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
    } catch {
      console.log("Local certs may be corrupted. Will regenerate.");
      valid = false;
    }
  }

  const certsExportPromise = (async () => {
    // console.log("Launching dotnet dev-certs to get fresh copy of local cert");
    const start = new Date();
    const oldContent = certContent;

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
    // console.log("dotnet dev-certs completed in %dms", new Date().valueOf() - start.valueOf())

    const newContent = await readFile(certFilePath);
    if (!oldContent?.equals(newContent)) {
      // Signal that we produced a new cert
      return true;
    } else {
      // Signal that the existing cert was OK.
      return false;
    }
  })();

  if (valid) {
    // The existing stored certs are /suspected/ to be valid, but aren't guaranteed,
    // since their trust chain may be broken if the dotnet dev cert has since been regenerated.
    // Unfortunately, since the cert is stored in the windows store, we can't easily validate it here,
    // so instead, we'll always run dotnet dev-certs to re-export the cert, but do so in the background
    // so that in the 99% case, we don't have to wait for it.

    // console.log("tentatively using existing saved ssl cert")
    return {
      certFilePath,
      keyFilePath,
      certsExportPromise,
    };
  } else {
    // We know the stored cert isn't valid. Wait for it to regenerate before we return.
    await certsExportPromise;
  }

  return {
    certFilePath,
    keyFilePath,
  };
}

async function getNpmDependencies(args: string): Promise<
  | {
      problems?: string[];
      dependencies: {
        [packageName: string]: {
          version: string;
          invalid?: string;
          problem?: string[];
        };
      };
    }
  | undefined
> {
  const node = process.env.npm_node_execpath;
  const npm = process.env.npm_execpath;
  const packageJson = process.env.npm_package_json;

  if (!node || !npm || !packageJson) return;
  try {
    const { stdout } = await execPromise(
      `"${node}" "${npm}" ls --json ${args}`,
      {
        maxBuffer: 1024 * 1000,
        cwd: path.dirname(packageJson),
      }
    );
    return JSON.parse(stdout);
  } catch (e: any) {
    if (typeof e == "object" && "stdout" in e) {
      return JSON.parse(e.stdout);
    }
  }
}
