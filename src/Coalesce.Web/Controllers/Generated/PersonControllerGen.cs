using Coalesce.Domain;
using Coalesce.Domain.External;
using Coalesce.Web;
using IntelliTect.Coalesce.Knockout.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class PersonController
        : BaseViewController<Coalesce.Domain.Person, AppDbContext>
    {
        public PersonController() : base() { }

        [AllowAnonymous]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/Person/Cards.cshtml");
        }

        [AllowAnonymous]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/Person/Table.cshtml");
        }


        [AllowAnonymous]
        public ActionResult TableEdit()
        {
            return IndexImplementation(true, @"~/Views/Generated/Person/Table.cshtml");
        }

        [AllowAnonymous]
        public ActionResult CreateEdit()
        {
            return CreateEditImplementation(@"~/Views/Generated/Person/CreateEdit.cshtml");
        }

        [AllowAnonymous]
        public ActionResult EditorHtml(bool simple = false)
        {
            return EditorHtmlImplementation(simple);
        }

        [AllowAnonymous]
        public ActionResult Docs()
        {
            return DocsImplementation();
        }
    }
}