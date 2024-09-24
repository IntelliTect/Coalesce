using Coalesce.Starter.Vue.Data.Auth;
#if AppInsights
using Microsoft.ApplicationInsights.AspNetCore;
#endif
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;

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
#if AppInsights
        [FromServices] JavaScriptSnippet appInsightsSnippet,
#endif
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
        // These will then be available as global variables in your Vue app:
        string headPrepend = $"""
        <script>
            ASPNETCORE_ENVIRONMENT="{JsEncode(hostingEnvironment.EnvironmentName)}"
        </script>
        """;

#if AppInsights
        if (appInsightsSnippet.FullScript.Length > 0)
        {
            headPrepend +=
                appInsightsSnippet.FullScript
                // Remove the automatic trackPageView event that is fired on load.
                // We fire our own page tracking events in router.ts to get better data.
                + "<script>window.appInsights.queue.pop()</script>";
        }
#endif

        contents = contents.Replace("<head>", "<head>" + headPrepend);

        return Content(contents, "text/html");

        static string JsEncode(string s) => HttpUtility.JavaScriptStringEncode(s);
    }
}
