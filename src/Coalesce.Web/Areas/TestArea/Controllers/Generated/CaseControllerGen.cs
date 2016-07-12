
using Intellitect.ComponentModel.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.PlatformAbstractions;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.TestArea.Controllers
{
    [Area("TestArea")]
    [Authorize]
    public partial class CaseController 
        : BaseViewController<Case, AppDbContext> 
    { 
        public CaseController() : base() { }

        [Authorize]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Areas/TestArea/Views/Generated/Case/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Areas/TestArea/Views/Generated/Case/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Areas/TestArea/Views/Generated/Case/CreateEdit.cshtml");
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