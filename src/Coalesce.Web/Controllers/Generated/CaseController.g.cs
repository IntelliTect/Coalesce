using IntelliTect.Coalesce.Knockout.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class CaseController : BaseViewController<Coalesce.Domain.Case>
    {
        [AllowAnonymous]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/Case/Cards.cshtml");
        }

        [AllowAnonymous]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/Case/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit()
        {
            return IndexImplementation(true, @"~/Views/Generated/Case/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit()
        {
            return CreateEditImplementation(@"~/Views/Generated/Case/CreateEdit.cshtml");
        }

        [Authorize]
        public ActionResult EditorHtml(bool simple = false)
        {
            return EditorHtmlImplementation(simple);
        }

        [Authorize]
        public ActionResult Docs([FromServices] IHostingEnvironment hostingEnvironment)
        {
            return DocsImplementation(hostingEnvironment);
        }
    }
}