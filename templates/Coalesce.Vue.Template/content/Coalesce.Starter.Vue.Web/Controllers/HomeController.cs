using Coalesce.Starter.Vue.Data.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Coalesce.Starter.Vue.Web.Controllers;

public class HomeController() : Controller
{
    /// <summary>
    /// Spa route for vue-based parts of the app
    /// </summary>
    /// <remarks>
    /// Caching is prevented on this route because the served file contains
    /// the links to compiled js/css that include hashes in the filenames.
    /// </remarks>
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
    public async Task<IActionResult> Index(
        [FromServices] IConfiguration config,
        [FromServices] IWebHostEnvironment hostingEnvironment
    )
    {
#if Tenancy
        if (!User.HasTenant())
        {
            return RedirectToPage("/SelectTenant", new { ReturnUrl = Request.GetEncodedPathAndQuery() });
        }
#endif

        var fileInfo = hostingEnvironment.WebRootFileProvider.GetFileInfo("index.html");
        if (!fileInfo.Exists) return NotFound($"{hostingEnvironment.WebRootPath}/index.html was not found");

        using var reader = new StreamReader(fileInfo.CreateReadStream());
        string contents = await reader.ReadToEndAsync();

        // OPTIONAL: Inject settings or other variables into index.html here.
        // These will then be available as global variables in your Vue app.
        // Declare them as globals in env.d.ts.
        Dictionary<string, object?> globalVars = new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = hostingEnvironment.EnvironmentName,
#if AppInsights
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = config.GetConnectionString("AppInsights"),
#endif
        };

        string globalVariables = string.Join(';', globalVars.Select(v => $"{v.Key} = {JsonSerializer.Serialize(v.Value)}"));

        contents = contents.Replace("<head>", $"<head><script>{globalVariables}</script>");

        return Content(contents, "text/html");
    }
}
