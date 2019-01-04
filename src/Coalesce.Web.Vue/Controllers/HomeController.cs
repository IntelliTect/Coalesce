using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Coalesce.Web.Vue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {

            IFileProvider provider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
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
