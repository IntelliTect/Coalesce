using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Coalesce.Web.Vue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {

            IFileProvider provider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
            IFileInfo fileInfo = provider.GetFileInfo("index.html");
            if (!fileInfo.Exists)
            {
                return Ok("index.html not found. HMR build is probably still running for the first time. Keep refreshing...");
            }
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
