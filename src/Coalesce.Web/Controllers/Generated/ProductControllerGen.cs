using Coalesce.Domain;
using Coalesce.Domain.External;
using Coalesce.Web;
using IntelliTect.Coalesce.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class ProductController
        : BaseViewController<Coalesce.Domain.Product, AppDbContext>
    {
        public ProductController() : base() { }

        [Authorize]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/Product/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/Product/Table.cshtml");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult TableEdit()
        {
            return IndexImplementation(true, @"~/Views/Generated/Product/Table.cshtml");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateEdit()
        {
            return CreateEditImplementation(@"~/Views/Generated/Product/CreateEdit.cshtml");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditorHtml(bool simple = false)
        {
            return EditorHtmlImplementation(simple);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Docs()
        {
            return DocsImplementation();
        }
    }
}