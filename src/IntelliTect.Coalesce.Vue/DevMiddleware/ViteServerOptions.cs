using Microsoft.AspNetCore.Http;

namespace IntelliTect.Coalesce;

public class ViteServerOptions
{
    /// <summary>
    /// Whether the vite server is running with HTTPS.  
    /// Vite.config.js must be appropriately configured to match.
    /// </summary>
    public bool UseHttps { get; set; } = true;

    /// <summary>
    /// The path off of localhost that the development server is served from.
    /// This is used to distinguish requests that should be proxied from those that should not.
    /// Defaults to `/vite_hmr`.
    /// </summary>
    public PathString PathBase { get; set; } = "/vite_hmr";

    /// <summary>
    /// The name of the script in package.json's "scripts"s that will launch the server.
    /// </summary>
    public string NpmScriptName { get; set; } = "dev";

    /// <summary>
    /// The port to run the vite dev server on. If not set, a random port will be chosen.
    /// It is strongly recommended to choose a specific port so that features like auto-reconnect work correctly.
    /// </summary>
    public int? DevServerPort { get; set; }

    /// <summary>
    /// Regex to watch for in the stdout of the vite server that signifies
    /// that it is ready to start serving requests.
    /// A capture group can specify the actual port that the server chose to listen on.
    /// </summary>
    public string OutputOnReady { get; set; } = @"//(?:localhost|127\.\d+\.\d+\.\d+):([0-9]+)/";

    /// <summary>
    /// If true, requests with Accept: text/html will be delayed until the dev server
    /// has started up. Startup includes writing index.html to disk.
    /// </summary>
    public bool WaitForReady { get; set; } = true;

}
