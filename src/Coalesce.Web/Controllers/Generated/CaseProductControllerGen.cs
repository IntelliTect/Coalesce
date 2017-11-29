using IntelliTect.Coalesce.Knockout.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class CaseProductController
        : BaseViewController<Coalesce.Domain.CaseProduct, Coalesce.Domain.AppDbContext>
    {
        public CaseProductController() : base() { }

        [Authorize]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/CaseProduct/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/CaseProduct/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit()
        {
            return IndexImplementation(true, @"~/Views/Generated/CaseProduct/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit()
        {
            return CreateEditImplementation(@"~/Views/Generated/CaseProduct/CreateEdit.cshtml");
        }

        [Authorize]
        public ActionResult EditorHtml(bool simple = false)
        {
            return EditorHtmlImplementation(simple);
        }

        [Authorize]
        public ActionResult Docs()
        {
            return DocsImplementation();
        }
    }
}