# Vite Integration

Coalesce provides seamless integration between your ASP.NET Core backend and your Vue.js frontend using [Vite](https://vitejs.dev/), a modern build tool that offers fast development server with hot module replacement (HMR). This integration allows you to launch your entire application—frontend and backend—with a single `dotnet run` or Visual Studio "F5".

## Overview

The Vite integration consists of two main components, both of which are already setup by the [Coalesce project template](/stacks/vue/getting-started.md):

1. `UseViteDevelopmentServer` middleware that launches and proxies to the Vite dev server
2. `createAspNetCoreHmrPlugin` Vite plugin that optimizes the development experience

## .NET: UseViteDevelopmentServer

The `UseViteDevelopmentServer` ASP.NET Core middleware:

- **Launches Vite**: Automatically starts the Vite development server when your ASP.NET Core app starts, and restarts it after a crash
- **Port Management**: Finds an available port automatically or uses a specified port
- **Request Proxying**: Forwards requests to the Vite dev server as needed

### Configuration Options

```csharp
app.UseViteDevelopmentServer(options =>
{
    options.DevServerPort = 5002;  // Specific port (optional)
    options.NpmScriptName = "dev"; // npm script to run (default: "dev")
    options.UseHttps = true;       // Use HTTPS (default: true in dev)
    options.WaitForReady = true;   // Wait for server before serving HTML
});
```

## Vite: createAspNetCoreHmrPlugin

The `createAspNetCoreHmrPlugin` Vite plugin, imported from `coalesce-vue/lib/build`, provides several features in development (it does nothing when building for production):

- **Writes to Disk**: Copies `index.html` and public assets to the `wwwroot` directory during development to mirror production behavior
- **SSL Certificate Management**: Automatically obtains SSL certificates from `dotnet dev-certs` and configures the Vite server to use HTTPS
- **Asset Bypass**: Rewrites static asset URLs to point directly to the Vite server in development, bypassing ASP.NET Core for greatly improved page load time
- **Process Cleanup**: Automatically shuts down when the .NET process exits, preventing the Vite process from being orphaned
- **Configuration Suggestions**: Shows possible causes and solutions if the browser cannot reach the Vite development server
- **Package Version Validation**: Compares installed packages against `package.json` and warns about version mismatches with resolution suggestions

### Configuration Options

```typescript
createAspNetCoreHmrPlugin({
  base: "/vite_hmr/",                    // Base path for Vite HMR
  https: true,                           // Enable HTTPS (default: true)
  assetBypass: true,                     // Bypass ASP.NET for assets (default: true)
  writeIndexHtmlToDisk: true,            // Write index.html to disk (default: true)
  host: "localhost",                     // Override hostname for asset URLs
  offerConfigurationSuggestions: true,   // Show config suggestions (default: true)
  checkPackageVersions: true,            // Validate package versions (default: true)
});
```


## HomeController

### Server-Side Configuration Injection

The `HomeController.Index` action serves as the entry point for your SPA. Because `createAspNetCoreHmrPlugin` copies `index.html` to `wwwroot` even in development, we can utilize the exact same code here in both development and production. One of the main benefits of this is being able to inject server-driven configuration into index.html, avoiding an extra round-trip to a configuration API endpoint on every page load. For example, the implementation of `HomeController.Index` from the template:

```csharp
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Authorize]
public async Task<IActionResult> Index(
    [FromServices] IConfiguration config,
    [FromServices] IWebHostEnvironment hostingEnvironment
)
{
    var fileInfo = hostingEnvironment.WebRootFileProvider.GetFileInfo("index.html");
    if (!fileInfo.Exists) return NotFound($"{hostingEnvironment.WebRootPath}/index.html was not found");

    using var reader = new StreamReader(fileInfo.CreateReadStream());
    string contents = await reader.ReadToEndAsync();

    // Inject configuration as global variables
    Dictionary<string, object?> globalVars = new() {
        ["ASPNETCORE_ENVIRONMENT"] = hostingEnvironment.EnvironmentName,
        ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = config["APPLICATIONINSIGHTS_CONNECTION_STRING"],
        // Add your own configuration values here
    };

    string globalVariables = string.Join(';', globalVars.Select(v => 
        $"{v.Key} = {JsonSerializer.Serialize(v.Value)}"
    ));

    contents = contents.Replace("<head>", $"<head><script>{globalVariables}</script>");

    return Content(contents, "text/html");
}
```

### Frontend Consumption

To consume the injected configuration in your Vue app, declare the global variables in your TypeScript declarations:

```typescript
// src/types/env.d.ts
declare global {
  declare const ASPNETCORE_ENVIRONMENT: string;
  declare const APPLICATIONINSIGHTS_CONNECTION_STRING: string;
  // Add your own global variables here
}

export {};
```

## External Debugging

If you're accessing your local development site from another device (e.g. testing on an actual phone or tablet), you'll need to do the following:

- **Same network**: Have both your development machine and your test device on the same network, with that network not being locked down in a way that prevents devices from reaching each other.
- **Disable Asset Bypass**: Configure `createAspNetCoreHmrPlugin` with `assetBypass: false` so that all requests will go through your ASP.NET Core server.
- **Listen on `0.0.0.0`**: By default, your application's `launchSettings.json` is configured to listen on `localhost`. ASP.NET Core will bind to this specific name and will not accept connections that request any other hostname or IP address. To open this up, you need to add `0.0.0.0` to `applicationUrl` in `launchSettings.json` - for example, `"applicationUrl": "https://localhost:5001;https://0.0.0.0:5001"`
- **Open Firewall**: On Windows, you may be prompted automatically to allow the app through Windows Firewall. If you're not prompted and are unable to connect, you'll need to create a firewall rule to allow incoming connections. Open Windows Defender Firewall with Advanced Security (🪟 + R, "wf.msc"), click "Inbound Rules" → "New Rule", select "Port", choose "TCP" and enter your port number (e.g., 5001), then allow the connection and assign it a name.
- **Access Site**: Find your development machine's IP address or hostname on the local network (use `ipconfig` on Windows or `ifconfig` on macOS/Linux) and navigate to `https://[your-ip]:5001` from your test device. You will need to accept the SSL certificate warning on the test device.
