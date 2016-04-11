using Intellitect.ComponentModel.TypeDefinition;
using Intellitect.ComponentModel.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Intellitect.ComponentModel.Controllers
{
    public abstract class BaseViewController<T, TContext> : BaseViewController<T, T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        protected BaseViewController() : base() { }
    }

    public abstract class BaseViewController<T, TDto, TContext> : BaseControllerWithDb<TContext>
        where T : class, new()
        where TContext : DbContext
    {
        protected ClassViewModel classViewModel;

        /// <summary>
        /// Override this method to set the API name.
        /// </summary>
        protected virtual string ApiName
        {
            get { return ControllerName; }
        }


        protected BaseViewController()
        {
            classViewModel = ReflectionRepository.GetClassViewModel(typeof(T), this.GetType().Name, ApiName);
        }

        // Page Listing the items in the collection.
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult IndexImplementation(bool editable, string viewName = "~/Views/Api/Index.cshtml")
        {
            ViewBag.ParentIdName = null;
            ViewBag.ParentId = null;
            ViewBag.Editable = editable;
            ViewBag.Query = "";
            foreach (var kvp in Request.Query)
            {
                ViewBag.ParentIdName =  kvp.Key;
                ViewBag.ParentId = kvp.Value;
                ViewBag.Query = kvp.Key + "=" + kvp.Value;
            }
            @ViewBag.Title = typeof(T).Name + " List";
            var model = ReflectionRepository.GetClassViewModel(typeof(T), this.GetType().Name, ApiName);
            return View(viewName, model);
        }

        // GET: Editing page
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult CreateEditImplementation(string viewName = "~/Views/Api/CreateEdit.cshtml")
        {
            string id = "";
            ViewBag.ParentIdName = null;
            ViewBag.ParentId = null;

            foreach (var kvp in Request.Query)
            {
                if (kvp.Key == "id") id = kvp.Value;
                else if (kvp.Key != string.Empty)
                {
                    ViewBag.ParentIdName = classViewModel.PropertyByName(kvp.Key).JsVariable;
                    ViewBag.ParentId = kvp.Value;
                    ViewBag.Query = kvp.Key + "=" + kvp.Value;
                }
            }

            @ViewBag.Title = typeof(T).Name + " Edit";
            ViewBag.Id = id;
            return View(viewName, classViewModel);
        }

        /// <summary>
        /// Gets partial HTML content for editing this object.
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult EditorHtmlImplementation(bool simple = false)
        {
            ViewBag.SimpleEditorOnly = simple;
            return PartialView("~/Views/Shared/_EditorHtml.cshtml", classViewModel);
        }


        [HttpGet]
        //[OutputCache(Duration = 10000, VaryByParam = "none")]
        protected ActionResult DocsImplementation()
        {
            // Load TypeScript docs
            var path = Path.Combine(AppEnv.ApplicationBasePath, "Scripts", "Generated", $"ko.{classViewModel.ViewModelClassName}.ts");

            ViewBag.ObjDoc = GenerateTypeScriptDocs(path);

            path = Path.Combine(AppEnv.ApplicationBasePath, "Scripts", "Generated", $"ko.{classViewModel.ListViewModelClassName}.ts");

            ViewBag.ListDoc = GenerateTypeScriptDocs(path);

            return View("~/Views/Api/Docs.cshtml", classViewModel);
        }

        private TypeScriptDocumentation GenerateTypeScriptDocs(string path)
        {
            var doc = new TypeScriptDocumentation();
            var file = new FileInfo(path);

            // don't gen JSON documentation for definition files
            if (!file.FullName.EndsWith(".d.ts"))
            {
                var reader = file.OpenText();
                doc.TsFilename = file.Name;
                doc.Generate(reader.ReadToEnd());
            }

            return doc;
        }
    }
}