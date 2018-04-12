using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Get()
        {
            return View();
        }

        public IActionResult Documentation()
        {
            return RedirectPermanent("https://coalesce.readthedocs.io/en/latest/");
        }
    }
}
