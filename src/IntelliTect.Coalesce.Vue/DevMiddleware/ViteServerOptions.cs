using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
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
        /// Text to watch for in the stdout of the vite server that signifies
        /// that it is ready to start serving requests.
        /// </summary>
        public string OutputOnReady { get; set; } = "dev server running at";

        /// <summary>
        /// The path of the client app, relative to the .NET process' working directory.
        /// </summary>
        public string ClientAppPath { get; set; } = ".";
    }
}
