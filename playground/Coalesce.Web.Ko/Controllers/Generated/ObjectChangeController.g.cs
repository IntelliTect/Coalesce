using IntelliTect.Coalesce.Knockout.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Coalesce.Web.Ko.Controllers
{
    [Authorize]
    public partial class ObjectChangeController : BaseViewController<Coalesce.Domain.ObjectChange>
    {
        [Authorize]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/ObjectChange/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/ObjectChange/Table.cshtml");
        }

    }
}
