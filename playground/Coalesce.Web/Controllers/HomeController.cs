
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

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
            return Redirect("https://intellitect.github.io/Coalesce");
        }
    }
}
