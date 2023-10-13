using IntelliTect.Coalesce.Knockout.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Coalesce.Web.Ko.Controllers
{
    [Authorize]
    public partial class ObjectChangePropertyController : BaseViewController<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty>
    {
        [Authorize]
        public ActionResult Cards()
        {
            return IndexImplementation(false, @"~/Views/Generated/ObjectChangeProperty/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table()
        {
            return IndexImplementation(false, @"~/Views/Generated/ObjectChangeProperty/Table.cshtml");
        }

    }
}
