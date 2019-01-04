using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Coalesce.Web.Vue.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            IFileProvider provider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot"));
            IFileInfo fileInfo = provider.GetFileInfo("index.html");
            var readStream = fileInfo.CreateReadStream();

            return File(readStream, "text/html");
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
