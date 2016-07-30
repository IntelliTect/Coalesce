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
    public partial class ProductController 
        : BaseViewController<Product, AppDbContext> 
    { 
        public ProductController() : base() { }

        [Authorize]
        public ActionResult Cards(){
            return IndexImplementation(false, @"~/Areas/TestArea/Views/Generated/Product/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Areas/TestArea/Views/Generated/Product/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Areas/TestArea/Views/Generated/Product/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Areas/TestArea/Views/Generated/Product/CreateEdit.cshtml");
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