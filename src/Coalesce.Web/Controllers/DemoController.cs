using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Coalesce.Web.Controllers
{
    public class DemoController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string demoUserRole = "", bool triggerReload = false)
        {
            if (!string.IsNullOrEmpty(demoUserRole) && (demoUserRole == "Admin" || demoUserRole == "User"))
            {
                HttpContext.Response.Cookies.Append("DemoUserRole", demoUserRole);
                // triggerReload is needed to force the user change to take effect in the current page context
                return new RedirectToActionResult("Index", "Demo", new { triggerReload = true });
            }
            if (triggerReload) return new RedirectToActionResult("Index", "Demo", null);

            ViewBag.CurrentRole = User.IsInRole("User") ? "User" : (User.IsInRole("Admin") ? "Admin" : "None");
            return View();
        }
    }
}
