using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Intellitect.ComponentModel.Controllers;
using Coalesce.Domain;
using Microsoft.AspNetCore.Builder;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Coalesce.Web.Controllers
{
    public partial class CasesController : BaseViewController<Case, AppDbContext>
    {
        public CasesController() : base() { }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
