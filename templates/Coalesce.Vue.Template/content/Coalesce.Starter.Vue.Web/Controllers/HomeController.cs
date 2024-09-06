using Coalesce.Starter.Vue.Data.Models;
#if AppInsights
using Microsoft.ApplicationInsights.AspNetCore;
#endif
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Starter.Vue.Web.Controllers;

public class HomeController(
#if Identity
    SignInManager<User> signInManager
#endif
) : Controller
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
#if AppInsights
        [FromServices] JavaScriptSnippet appInsightsSnippet,
#endif
        [FromServices] IWebHostEnvironment hostingEnvironment
    )
    {
        var fileInfo = hostingEnvironment.WebRootFileProvider.GetFileInfo("index.html");
        if (!fileInfo.Exists) return NotFound($"{hostingEnvironment.WebRootPath}/index.html was not found");

        using var reader = new StreamReader(fileInfo.CreateReadStream());
        string contents = await reader.ReadToEndAsync();

        contents = contents.Replace("<head>", "<head>"
#if AppInsights
            + appInsightsSnippet.FullScript
            // Remove the automatic trackPageView event that is fired on load.
            // We fire our own page tracking events in router.ts to get better data.
            + "<script>window.appInsights.queue.pop()</script>"

#endif
            // OPTIONAL: Inject settings or other variables into index.html here.
            // These will then be available as global variables in your Vue app:
            + $"""
            <script>
                ASPNETCORE_ENVIRONMENT="{hostingEnvironment.EnvironmentName}"
            </script>
            """);

        return Content(contents, "text/html");
    }

#if Identity
    [HttpGet]
    public async new Task<ActionResult> SignOut()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }
#endif
}
