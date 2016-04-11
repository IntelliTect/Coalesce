
using Intellitect.ComponentModel.Controllers;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.PlatformAbstractions;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class ProductController 
        : BaseViewController<Product, AppContext> 
    { 
        public ProductController() : base() { }

        [Authorize]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Views/Generated/Product/Table.cshtml");
        }

        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Views/Generated/Product/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Views/Generated/Product/CreateEdit.cshtml");
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