using IntelliTect.Coalesce.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.TestArea.Controllers
{
    [Area("TestArea")]
    [Authorize]
    public partial class CaseProductController 
        : BaseViewController<CaseProduct, AppDbContext> 
    { 
        public CaseProductController() : base() { }

        [Authorize]
        public ActionResult Cards(){
            return IndexImplementation(false, @"~/Areas/TestArea/Views/Generated/CaseProduct/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Areas/TestArea/Views/Generated/CaseProduct/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Areas/TestArea/Views/Generated/CaseProduct/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Areas/TestArea/Views/Generated/CaseProduct/CreateEdit.cshtml");
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